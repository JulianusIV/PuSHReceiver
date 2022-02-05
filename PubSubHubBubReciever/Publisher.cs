using System;
using System.Collections.Specialized;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace PubSubHubBubReciever
{
    public class Publisher
    {
        private static byte[] lastPublishHash;

        public static void PublishToDiscord(string link)
        {
            using WebClient webClient = new WebClient();
            var pubText = Environment.GetEnvironmentVariable(EnvVars.PUBLISH_TEXT.ToString());
            pubText = Regex.Unescape(pubText);

            var hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(pubText));
            if (!(lastPublishHash is null))
            {
                if (hash == lastPublishHash)
                    return;
            }

            lastPublishHash = hash;

            webClient.UploadValues(Environment.GetEnvironmentVariable(EnvVars.WEBHOOK_URL.ToString()), new NameValueCollection
            {
                { "username", Environment.GetEnvironmentVariable(EnvVars.USERNAME.ToString()) },
                { "content", pubText + " " + link },
                { "avatar_url", Environment.GetEnvironmentVariable(EnvVars.HOOK_PFP.ToString()) }
            });
        }
    }
}
