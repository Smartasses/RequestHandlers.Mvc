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
            // Add them to Mvc so they can be used as controllers
            services
                .AddControllers()
                .AddRequestHandlers(assemblies);
            return services;
        }
        ///<summary>
        /// Registers all services needed to use RequestHandlers.Mvc
        /// This method implicitly adds Mvc as it is needed by RequestHandlers.Mvc
        ///</summary>
        public static IMvcBuilder AddRequestHandlers(this IMvcBuilder mvcBuilder, params Assembly[] assemblies)
        {
            
            // Services used by RequestHandlers.Mvc
            mvcBuilder.Services.AddTransient<IWebRequestProcessor, DefaultWebRequestProcessor>();
            mvcBuilder.Services.AddTransient<IRequestDispatcher, DefaultRequestDispacher>();
            mvcBuilder.Services.AddTransient<IRequestHandlerResolver>(x => new DefaultRequestHandlerResolver(x));

            // Helper to create the generic interfaces
            var requestHandlerInterface = typeof(IRequestHandler<,>);

            // Get all RequestHandlerDefinitions from the given assemblies
            var requestHandlerDefinitions = RequestHandlerFinder.InAssembly(assemblies);

            // Register them as services
            foreach (var requestHandler in requestHandlerDefinitions)
            {
                mvcBuilder.Services.Add(new ServiceDescriptor(requestHandlerInterface.MakeGenericType(requestHandler.RequestType, requestHandler.ResponseType), requestHandler.RequestHandlerType, ServiceLifetime.Transient));
            }

            // Add them to Mvc so they can be used as controllers
            mvcBuilder
                .AddApplicationPart(RequestHandlerControllerBuilder.Build(requestHandlerDefinitions));
            return mvcBuilder;
        }
    }
}