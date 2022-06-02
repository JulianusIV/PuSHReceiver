using Data.JSONObjects;
using PubSubHubBubReciever;
using Services;

namespace ServiceLayer
{
    public class TopicDataService : ITopicDataService
    {
        public bool AddTopic(DataSub dataSub)
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            provider.Data.Subs.Add(dataSub);
            try
            {
                provider.Save();
            }
            catch (Exception)
            {
                provider.Data.Subs.Remove(dataSub);
                return false;
            }
            return true;
        }

        public int CountSubbedTopics()
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            return provider.Data.Subs.Count(x => x.Subscribed);
        }

        public bool DeleteTopic(DataSub dataSub)
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            provider.Data.Subs.Remove(dataSub);
            try
            {
                provider.Save();
            }
            catch (Exception)
            {
                provider.Data.Subs.Add(dataSub);
                return false;
            }
            return true;
        }

        public string GetCallback(ulong id)
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            return $"{provider.Data.CallbackURL}/{id}";
        }

        public DataSub? GetDataSub(ulong id)
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            return provider.Data.Subs.SingleOrDefault(x => x.TopicID == id);
        }

        public IEnumerable<DataSub> GetExpiredSubs()
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            return provider.Data.Subs.Where(x => (x.LastLease + TimeSpan.FromSeconds(x.LeaseTime) < DateTime.Now) || !x.Subscribed);
        }

        public IEnumerable<DataSub> GetRunningSubs()
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            return provider.Data.Subs.Where(x => (x.LastLease + TimeSpan.FromSeconds(x.LeaseTime) >= DateTime.Now) || x.Subscribed);
        }

        public IEnumerable<DataSub> GetSubbedTopics()
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            return provider.Data.Subs.Where(x => x.Subscribed);
        }

        public bool UpdateLease(ulong id, bool subscribe, int leaseTime = 0)
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();
            var lease = GetDataSub(id);
            if (lease is null)
                return false;
            var oldLeaseTime = lease.LeaseTime;
            var oldLastLease = lease.LastLease;
            var oldSubscribed = lease.Subscribed;
            lease.LeaseTime = leaseTime;
            lease.LastLease = leaseTime == 0 ? DateTime.MinValue : DateTime.Now;
            lease.Subscribed = subscribe;
            try
            {
                provider.Save();
            }
            catch (Exception)
            {
                lease.LeaseTime = oldLeaseTime;
                lease.LastLease = oldLastLease;
                lease.Subscribed = oldSubscribed;
                return false;
            }
            return true;
        }

        public bool UpdateTopic(DataSub dataSub)
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            var oldDataSub = GetDataSub(dataSub.TopicID);
            if (oldDataSub is null)
                return false;

            var index = provider.Data.Subs.IndexOf(oldDataSub);

            provider.Data.Subs[index] = dataSub;

            try
            {
                provider.Save();
            }
            catch (Exception)
            {
                provider.Data.Subs[index] = oldDataSub;
                return false;
            }
            return true;
        }

        public bool VerifyAdminToken(string token)
        {
            var provider = Runtime.Instance.ServiceLoader.ResolveService<IDataProviderService>();

            return provider.Data.AdminToken.Equals(token);
        }
    }
}
