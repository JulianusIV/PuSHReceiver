using DataLayer.JSONObject;
using Plugin.Objects;

namespace Plugin.PublisherPlugin
{
    public interface IPublishPlugin : IBasePlugin
    {
        public void FeedUpdate(Feed feed, DataSub dataSub);
    }
}
