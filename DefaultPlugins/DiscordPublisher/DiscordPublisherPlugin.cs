using Data.JSONObjects;
using Plugins.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DefaultPlugins.DiscordPublisher
{
    public class DiscordPublisherPlugin : IPublisherPlugin
    {
        private static readonly Dictionary<ulong, string> lastPublishUrls = new();

        public string Name => "Default_DiscordPublisher";

        public void Init()
        {
        }

        public void Publish(DataSub dataSub, string user, string itemUrl, params string[] args)
        {
            if (lastPublishUrls.ContainsKey(dataSub.TopicID))
            {
                if (itemUrl == lastPublishUrls[dataSub.TopicID])
                    return;
                else
                    lastPublishUrls[dataSub.TopicID] = itemUrl;
            }
            else
                lastPublishUrls.Add(dataSub.TopicID, itemUrl);

            PluginData? pluginData = JsonSerializer.Deserialize<PluginData>(dataSub.PublisherData);

            if (pluginData is null)
                return;
            string content = $"{{\"username\": \"{pluginData.PubName}\", " +
                $"\"content\": \"{Regex.Unescape(pluginData.PubText) + " " + itemUrl}\", " +
                $"\"avatar_url\": \"{pluginData.PubPfp}\"}}";

            using var client = new HttpClient();
            client.PostAsync(pluginData.WebhookURL, new StringContent(content, Encoding.UTF8, "application/json"));
        }
    }
}
