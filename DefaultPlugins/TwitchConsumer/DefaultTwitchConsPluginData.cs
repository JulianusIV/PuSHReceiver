using PluginLibrary;

namespace DefaultPlugins.TwitchConsumer
{
    [PluginData(typeof(TwitchConsumer))]
    public class DefaultTwitchConsPluginData
    {
        public string Secret { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public DefaultTwitchConsPluginData(string secret, string clientId, string clientSecret)
        {
            Secret = secret;
            ClientId = clientId;
            ClientSecret = clientSecret;
        }
    }
}
