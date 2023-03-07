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
        public string PublisherData { get; set; } = "{}";
        [Column(TypeName = "json")]
        public string ConsumerData { get; set; } = "{}";

        public Lease(string displayName, string topicUrl)
        {
            DisplayName = displayName;
            if (string.IsNullOrEmpty(topicUrl))
                throw new ArgumentNullException(nameof(topicUrl));
            TopicUrl = topicUrl;
        }

        [Obsolete("Use ctor with displayName and topicUrl parameters instead")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Lease() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void UpdateLeaseRecieved(bool subscribe, int leaseTime = 0)
        {
            LeaseTime = leaseTime;
            LastLease = DateTime.Now;
            Subscribed = subscribe;
        }

        public T? GetObjectFromPublisherString<T>()
            => JsonSerializer.Deserialize<T>(PublisherData);

        public T? GetObjectFromConsumerString<T>()
            => JsonSerializer.Deserialize<T>(ConsumerData);

        public string GetCallbackUrl()
            => ConfigurationManager.WebConfig.CallbackUrl + $"/{Id}";
    }
}
