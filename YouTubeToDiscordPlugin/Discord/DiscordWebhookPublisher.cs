using DataLayer.JSONObject;
using Plugin.Objects;
using Plugin.PublisherPlugin;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace YouTubeToDiscordPlugin.Discord
{
    public class DiscordWebhookPublisher : IPublishPlugin
    {
        private static readonly Dictionary<ulong, byte[]> lastPublishHashes = new Dictionary<ulong, byte[]>();

        public string Name => "DiscordWebhookPublisher";

        public void FeedUpdate(Feed feed, DataSub dataSub)
        {
            PluginDataObject pluginData = JsonSerializer.Deserialize<PluginDataObject>(dataSub.PublisherData);

            using WebClient webClient = new WebClient();
            var pubText = Regex.Unescape(pluginData.PubText);

            var hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(pubText));
            if (lastPublishHashes.ContainsKey(dataSub.TopicID))
            {
                if (hash == lastPublishHashes[dataSub.TopicID])
                    return;
                else
                    lastPublishHashes[dataSub.TopicID] = hash;
            }
            else
                lastPublishHashes.Add(dataSub.TopicID, hash);

            webClient.UploadValues(pluginData.WebhookURL, new NameValueCollection
            {
                { "username", pluginData.PubName },
                { "content", pubText + " " + feed.ItemURL },
                { "avatar_url", pluginData.PubPfp }
            });
        }

        public string TopicAdded(DataSub dataSub, params string[] additionalInfo)
        {
            var dataObj = new PluginDataObject();
            foreach (var item in additionalInfo)
            {
                var props = dataObj.GetType().GetProperties();

                if (item.StartsWith(Name))
                {
                    var property = item[..item.IndexOf('=')].Replace("DiscordWebhookPublisher", "");
                    var value = item[(item.IndexOf('=') + 1)..];

                    props.FirstOrDefault(x => x.Name == property)?.SetValue(dataObj, value);
                }
            }
            return JsonSerializer.Serialize(dataObj);
        }

        public string TopicUpdated(DataSub dataSub, DataSub oldData, params string[] additionalInfo)
        {
            var oldDataObj = JsonSerializer.Deserialize<PluginDataObject>(oldData.PublisherData);

            var dataObj = new PluginDataObject();
            var props = dataObj.GetType().GetProperties();
            foreach (var item in additionalInfo)
            {

                if (item.StartsWith(Name))
                {
                    var property = item[..item.IndexOf('=')].Replace("DiscordWebhookPublisher", "");
                    var value = item[(item.IndexOf('=') + 1)..];

                    props.FirstOrDefault(x => x.Name == property)?.SetValue(dataObj, value);
                }
            }

            foreach (var prop in props)
                if (prop.GetValue(dataObj) is null)
                    prop.SetValue(dataObj, prop.GetValue(oldDataObj));

            return JsonSerializer.Serialize(dataObj);
        }
    }
}
