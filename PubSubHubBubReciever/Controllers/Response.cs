using Data.JSONObjects;

namespace PubSubHubBubReciever.Controllers
{
    public class Response
    {
        public bool Challenge { get; set; }
        public int RetCode { get; set; }
        public string ReponseBody { get; set; }
        public DataSub Item { get; set; }
        public string User { get; set; }
        public string ItemUrl { get; set; }
        public string[] Args { get; set; }
    }
}
