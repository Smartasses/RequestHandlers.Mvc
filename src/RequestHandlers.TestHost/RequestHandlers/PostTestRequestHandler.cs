using RequestHandlers.Http;

namespace RequestHandlers.TestHost.RequestHandlers
{
    [PostRequest("api/test/{param1}?test&test2")]
    public class PostTestRequest : IReturn<PostTestResponse>
    {
        public string Param1 { get; set; }
        public string Test { get; set; }
        public string Test2 { get; set; }
        [FromQueryString]
        public string ThisShouldBeQueryString { get; set; }
        public string ThisShouldBeBody { get; set; }
        public string ThisShouldBeAsWell { get; set; }
    }
    public class PostTestResponse
    {

        public string Param1 { get; set; }
        public string Test { get; set; }
        public string Test2 { get; set; }
    }
    public class PostTestRequestHandler : IRequestHandler<PostTestRequest, PostTestResponse>
    {
        public PostTestResponse Handle(PostTestRequest request)
        {
            return new PostTestResponse
            {
                Param1 = request.Param1,
                Test2 = request.Test2,
                Test = request.Test
            };
        }
    }
}