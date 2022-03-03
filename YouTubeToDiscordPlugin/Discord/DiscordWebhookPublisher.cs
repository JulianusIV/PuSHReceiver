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
        private static readonly Dictionary<ulong, string> lastPublishUrls = new Dictionary<ulong, string>();

        public string Name => "DiscordWebhookPublisher";

        public void FeedUpdate(Feed feed, DataSub dataSub)
        {
            if (lastPublishUrls.ContainsKey(dataSub.TopicID))
            {
                if (feed.ItemURL == lastPublishUrls[dataSub.TopicID])
                    return;
                else
                    lastPublishUrls[dataSub.TopicID] = feed.ItemURL;
            }
            else
                lastPublishUrls.Add(dataSub.TopicID, feed.ItemURL);
            
            PluginDataObject pluginData = JsonSerializer.Deserialize<PluginDataObject>(dataSub.PublisherData);

            new WebClient().UploadValues(pluginData.WebhookURL, new NameValueCollection
            {
                { "username", pluginData.PubName },
                { "content", Regex.Unescape(pluginData.PubText) + " " + feed.ItemURL },
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
