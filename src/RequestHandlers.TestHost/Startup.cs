using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RequestHandlers.Mvc;
using RequestHandlers.TestHost.RequestHandlers;

namespace RequestHandlers.TestHost
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IRequestProcessor, DefaultRequestProcessor>();
            services.AddTransient<IRequestDispatcher, DefaultRequestDispacher>();
            services.AddTransient<IRequestHandlerResolver>(x => new RequestHandlerResolver(x));
            var requestHandlerInterface = typeof(IRequestHandler<,>);
            foreach (var requestHandler in RequestHandlerFinder.InAssembly(this.GetType().GetTypeInfo().Assembly))
            {
                services.Add(new ServiceDescriptor(requestHandlerInterface.MakeGenericType(requestHandler.RequestType, requestHandler.ResponseType), requestHandler.RequestHandlerType, ServiceLifetime.Transient));
            }
            // Add framework services.
            
            var mvc = services.AddMvc();
            mvc.AddApplicationPart(RequestHandlerControllerBuilder.Build(RequestHandlerFinder.InAssembly(GetType().GetTypeInfo().Assembly)));

            // Inject an implementation of ISwaggerProvider with defaulted settings applied
            services.AddSwaggerGen();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();

            // Enable middleware to serve generated Swagger as a JSON endpoint
            app.UseSwagger();

            // Enable middleware to serve swagger-ui assets (HTML, JS, CSS etc.)
            app.UseSwaggerUi();
        }
    }

    class RequestHandlerResolver : IRequestHandlerResolver
    {
        private readonly IServiceProvider _provider;

        public RequestHandlerResolver(IServiceProvider provider)
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
