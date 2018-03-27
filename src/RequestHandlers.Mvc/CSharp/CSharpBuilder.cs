using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RequestHandlers.Http;

namespace RequestHandlers.Mvc.CSharp
{
    public class CSharpBuilder : IControllerAssemblyBuilder
    {
        private readonly HashSet<string> _classNames;
        private readonly string _assemblyName;
        private readonly string _saveToFilePath;
        private readonly AttributeGenerator _attributeGenerator;

        public CSharpBuilder(string assemblyName, string saveToFilePath = null)
        {
            _classNames = new HashSet<string>();
            _assemblyName = assemblyName;
            _saveToFilePath = saveToFilePath;
            _attributeGenerator = new AttributeGenerator();
        }

        public Assembly Build(HttpRequestHandlerDefinition[] definitions)
        {
            var references = new AssemblyReferencesHelper()
                .AddReferenceForTypes(typeof(object), typeof(Controller), typeof(RequestHandlerControllerBuilder))
                .AddReferenceForTypes(definitions.SelectMany(x => new[] { x.Definition.RequestType, x.Definition.ResponseType }).ToArray())
                .GetReferences();

            var compilation = CSharpCompilation.Create(_assemblyName)
                .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references);
            var uniqueClassNames = new HashSet<string>
            {
                "ProxyController"
            };
            var operationResults = definitions.Select(temp => CreateCSharp(GetOperationName(temp.Definition.RequestType), temp, uniqueClassNames)).ToArray();
            var files = new Dictionary<string, string>();
            files.Add("ProxyController", $@"namespace Proxy
{{
    public class ProxyController : {GetCorrectFormat(typeof(Controller))}
    {{
        private readonly {GetCorrectFormat(typeof(IWebRequestProcessor))} _requestProcessor;
        public ProxyController({GetCorrectFormat(typeof(IWebRequestProcessor))} requestProcessor)
        {{
            _requestProcessor = requestProcessor;
        }}

    {CodeStr.Foreach(operationResults.SelectMany(x => x.Operation.Split(new[] { Environment.NewLine }, StringSplitOptions.None)), operation => $@"
        {operation}")}
    }}
}}");
            foreach (var operationResult in operationResults)
            {
                files.Add(operationResult.OperationName, operationResult.RequestClass);
            }
            foreach (var temp in files)
            {
                compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(temp.Value));
            }
/*
            var parameters = new CompilerParameters();
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;*/
            var saveToFile = !string.IsNullOrEmpty(_saveToFilePath);
            var assemblyStream = new MemoryStream();
            var result = saveToFile ? compilation.Emit(_saveToFilePath) : compilation.Emit(assemblyStream);
            if (!result.Success)
            {
                var errormsg = new StringBuilder();
                foreach (var diagnostic in result.Diagnostics)
                {
                    errormsg.AppendLine(diagnostic.ToString());
                }
                throw new Exception(errormsg.ToString());
            }

            if (saveToFile)
            {
                return AssemblyLoadContext.Default.LoadFromAssemblyPath(_saveToFilePath);
            }
            else
            {
                assemblyStream.Seek(0, SeekOrigin.Begin);
                return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
            }
        }

        private string GetOperationName(Type requestType)
        {
            var name = requestType.Name;
            string className;
            int? addition = null;
            do
            {
                var add = addition.HasValue ? addition.ToString() : "";
                className = $"{name}Handler{add}";
                addition = addition + 1 ?? 2;
            } while (_classNames.Contains(className));
            return className;
        }
        public OperationResult CreateCSharp(string operationName, HttpRequestHandlerDefinition builderDefinition,
            HashSet<string> uniqueClassNames)
        {
            var requestBodyProperties = builderDefinition.Parameters.Where(x => x.BindingType == BindingType.FromBody || x.BindingType == BindingType.FromForm).ToArray();
            var requestClass = builderDefinition.Definition.RequestType.Name;
            var original = requestClass;
            var tryCount = 1;
            while (uniqueClassNames.Contains(requestClass))
            {
                requestClass = $"{original}_{++tryCount}";
            }

            var methodArgs = string.Join(",  ", builderDefinition.Parameters.GroupBy(x => x.PropertyName).Select(x => new
            {
                Name = x.Key,
                Type = x.First().BindingType == BindingType.FromBody || x.First().BindingType == BindingType.FromForm ? requestClass : GetCorrectFormat(x.First().PropertyInfo.PropertyType),
                Binder = x.First().BindingType
            }).Select(x => $"[Microsoft.AspNetCore.Mvc.{x.Binder}Attribute] {x.Type} {x.Name}"));
            var isAsync = builderDefinition.Definition.ResponseType.IsConstructedGenericType &&
                          builderDefinition.Definition.ResponseType.GetGenericTypeDefinition() == typeof(Task<>);
            var responseType = isAsync
                ? builderDefinition.Definition.ResponseType.GetGenericArguments()[0]
                : builderDefinition.Definition.ResponseType;
            var requestVariable = "request_" + Guid.NewGuid().ToString().Replace("-", "");
            var operationResult = new OperationResult();
            operationResult.OperationName = operationName;
            operationResult.RequestClass = $@"public class {requestClass}
{{{CodeStr.Foreach(requestBodyProperties, source => $@"
    public {GetCorrectFormat(source.PropertyInfo.PropertyType)} {source.PropertyInfo.Name} {{ get; set; }}")}
}}";
            operationResult.Operation = $@"{GetAttributes(builderDefinition.Definition)}[Microsoft.AspNetCore.Mvc.Http{builderDefinition.HttpMethod}Attribute(""{builderDefinition.Route}""), {GetCorrectFormat(typeof(ProducesAttribute))}(typeof({GetCorrectFormat(responseType)}))]
public  {(isAsync ? "async " : string.Empty)}{GetCorrectFormat(isAsync ? typeof(Task<IActionResult>) : typeof(IActionResult))} {operationName}({methodArgs})
{{
    var {requestVariable} = new {GetCorrectFormat(builderDefinition.Definition.RequestType)}
    {{{CodeStr.Foreach(builderDefinition.Parameters, assignment => $@"
        {assignment.PropertyInfo.Name} = {assignment.PropertyName}{(assignment.BindingType == BindingType.FromBody || assignment.BindingType == BindingType.FromForm ? $".{assignment.PropertyInfo.Name}" : "")},").Trim(',')}
    }};

    var response = {(isAsync ? "await " : string.Empty)}_requestProcessor.Process{(isAsync ? "Async" : string.Empty)}<{GetCorrectFormat(builderDefinition.Definition.RequestType)},{GetCorrectFormat(responseType)}>({requestVariable}, this);
    return response;
}}";
            return operationResult;
        }

        private string GetAttributes(IRequestDefinition requestDefinition)
        {
            return requestDefinition is RequestHandlerDefinition requestHandlerDefinition
                ? GetAttributes(requestHandlerDefinition)
                : string.Empty;
        }

        private string GetAttributes(RequestHandlerDefinition requestHandlerDefinition)
        {
            var attributes = requestHandlerDefinition.RequestHandlerType.GetTypeInfo()
                .CustomAttributes
                .Select(x => _attributeGenerator.Generate(x)).Append(string.Empty);
            return string.Join(Environment.NewLine, attributes);
        }

        private string GetCorrectFormat(Type type)
        {
            if (type.IsArray)
            {
                return GetCorrectFormat(type.GetElementType()) + "[]";
            }
            if (type.IsConstructedGenericType)
                return string.Format("{0}<{1}>", type.FullName.Split('`')[0], string.Join(", ", type.GetGenericArguments().Select(GetCorrectFormat)));
            else
                return type.FullName;
        }
    }
    public class OperationResult
    {
        public string OperationName { get; set; }
        public string RequestClass { get; set; }
        public string Operation { get; set; }
    }
    static class CodeStr
    {
        public static string Foreach<T>(IEnumerable<T> source, Func<T, string> format)
        {
            var sb = new StringBuilder();
            foreach (var item in source)
            {
                sb.Append(format(item));
            }
            return sb.ToString();
        }
        public static string If(bool value, string ifTrue, string ifFalse = "") => value ? ifTrue : ifFalse;
        public static string Wrap(Func<string> action) => action();
    }
}