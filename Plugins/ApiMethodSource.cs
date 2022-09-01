using PubSubHubBubReciever.Controllers;

namespace Plugins
{
    public class ApiMethodSource
    {
#nullable disable
        public delegate Response OnGetRequestHandler(Request request, ulong topicId);
        public delegate Response OnPostRequestHandler(Request request, ulong topicId);
        public static event OnGetRequestHandler OnGet;
        public static event OnPostRequestHandler OnPost;
#nullable enable

        public static Response InvokeGet(Request request, ulong topicId)
            => OnGet.Invoke(request, topicId);

        public static Response InvokePost(Request request, ulong topicId)
            => OnPost.Invoke(request, topicId);
    }

    public class Request
    {
#nullable disable
        public Dictionary<string, string> QueryParameters { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
#nullable enable
    }
}
