using System;
using System.Collections.Generic;

namespace DataLayer.JSONObject
{
    public class Data
    {
        public string AdminToken { get; set; }
        public string CallbackURL { get; set; }

        public Dictionary<string, string> PluginData { get; set; }
        public List<DataSub> Subs { get; set; }
    }

    public class DataSub
    {
        public Guid TopicID { get; set; }
        public string TopicURL { get; set; }
        public string Token { get; set; }
        public string Secret { get; set; }
        public string PubName { get; set; }
        public string PubText { get; set; }
        public string PubProfilePic { get; set; }
        public string WebhookURL { get; set; }
        public string FeedPublisher { get; set; }
        public string FeedParser { get; set; }
    }

}
