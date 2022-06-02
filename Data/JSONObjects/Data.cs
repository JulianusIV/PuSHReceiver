namespace Data.JSONObjects
{
#pragma warning disable CS8618
    public class Data
    {
        public string AdminToken { get; set; }
        public string CallbackURL { get; set; }
        public List<DataSub> Subs { get; set; }
    }

    public class DataSub
    {
        public ulong TopicID { get; set; }
        public string TopicURL { get; set; }
        public DateTime LastLease { get; set; }
        public int LeaseTime { get; set; }
        public bool Subscribed { get; set; }
        public string FeedPublisher { get; set; }
        public string FeedParser { get; set; }
        public string PublisherData { get; set; }
        public string ParserData { get; set; }
    }
#pragma warning restore CS8618
}
