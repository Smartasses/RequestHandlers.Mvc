using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RequestHandlers.Http;

namespace RequestHandlers.Mvc
{
    public interface IControllerAssemblyBuilder
    {
        Assembly Build(HttpRequestHandlerDefinition[] definitions);
    }
    public interface IWebRequestProcessor
    {
        IActionResult Process<TRequest, TResponse>(TRequest request, Controller controller);
        Task<IActionResult> ProcessAsync<TRequest, TResponse>(TRequest request, Controller controller);
    }

    public class DefaultWebRequestProcessor : IWebRequestProcessor
    {
        private readonly IRequestDispatcher _dispatcher;

        public DefaultWebRequestProcessor(IRequestDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public IActionResult Process<TRequest, TResponse>(TRequest request, Controller controller)
        {
            var response = _dispatcher.Process<TRequest, TResponse>(request);
            return new OkObjectResult(response);
        }

        public async Task<IActionResult> ProcessAsync<TRequest, TResponse>(TRequest request, Controller controller)
        {
            var response = await _dispatcher.Process<TRequest, Task<TResponse>>(request);
            return new OkObjectResult(response);
        }
    }
}