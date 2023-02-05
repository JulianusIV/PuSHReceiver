using PluginLibrary;

namespace DefaultPlugins.DiscordPublisher
{
    [PluginData(typeof(DiscordPublisher))]
    public class DefaultDiscordPubPluginData
    {
        public string WebhookUrl { get; set; }
        public string PubName { get; set; } = "PuSH";
        public string PubPfp { get; set; }
        public string PubText { get; set; }

        public DefaultDiscordPubPluginData(string webhookUrl, string pubPfp, string pubText)
        {
            WebhookUrl = webhookUrl;
            PubPfp = pubPfp;
            PubText = pubText;
        }
    }
}
