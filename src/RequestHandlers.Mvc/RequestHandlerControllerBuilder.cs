using System.Linq;
using System.Reflection;
using RequestHandlers.Http;
using RequestHandlers.Mvc.CSharp;

namespace RequestHandlers.Mvc
{
    public static class RequestHandlerControllerBuilder
    {
        public static Assembly Build(IRequestDefinition[] definitions, IControllerAssemblyBuilder controllerAssemblyBuilder = null)
        {
            controllerAssemblyBuilder = controllerAssemblyBuilder ?? new CSharpBuilder("Proxy");
            var controllerDefinitions =
                definitions.SelectMany(x => 
                    x.RequestType.GetTypeInfo()
                        .GetCustomAttributes(true)
                        .OfType<HttpRequestAttribute>()
                        .Select(d => new HttpRequestHandlerDefinition(d, x))
                )
                .ToArray();
            return controllerAssemblyBuilder.Build(controllerDefinitions);
        }
    }

}
