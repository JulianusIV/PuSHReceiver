using System.Net;

namespace Models.ApiCommunication
{
    public class Response
    {
        public bool Challenge { get; set; }
        public HttpStatusCode ReturnStatus { get; set; }
        public string ResponseBody { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string ItemUrl { get; set; } = string.Empty;
        public string EventId { get; set; } = string.Empty;
        public string[] Args { get; set; } = Array.Empty<string>();
        public bool IsSuccessStatusCode => ((int)ReturnStatus >= 200) && ((int)ReturnStatus <= 299);
    }
}
