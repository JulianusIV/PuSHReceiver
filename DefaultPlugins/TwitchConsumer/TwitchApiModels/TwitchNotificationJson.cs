using System.Text.Json.Serialization;

namespace DefaultPlugins.TwitchConsumer.TwitchApiModels
{
    public class TwitchNotificationJson
    {
        [JsonPropertyName("subscription")]
        public Subscription Subscription { get; set; } = new();

        [JsonPropertyName("event")]
        public Event Event { get; set; } = new();
    }

    public class Subscription
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("version")]
        public string Version { get; set; } = string.Empty;
        [JsonPropertyName("cost")]
        public int Cost { get; set; }
        [JsonPropertyName("condition")]
        public Condition Condition { get; set; } = new();
        [JsonPropertyName("transport")]
        public Transport Transport { get; set; } = new();
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class Condition
    {
        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;
    }

    public class Transport
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;
        [JsonPropertyName("callback")]
        public string Callback { get; set; } = string.Empty;
    }

    public class Event
    {
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;
        [JsonPropertyName("user_login")]
        public string UserLogin { get; set; } = string.Empty;
        [JsonPropertyName("user_name")]
        public string UserName { get; set; } = string.Empty;
        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;
        [JsonPropertyName("broadcaster_user_login")]
        public string BroadcasterUserLogin { get; set; } = string.Empty;
        [JsonPropertyName("broadcaster_user_name")]
        public string BroadcasterUserName { get; set; } = string.Empty;
        [JsonPropertyName("followed_at")]
        public string FollowedAt { get; set; } = string.Empty;
    }

}
