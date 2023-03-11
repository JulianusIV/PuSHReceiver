using System.Text.Json.Serialization;

namespace DefaultPlugins.TwitchConsumer.TwitchApiModels
{
    public class TwitchCallbackVerificationJson
    {
        [JsonPropertyName("challenge")]
        public string Challenge { get; set; } = string.Empty;

        [JsonPropertyName("subscription")]
        public Subscription Subscription { get; set; } = new();
    }
}
