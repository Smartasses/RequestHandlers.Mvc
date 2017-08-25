using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using RequestHandlers.Http;

namespace RequestHandlers.TestHost.RequestHandlers
{
    [GetRequest("api/authorize-test")]
    public class AuthorizedGetTestRequest : IReturn<GetTestResponse> { }
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