# RequestHandlers.Mvc [![NuGet](https://img.shields.io/nuget/v/RequestHandlers.Mvc.svg)](https://www.nuget.org/packages/RequestHandlers.Mvc/) ![Build](https://img.shields.io/vso/build/smartasses/3d9a33af-eaed-44e1-8b7d-40ed447cb6e3/64.svg)
RequestHandlers is a framework that helps you structure your code into small, easy-to-test units of work.

When you are writing an MVC-application, your controllers sometimes get cluttered with too much actions and code. If you use the classes and conventions given to you by RequestHandlers, your code will be more testable and comprehensable.

This package is specifically built for **ASP.NET Core MVC**.

## Getting Started
We'll split up this section in _Setup_ and _Creating a RequestHandler_
### Setup
 1. Download the NuGet package 
 with the dotnet command  
 ```> dotnet add package RequestHandlers.Mvc```  
 or via the NuGet Package Manager in Visual Studio
 2. Add RequestHandlers.Mvc to your registered services  
 This will also implicitly add Mvc-services  
 The assembly passed in the method, is the assembly in which our RequestHandlers are present.
 ```csharp  
 // Inside Startup.cs  
 public void ConfigureServices(IServiceCollection services)  
 {  
     services  
         .AddRequestHandlers(this.GetType().GetTypeInfo().Assembly);  
 }  
 ```

 3. Add Mvc to your Application
  ```csharp
 // Inside Startup.cs
 public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
 {
     app.UseMvc();
 }
 ```
 ### Creating a RequestHandler
 Now we're all set up to create our first RequestHandler.  

 Add a file to your application named _MyFirstRequestHandler.cs_
 In this file Add a 3 classes: _MyFirstRequest_, _MyFirstResponse_, and _MyFirstRequestHandler_.  
 
 Make _MyFirstRequest_ implement _IReturn\<MyFirstResponse>_ and give a a _GetRequest_-attribute with the parameter _"api/my-first-request-handler"_. This represents the url on which you can call the request handler.  
 and _MyFirstRequestHandler_ should implement _IRequestHandler\<MyFirstRequest, MyFirstResponse>_  
 This should look something like this:  
 ```csharp
[GetRequest("api/my-first-request-handler")]
public class MyFirstRequest : IReturn<MyFirstResponse>
{
}
public class MyFirstResponse
{
}
public class MyFirstRequestHandler : IRequestHandler<MyFirstRequest, MyFirstResponse>
{
    public MyFirstResponse Handle(MyFirstRequest request)
    {
        return new MyFirstResponse();
    }
}
 ```
 At this point, you should be able to run the application and request the configured url. By default in an ASP.NET Core application this should be _http://localhost:5000/api/my-first-request-handler_.  

### Adding url-variables and query string paramaters
Now we can call a simple, static url. But you probably want to pass some parameters to your RequestHandler.
You can add some properties to the request and response like this:
 ```csharp
[GetRequest("api/my-first-request-handler/{myParameter}")]
public class MyFirstRequest : IReturn<MyFirstResponse>
{
    public int MyParameter { get; set; }
}
public class MyFirstResponse
{
    public int BodyProperty { get; set; }
}
```
Those can be used inside the RequestHandler as so:
```csharp
public class MyFirstRequestHandler : IRequestHandler<MyFirstRequest, MyFirstResponse>
{
    public MyFirstResponse Handle(MyFirstRequest request)
    {
        return new MyFirstResponse { BodyProperty = request.MyParameter + 10 };
    }
}
 ```
 If you run and request `http://localhost:5000/api/my-request-handler/15` the response will be
 ```json
 {
     "BodyProperty": 25
 }
 ```
 These are the most basic examples of using RequestHandlers.
