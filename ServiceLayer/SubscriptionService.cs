using Plugins.Interfaces;
using PubSubHubBubReciever;
using Services;

namespace ServiceLayer
{
    public class SubscriptionService : ISubscriptionService
    {
        public async void SubscribeAll()
        {
            var service = Runtime.Instance.ServiceLoader.ResolveService<ITopicDataService>();

            foreach (var sub in service.GetExpiredSubs())
                if (sub.FeedConsumer is not null)
                    await Runtime.Instance.PluginLoader.ResolvePlugin<IConsumerPlugin>(sub.FeedConsumer)
                        .SubscribeAsync(sub);

            foreach (var sub in service.GetRunningSubs())
                Runtime.Instance.ServiceLoader.ResolveService<ILeaseService>()
                    .RegisterLease(sub,
                    (int)(sub.LastLease + TimeSpan.FromSeconds(sub.LeaseTime) - DateTime.Now).TotalSeconds);
        }

        public async void UnsubscribeAll()
        {
            var service = Runtime.Instance.ServiceLoader.ResolveService<ITopicDataService>();

            foreach (var sub in service.GetSubbedTopics())
                await Runtime.Instance.PluginLoader.ResolvePlugin<IConsumerPlugin>(sub.FeedConsumer)
                    .SubscribeAsync(sub, false);
        }
    }
}
