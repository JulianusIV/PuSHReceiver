using Plugin.Objects;

namespace Plugin.PublisherPlugin
{
    public interface IPublishPlugin
    {
        public string Name { get; protected set; }

        public void FeedUpdate(Feed feed);
    }
}
