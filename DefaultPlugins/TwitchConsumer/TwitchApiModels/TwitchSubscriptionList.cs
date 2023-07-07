using System.Text.Json.Serialization;

namespace DefaultPlugins.TwitchConsumer.TwitchApiModels
{
    public class TwitchSubscriptionList
    {
        [JsonPropertyName("data")]
        public Datum[] Data { get; set; } = Array.Empty<Datum>();
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("total_cost")]
        public int TotalCost { get; set; }
        [JsonPropertyName("max_total_cost")]
        public int MaxTotalCost { get; set; }
        [JsonPropertyName("pagination")]
        public Pagination Pagination { get; set; } = new();
    }

    public class Pagination { }

    public class Datum
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
        public SubListCondition Condition { get; set; } = new();
        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = string.Empty;
        [JsonPropertyName("transport")]
        public SubListTransport Transport { get; set; } = new();
    }

    public class SubListCondition
    {
        [JsonPropertyName("broadcaster_user_id")]
        public string BroadcasterUserId { get; set; } = string.Empty;
        [JsonPropertyName("user_id")]
        public string UserId { get; set; } = string.Empty;
    }

    public class SubListTransport
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;
        [JsonPropertyName("callback")]
        public string Callback { get; set; } = string.Empty;
    }

}
