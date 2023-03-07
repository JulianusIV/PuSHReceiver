namespace Models.ApiCommunication
{
    public class Request
    {
        public Dictionary<string, string> Headers { get; set; } = new();
        public Dictionary<string, string> QueryParameters { get; set; } = new();
        public string? Body { get; set; }
    }
}
