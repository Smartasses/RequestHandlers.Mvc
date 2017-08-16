using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace RequestHandlers.Mvc
{
    public static class OwinExtensions
    {
        ///<summary>
        /// Registers all services needed to use RequestHandlers.Mvc
        /// This method implicitly adds Mvc as it is needed by RequestHandlers.Mvc
        ///</summary>
        public static IServiceCollection AddRequestHandlers(this IServiceCollection services, params Assembly[] assemblies)
        {
            // Services used by RequestHandlers.Mvc
            services.AddTransient<IWebRequestProcessor, DefaultWebRequestProcessor>();
            services.AddTransient<IRequestDispatcher, DefaultRequestDispacher>();
            services.AddTransient<IRequestHandlerResolver>(x => new DefaultRequestHandlerResolver(x));

            // Helper to create the generic interfaces
            var requestHandlerInterface = typeof(IRequestHandler<,>);

            // Get all RequestHandlerDefinitions from the given assemblies
            var requestHandlerDefinitions = RequestHandlerFinder.InAssembly(assemblies);

            // Register them as services
            foreach (var requestHandler in requestHandlerDefinitions)
            {
                services.Add(new ServiceDescriptor(requestHandlerInterface.MakeGenericType(requestHandler.RequestType, requestHandler.ResponseType), requestHandler.RequestHandlerType, ServiceLifetime.Transient));
            }

            // Add them to Mvc so they can be used as controllers
            services
                .AddMvc()
                .AddApplicationPart(RequestHandlerControllerBuilder.Build(requestHandlerDefinitions));
            return services;
        }
    }
}