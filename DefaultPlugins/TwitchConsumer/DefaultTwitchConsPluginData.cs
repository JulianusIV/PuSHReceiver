using PluginLibrary;

namespace DefaultPlugins.TwitchConsumer
{
    [PluginData(typeof(TwitchConsumer))]
    public class DefaultTwitchConsPluginData
    {
        public string Secret { get; set; }

        public DefaultTwitchConsPluginData(string secret)
        {
            Secret = secret;
        }
    }
}
