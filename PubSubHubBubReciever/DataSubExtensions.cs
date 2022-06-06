using Data.JSONObjects;
using Plugins.Interfaces;
using System.Threading.Tasks;

namespace PubSubHubBubReciever
{
    public static class DataSubExtensions
    {
        public static Task Publish(this DataSub dataSub, string user, string itemUrl, params string[] args)
        {
            var plugin = Runtime.Instance.PluginLoader.ResolvePlugin<IPublisherPlugin>(dataSub.FeedPublisher);
            plugin.Publish(dataSub, user, itemUrl, args);
            return Task.CompletedTask;
        }
    }
}
