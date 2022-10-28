using PubSubHubBubReciever.Controllers;
using System.Text;

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

        public override string ToString()
        {
            StringBuilder stringBuilder = new();

            if (QueryParameters is not null && QueryParameters.Any())
            {
                stringBuilder.AppendLine("Parameters:");
                foreach (var parameter in QueryParameters)
                    stringBuilder.AppendLine(parameter.ToString());
            }

            if (Headers is not null && Headers.Any())
            {
                stringBuilder.AppendLine("Headers:");
                foreach (var header in Headers)
                    stringBuilder.AppendLine(header.ToString());
            }

            if (!string.IsNullOrWhiteSpace(Body))
            {
                stringBuilder.AppendLine("Body:");
                stringBuilder.AppendLine(Body);
            }

            return stringBuilder.ToString();
        }
    }
}
