using Plugin.Objects;

namespace Plugin.ParserPlugin
{
    public interface IParserPlugin : IBasePlugin
    {
        public Feed FeedUpdate(string payload);
    }
}
