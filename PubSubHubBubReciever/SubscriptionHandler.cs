using PubSubHubBubReciever.JSONObjects;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PubSubHubBubReciever
{
    public class SubscriptionHandler
    {
        public static bool TopicExists(long topicId)
            => TopicRepository.Instance.Data.Subs.Any(x => x.TopicID == topicId);

        public static bool VerifyToken(long topicId, string token)
            => TopicRepository.Instance.Data.Subs.Single(x => x.TopicID == topicId).Token == token;

        public static bool VerifyTopicURL(long topicId, string topicUrl)
            => TopicRepository.Instance.Data.Subs.Single(x => x.TopicID == topicId).TopicURL == topicUrl;

        public static void UpdateLeaseFile(long topicId, bool subscribe, int leaseTime = 0)
        {
            var lease = TopicRepository.Instance.Leases.Subs.Single(x => x.TopicID == topicId);
            lease.LeaseTime = leaseTime;
            lease.LastLease = leaseTime == 0 ? DateTime.MinValue : DateTime.Now;
            lease.Subscribed = subscribe;
            TopicRepository.Instance.Save(FileNames.leases);
        }

        public async static void SubscribeAll()
        {
            var toSubscribe = TopicRepository.Instance.Data.Subs.Where(x =>
            {
                var lease = TopicRepository.Instance.Leases.Subs.Single(y => y.TopicID == x.TopicID);
                var leaseExpiration = lease.LastLease + TimeSpan.FromSeconds(lease.LeaseTime);
                return leaseExpiration < DateTime.Now || !lease.Subscribed;
            });

            foreach (var item in toSubscribe)
            {
                await FeedSubscriber.SubscribeAsync(item);
            }

            foreach (var item in TopicRepository.Instance.Leases.Subs.Where(x => 
                (!toSubscribe.Any(y => y.TopicID == x.TopicID)) && 
                (x.LastLease + TimeSpan.FromSeconds(x.LeaseTime) > DateTime.Now) && 
                x.Subscribed))
            {
                FeedSubscriber.AwaitLease(item.TopicID, (int)(item.LastLease + TimeSpan.FromSeconds(item.LeaseTime) - DateTime.Now).TotalSeconds);
            }
        }

        public async static void UnsubscribeAll()
        {
            var toUnsubscribe = TopicRepository.Instance.Data.Subs.Where(x =>
            {
                var lease = TopicRepository.Instance.Leases.Subs.Single(y => y.TopicID == x.TopicID);
                return lease.Subscribed;
            });

            foreach (var item in toUnsubscribe)
            {
                await FeedSubscriber.SubscribeAsync(item, false);
            }
        }

        public static DataSub GetTopic(long topicId)
            => TopicRepository.Instance.Data.Subs.Any(x => x.TopicID == topicId) ? TopicRepository.Instance.Data.Subs.Single(x => x.TopicID == topicId) : null;

        public static int CountSubs()
            => TopicRepository.Instance.Leases.Subs.Count(x => x.Subscribed);

        public static bool IsSubscribed(long topicId)
            => TopicRepository.Instance.Leases.Subs.Single(x => x.TopicID == topicId).Subscribed;

        public static string GetSecret(long topicId)
            => TopicRepository.Instance.Data.Subs.Single(x => x.TopicID == topicId).Secret;

        public static string GetTopicCallback(long topicId)
            => TopicRepository.Instance.Data.Subs.Any(x => x.TopicID == topicId) ? TopicRepository.Instance.Data.CallbackURL + "/" + topicId : null;

        public static bool VerifyAdminToken(string token)
            => TopicRepository.Instance.Data.AdminToken == token;

        public async static Task<bool> AddTopic(DataSub dataSub, LeaseSub leaseSub)
        {
            TopicRepository.Instance.Data.Subs.Add(dataSub);
            TopicRepository.Instance.Leases.Subs.Add(leaseSub);

            if (await FeedSubscriber.SubscribeAsync(dataSub))
            {
                TopicRepository.Instance.Save(FileNames.data);
                TopicRepository.Instance.Save(FileNames.leases);
                return true;
            }

            TopicRepository.Instance.Data.Subs.Remove(dataSub);
            TopicRepository.Instance.Leases.Subs.Remove(leaseSub);

            return false;
        }

        public async static Task<bool> RemoveTopic(long topicId)
        {
            var topic = TopicRepository.Instance.Data.Subs.Single(x => x.TopicID == topicId);
            if (!await FeedSubscriber.SubscribeAsync(topic, false))
                return false;

            TopicRepository.Instance.Data.Subs.Remove(topic);
            TopicRepository.Instance.Leases.Subs.Remove(TopicRepository.Instance.Leases.Subs.Single(x => x.TopicID == topicId));

            TopicRepository.Instance.Save(FileNames.data);
            TopicRepository.Instance.Save(FileNames.leases);

            return true;
        }

        public async static Task<bool> UpdateTopic(DataSub dataSub, LeaseSub leaseSub)
        {
            var oldTopic = TopicRepository.Instance.Data.Subs.Single(x => x.TopicID == dataSub.TopicID);
            if (!await FeedSubscriber.SubscribeAsync(oldTopic, false))
                return false;

            var dataIndex = TopicRepository.Instance.Data.Subs.IndexOf(oldTopic);
            TopicRepository.Instance.Data.Subs[dataIndex] = dataSub;

            var oldLease = TopicRepository.Instance.Leases.Subs.Single(x => x.TopicID == dataSub.TopicID);
            var leaseIndex = TopicRepository.Instance.Leases.Subs.IndexOf(oldLease);
            TopicRepository.Instance.Leases.Subs[leaseIndex] = leaseSub;

            if (await FeedSubscriber.SubscribeAsync(dataSub))
            {
                TopicRepository.Instance.Save(FileNames.data);
                TopicRepository.Instance.Save(FileNames.leases);
                return true;
            }

            TopicRepository.Instance.Data.Subs[dataIndex] = dataSub;
            TopicRepository.Instance.Leases.Subs[leaseIndex] = oldLease;

            await FeedSubscriber.SubscribeAsync(dataSub);
            return false;
        }
    }
}
