using PluginLibrary;

namespace DefaultPlugins.YouTubeConsumer
{
    [PluginData(typeof(YouTubeConsumer))]
    public class DefaultYouTubeConsPluginData
    {
        public string Token { get; set; }
        public string Secret { get; set; }

        public DefaultYouTubeConsPluginData(string token, string secret)
        {
            Token = token;
            Secret = secret;
        }
    }
}
