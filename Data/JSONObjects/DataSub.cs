namespace Data.JSONObjects
{
#pragma warning disable CS8618
    public class DataSub
    {
        public ulong TopicID { get; set; }
        public string TopicURL { get; set; }
        public string HubURL { get; set; }
        public DateTime LastLease { get; set; }
        public int LeaseTime { get; set; }
        public bool Subscribed { get; set; }
        public string FeedPublisher { get; set; }
        public string FeedConsumer { get; set; }
        public string PublisherData { get; set; }
        public string ConsumerData { get; set; }
    }
#pragma warning restore CS8618
}
