using System.Threading.Tasks;
using RequestHandlers.Http;

namespace RequestHandlers.TestHost.RequestHandlers
{
    [GetRequest("api/test/{param1}?test&test2")]
    public class GetTestRequest : IReturn<GetTestResponse>
    {
        public string Param1 { get; set; }
        public string Test { get; set; }
        public string Test2 { get; set; }
    }
    public class GetTestResponse
    {

        public string Param1 { get; set; }
        public string Test { get; set; }
        public string Test2 { get; set; }
    }
    public class GetTestRequestHandler : IAsyncRequestHandler<GetTestRequest, GetTestResponse>
    {
        public async Task<GetTestResponse> Handle(GetTestRequest request)
        {
            await Task.Delay(30);
            return new GetTestResponse
            {
                Param1 = request.Param1,
                Test2 = request.Test2,
                Test = request.Test
            };
        }
    }
}