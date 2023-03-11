using System.Text.Json.Serialization;

namespace DefaultPlugins.TwitchConsumer.TwitchApiModels
{
    internal class TwitchTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresInSeconds { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonIgnore]
        public DateTime GrantedAt { get; set; }
    }
}
