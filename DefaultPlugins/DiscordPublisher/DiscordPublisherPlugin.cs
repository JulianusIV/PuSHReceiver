using Data.JSONObjects;
using Plugins.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DefaultPlugins.DiscordPublisher
{
    public class DiscordPublisherPlugin : IPublisherPlugin
    {
        private static readonly Dictionary<ulong, Queue<string>> lastPublishUrls = new();

        public string Name => "Default_DiscordPublisher";

        public void Init() { }

        public async Task PublishAsync(DataSub dataSub, string user, string itemUrl, params string[] args)
        {
            if (lastPublishUrls.ContainsKey(dataSub.TopicID))
            {
                if (lastPublishUrls[dataSub.TopicID].Contains(itemUrl))
                    return;
                else
                {
                    lastPublishUrls[dataSub.TopicID].Enqueue(itemUrl);
                    if (lastPublishUrls[dataSub.TopicID].Count > 10)
                        lastPublishUrls[dataSub.TopicID].Dequeue();
                }
            }
            else
            {
                lastPublishUrls.Add(dataSub.TopicID, new());
                lastPublishUrls[dataSub.TopicID].Enqueue(itemUrl);
            }

            PluginData? pluginData = JsonSerializer.Deserialize<PluginData>(dataSub.PublisherData);

            if (pluginData is null)
                return;
            string content = $"{{\"username\": \"{pluginData.PubName}\", " +
                $"\"content\": \"{Regex.Unescape(pluginData.PubText) + " " + itemUrl}\", " +
                $"\"avatar_url\": \"{pluginData.PubPfp}\"}}";

            using var client = new HttpClient();
            await client.PostAsync(pluginData.WebhookURL, new StringContent(content, Encoding.UTF8, "application/json"));
        }

        public string? AddSubscription(params string[] additionalInfo)
        {
            var ret = new PluginData();

            foreach (var item in additionalInfo)
            {
                var props = ret.GetType().GetProperties();

                if (item.StartsWith(Name))
                {
                    var property = item[..item.IndexOf('=')].Replace("DiscordWebhookPublisher", "");
                    var value = item[(item.IndexOf('=') + 1)..];

                    props.FirstOrDefault(x => x.Name == property)?.SetValue(ret, value);
                }
            }
            return JsonSerializer.Serialize(ret);
        }

        public string? UpdateSubscription(string oldData, params string[] additionalInfo)
        {
            var oldDataObj = JsonSerializer.Deserialize<PluginData>(oldData);

            var ret = new PluginData();

            var props = ret.GetType().GetProperties();
            foreach (var item in additionalInfo)
            {

                if (item.StartsWith(Name))
                {
                    var property = item[..item.IndexOf('=')].Replace("DiscordWebhookPublisher", "");
                    var value = item[(item.IndexOf('=') + 1)..];

                    props.FirstOrDefault(x => x.Name == property)?.SetValue(ret, value);
                }
            }

            foreach (var prop in props)
                if (prop.GetValue(ret) is null)
                    prop.SetValue(ret, prop.GetValue(oldDataObj));

            return JsonSerializer.Serialize(ret);
        }
    }
}
