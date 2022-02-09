using Plugin.Objects;

namespace Plugin.ParserPlugin
{
    public interface IParserPlugin
    {
        public string Name { get; protected set; }

        public Feed FeedUpdate(string payload);
    }
}
