namespace Models
{
    public class Log
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Content { get; set; }

        public Log(DateTime timestamp, string content)
        {
            Timestamp = timestamp;
            Content = content;
        }
    }
}
