using System;

namespace RequestHandlers.Mvc
{
    public class DefaultRequestHandlerResolver : IRequestHandlerResolver
    {
        private readonly IServiceProvider _provider;

        public DefaultRequestHandlerResolver(IServiceProvider provider)
        {
            _provider = provider;
        }

        public IRequestHandler<TRequest, TResponse> Resolve<TRequest, TResponse>()
        {
            var result = (IRequestHandler<TRequest, TResponse>)_provider.GetService(typeof(IRequestHandler<TRequest, TResponse>));
            return result;
        }
    }
}