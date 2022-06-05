using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PubSubHubBubReciever.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeedRecieverController : ControllerBase
    {
        #region Events
        public delegate int OnGetRequestHandler(HttpRequest request, ulong topicId);
        public delegate int OnPostRequestHandler(HttpRequest request, ulong topicId);
        public static event OnGetRequestHandler OnGet;
        public static event OnPostRequestHandler OnPost;
        #endregion

        [HttpGet]
        [Route("{topicId}")]
        public IActionResult Get([FromRoute] ulong topicId)
            => StatusCode(OnGet.Invoke(Request, topicId));

        [HttpPost]
        [Route("{topicId}")]
        [Consumes("application/xml")]
        public IActionResult Post([FromRoute] ulong topicId)
            => StatusCode(OnPost.Invoke(Request, topicId));
    }
}
