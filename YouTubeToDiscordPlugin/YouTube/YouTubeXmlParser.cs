using DataLayer.JSONObject;
using Plugin.Objects;
using Plugin.ParserPlugin;
using System.IO;
using System.Xml.Serialization;

namespace YouTubeToDiscordPlugin.YouTube
{
    public class YouTubeXmlParser : IParserPlugin
    {
        public string Name => "YouTubeXmlParser";

        public Feed FeedUpdate(string payload)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(feed));
            using StringReader stringReader = new StringReader(payload);
            feed xml = (feed)serializer.Deserialize(stringReader);

            return new Feed
            {
                ItemURL = xml.entry.link.href,
                User = xml.entry.author.name
            };
        }

        public string TopicAdded(DataSub dataSub, params string[] additionalInfo)
            => null;

        public string TopicUpdated(DataSub dataSub, DataSub oldData, params string[] additionalInfo)
            => null;
    }
}
