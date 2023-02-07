using Configuration;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Models
{
    public class Lease
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string TopicUrl { get; set; }
        public string HubUrl { get; set; } = "https://pubsubhubbub.appspot.com/subscribe";
        public DateTime LastLease { get; set; }
        public int LeaseTime { get; set; }
        public bool Subscribed { get; set; }
        public bool Active { get; set; }
        public string Publisher { get; set; } = "Default_DiscordPublisher";
        public string Consumer { get; set; } = "Default_YoutubeConsumer";
        public User? Owner { get; set; }
        [Column(TypeName = "json")]
        public string PublisherData { get; set; } = string.Empty;
        [Column(TypeName = "json")]
        public string ConsumerData { get; set; } = string.Empty;

        public Lease(string displayName, string topicUrl)
        {
            DisplayName = displayName;
            if (string.IsNullOrEmpty(topicUrl))
                throw new ArgumentNullException(nameof(topicUrl));
            TopicUrl = topicUrl;
        }

        public void UpdateLeaseRecieved(bool subscribe, int leaseTime = 0)
        {
            this.LeaseTime = leaseTime;
            this.LastLease = DateTime.Now;
            this.Subscribed = subscribe;
        }

        public T? GetObjectFromPublisherString<T>()
            => JsonSerializer.Deserialize<T>(PublisherData);

        public T? GetObjectFromConsumerString<T>()
            => JsonSerializer.Deserialize<T>(ConsumerData);

        public string GetCallbackUrl()
            => ConfigurationManager.WebConfig.CallbackUrl + $"/{Id}";
    }
}
