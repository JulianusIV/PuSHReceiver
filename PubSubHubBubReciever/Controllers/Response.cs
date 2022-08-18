using Data.JSONObjects;
using System.Net;

namespace PubSubHubBubReciever.Controllers
{
    public class Response
    {
        public bool Challenge { get; set; }
        public HttpStatusCode RetCode { get; set; }
        public string ReponseBody { get; set; }
        public DataSub Item { get; set; }
        public string User { get; set; }
        public string ItemUrl { get; set; }
        public string[] Args { get; set; }
        public bool IsSuccessStatusCode => ((int)RetCode >= 200) && ((int)RetCode <= 299);
    }
}
