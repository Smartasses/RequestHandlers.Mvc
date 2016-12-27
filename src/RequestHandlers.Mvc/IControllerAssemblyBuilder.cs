using System.Reflection;
using RequestHandlers.Http;

namespace RequestHandlers.Mvc
{
    public interface IControllerAssemblyBuilder
    {
        Assembly Build(HttpRequestHandlerDefinition[] definitions);
    }
}