using PubSubHubBubReciever.JSONObjects;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PubSubHubBubReciever
{
    public class Publisher
    {
        private static readonly Dictionary<long, byte[]> lastPublishHashes = new Dictionary<long, byte[]>();

        public static void PublishToDiscord(DataSub topic, string link)
        {
            using WebClient webClient = new WebClient();
            var pubText = Regex.Unescape(topic.PubText);

            var hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(pubText));
            if (lastPublishHashes.ContainsKey(topic.TopicID))
            {
                if (hash == lastPublishHashes[topic.TopicID])
                    return;
                else
                    lastPublishHashes[topic.TopicID] = hash;
            }
            else
                lastPublishHashes.Add(topic.TopicID, hash);

            webClient.UploadValues(topic.WebhookURL, new NameValueCollection
            {
                { "username", topic.PubName },
                { "content", pubText + " " + link },
                { "avatar_url", topic.PubProfilePic }
            });
        }
    }
}
