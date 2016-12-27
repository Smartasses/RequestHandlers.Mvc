using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using RequestHandlers.Http;

namespace RequestHandlers.Mvc.CSharp
{
    class CSharpBuilder : IControllerAssemblyBuilder
    {
        private readonly HashSet<string> _classNames;
        private readonly string _assemblyName;

        public CSharpBuilder(string assemblyName)
        {
            _classNames = new HashSet<string>();
            _assemblyName = assemblyName;
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

            foreach (var temp in definitions)
            {
                var sb = new StringBuilder();
                foreach (var line in CreateCSharp(GetClassName(temp.Definition.RequestType), temp))
                    sb.AppendLine(line);
                var csharp = sb.ToString();
                compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(csharp));
            }

            var assemblyStream = new MemoryStream();
            var result = compilation.Emit(assemblyStream);
            if (!result.Success)
            {
                var errormsg = new StringBuilder();
                foreach (var diagnostic in result.Diagnostics)
                {
                    errormsg.AppendLine(diagnostic.ToString());
                }
                throw new Exception(errormsg.ToString());
            }
            assemblyStream.Seek(0, SeekOrigin.Begin);
            return AssemblyLoadContext.Default.LoadFromStream(assemblyStream);
        }

        private string GetClassName(Type requestType)
        {
            var name = requestType.Name;
            string className;
            int? addition = null;
            do
            {
                var add = addition.HasValue ? addition.ToString() : "";
                className = $"{name}Handler{add}Controller";
                addition = addition + 1 ?? 2;
            } while (_classNames.Contains(className));
            return className;
        }
        public IEnumerable<string> CreateCSharp(string className, HttpRequestHandlerDefinition builderDefinition)
        {
            yield return "namespace Proxy";
            yield return "{";

            var requestBodyProperties = builderDefinition.Parameters.Where(x => x.BindingType == BindingType.FromBody || x.BindingType == BindingType.FromForm).ToArray();
            var requestClass = builderDefinition.Definition.RequestType.Name + "_" + Guid.NewGuid().ToString().Replace("-", "");
            if (requestBodyProperties.Any())
            {
                yield return $"    public class {requestClass}";
                yield return "    {";
                foreach (var source in requestBodyProperties)
                {
                    yield return $"        public {source.PropertyInfo.PropertyType.FullName} {source.PropertyInfo.Name} {{ get; set; }}";
                }
                yield return "    }";
            }

            var methodArgs = string.Join(",  ", builderDefinition.Parameters.GroupBy(x => x.PropertyName).Select(x => new
            {
                Name = x.Key,
                Type = x.First().BindingType == BindingType.FromBody || x.First().BindingType == BindingType.FromForm ? requestClass : x.First().PropertyInfo.PropertyType.FullName,
                Binder = x.First().BindingType
            }).Select(x => $"[Microsoft.AspNetCore.Mvc.{x.Binder}Attribute] {x.Type} {x.Name}"));

            yield return $"    public class {className} : Microsoft.AspNetCore.Mvc.Controller";
            yield return "    {";
            yield return "        private readonly RequestHandlers.IRequestDispatcher _requestDispatcher;";
            yield return "";
            yield return $"        public {className}(RequestHandlers.IRequestDispatcher requestDispatcher)";
            yield return "        {";
            yield return "            _requestDispatcher = requestDispatcher;";
            yield return "        }";
            yield return $"        [Microsoft.AspNetCore.Mvc.Http{builderDefinition.HttpMethod}Attribute(\"{builderDefinition.Route}\")]";
            yield return $"        public {builderDefinition.Definition.ResponseType.FullName} Handle({methodArgs})";
            yield return "        {";
            var requestVariable = "request_" + Guid.NewGuid().ToString().Replace("-", "");
            yield return $"            var {requestVariable} = new {builderDefinition.Definition.RequestType.FullName}";
            yield return "            {";
            foreach (var assignment in builderDefinition.Parameters)
            {
                var fromRequest = assignment.BindingType == BindingType.FromBody || assignment.BindingType == BindingType.FromForm;
                yield return $"                {assignment.PropertyInfo.Name} = {assignment.PropertyName}{(fromRequest ? $".{assignment.PropertyInfo.Name}" : "")},";
            }
            yield return "            };";
            yield return $"            var response = _requestDispatcher.Process<{builderDefinition.Definition.RequestType.FullName},{builderDefinition.Definition.ResponseType.FullName}>({requestVariable});";
            yield return "            return response;";
            yield return "        }";
            yield return "    }";
            yield return "}";


        }
    }
}