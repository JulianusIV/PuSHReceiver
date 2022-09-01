using Data.JSONObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PubSubHubBubReciever.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FeedRecieverController : ControllerBase
    {
        #region Events
        public delegate Response OnGetRequestHandler(HttpRequest request, ulong topicId);
        public delegate Response OnPostRequestHandler(HttpRequest request, ulong topicId);
        public static event OnGetRequestHandler OnGet;
        public static event OnPostRequestHandler OnPost;
        #endregion

        [HttpGet]
        [Route("{topicId}")]
        public IActionResult Get([FromRoute] ulong topicId)
        {
            var ret = OnGet.Invoke(Request, topicId);
            
            if (!ret.Challenge && ret.IsSuccessStatusCode)
                ret.Item.Publish(ret.User, ret.ItemUrl, ret.Args);

            ContentResult result = null;
            if (ret.ReponseBody is not null)
            {
                result = Content(ret.ReponseBody);
                result.StatusCode = (int)ret.RetCode;
            }

            return result is null ? StatusCode((int)ret.RetCode) : result;
        }

        [HttpPost]
        [Route("{topicId}")]
        [Consumes("application/xml")]
        public IActionResult Post([FromRoute] ulong topicId)
        {
            var ret = OnPost.Invoke(Request, topicId);

            if (!ret.Challenge && ret.IsSuccessStatusCode)
                ret.Item.Publish(ret.User, ret.ItemUrl, ret.Args);

            ContentResult result = null;
            if (ret.ReponseBody is not null)
            {
                result = Content(ret.ReponseBody);
                result.StatusCode = (int)ret.RetCode;
            }

            return result is null ? StatusCode((int)ret.RetCode) : result;
        }
    }
}
