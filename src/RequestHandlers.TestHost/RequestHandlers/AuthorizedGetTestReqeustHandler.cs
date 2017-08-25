using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using RequestHandlers.Http;

namespace RequestHandlers.TestHost.RequestHandlers
{
    [GetRequest("api/authorize-test/{param1}?test&test2")]
    public class AuthorizedGetTestRequest : IReturn<GetTestResponse>
    {
        public string Param1 { get; set; }
        public string Test { get; set; }
        public string Test2 { get; set; }
    }
    public class AuthorizedGetTestResponse { }

    // This will throw an exception and return a 500 instead of a 401 because Authentication isn't configured.
    [Authorize]
    public class AuthorizedGetTestRequestHandler : IRequestHandler<AuthorizedGetTestRequest, AuthorizedGetTestResponse>
    {
        public AuthorizedGetTestResponse Handle(AuthorizedGetTestRequest request)
        {
            return new AuthorizedGetTestResponse();
        }
    }
}