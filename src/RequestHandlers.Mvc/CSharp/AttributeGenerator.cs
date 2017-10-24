using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RequestHandlers.Mvc.CSharp
{
    internal class AttributeGenerator
    {
        public string Generate(CustomAttributeData attributeData)
        {
            var sb = new StringBuilder();
            sb.Append('[');
            sb.Append(attributeData.AttributeType.FullName);
            if (attributeData.ConstructorArguments.Any() || attributeData.NamedArguments.Any())
            {
                AppendConstructor(attributeData.ConstructorArguments, attributeData.NamedArguments, sb);
            }
            sb.Append(']');
            return sb.ToString();
        }

        private void AppendConstructor(IEnumerable<CustomAttributeTypedArgument> constructorArguments, IEnumerable<CustomAttributeNamedArgument> namedArguments, StringBuilder builder)
        {
            builder.Append('(');
            var arguments = constructorArguments
                .Select(x => GenerateConstructorArgument(x))
                .Concat(namedArguments.Select(x => GenerateNamedArgument(x)));
            builder.Append(string.Join(", ", arguments));
            builder.Append(')');
        }

        private string GenerateConstructorArgument(CustomAttributeTypedArgument constructorArgument)
        {
            return ObjectToCodeString(constructorArgument.ArgumentType, constructorArgument.Value);
        }

        private string GenerateNamedArgument(CustomAttributeNamedArgument namedArgument)
        {
            return $"{namedArgument.MemberName} = {ObjectToCodeString(namedArgument.TypedValue.ArgumentType, namedArgument.TypedValue.Value)}";
        }

        private static string ObjectToCodeString(Type type, object value)
        {
            if (type == typeof(string))
            {
                return $"\"{value}\"";
            }

            if (type == typeof(bool))
            {
                return (bool)value ? "true" : "false";
            }

            if (type.GetTypeInfo().IsEnum)
            {
                return type.FullName + "." + Enum.GetName(type, value);
            }

            return value.ToString();
        }
    }
}