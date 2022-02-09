using PubSubHubBubReciever.DataService.Interface;
using PubSubHubBubReciever.JSONObject;
using PubSubHubBubReciever.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PubSubHubBubReciever.DataService
{
    internal class TopicDataService : ITopicDataService
    {
        bool ITopicDataService.AddTopic(DataSub dataSub, LeaseSub leaseSub)
        {
            TopicRepository.Data.Subs.Add(dataSub);
            TopicRepository.Leases.Subs.Add(leaseSub);
            try
            {
                TopicRepository.Save(FileNames.data);
                TopicRepository.Save(FileNames.leases);
            }
            catch (Exception)
            {
                TopicRepository.Data.Subs.Remove(dataSub);
                TopicRepository.Leases.Subs.Remove(leaseSub);
                return false;
            }
            return true;
        }

        int ITopicDataService.CountSubbedTopics()
            => TopicRepository.Leases.Subs.Count(x => x.Subscribed);

        bool ITopicDataService.DeleteTopic(DataSub dataSub, LeaseSub leaseSub)
        {
            TopicRepository.Data.Subs.Remove(dataSub);
            TopicRepository.Leases.Subs.Remove(leaseSub);
            try
            {
                TopicRepository.Save(FileNames.data);
                TopicRepository.Save(FileNames.leases);
            }
            catch (Exception)
            {
                TopicRepository.Data.Subs.Add(dataSub);
                TopicRepository.Leases.Subs.Add(leaseSub);
                return false;
            }
            return true;
        }

        string ITopicDataService.GetCallback(Guid id)
            => TopicRepository.Data.CallbackURL + "/" + id.ToString("N");

        DataSub ITopicDataService.GetDataSub(Guid id)
            => TopicRepository.Data.Subs.SingleOrDefault(x => x.TopicID == id);

        (List<DataSub>, List<LeaseSub>) ITopicDataService.GetExpiredAndRunningSubs()
        {
            var expired = TopicRepository.Data.Subs.Where(x =>
            {
                var lease = TopicRepository.Leases.Subs.Single(y => y.TopicID == x.TopicID);
                var leaseExpiration = lease.LastLease + TimeSpan.FromSeconds(lease.LeaseTime);
                return leaseExpiration < DateTime.Now || !lease.Subscribed;
            }).ToList();
            var running = TopicRepository.Leases.Subs.Where(x => !expired.Any(y => y.TopicID == x.TopicID)).ToList();
            return (expired, running);
        }

        LeaseSub ITopicDataService.GetLeaseSub(Guid id)
            => TopicRepository.Leases.Subs.Single(x => x.TopicID == id);

        void ITopicDataService.UpdateLease(Guid id, bool subscribe, int leaseTime)
        {
            var lease = TopicRepository.Leases.Subs.Single(x => x.TopicID == id);
            lease.LeaseTime = leaseTime;
            lease.LastLease = leaseTime == 0 ? DateTime.MinValue : DateTime.Now;
            lease.Subscribed = subscribe;
            TopicRepository.Save(FileNames.leases);
        }

        bool ITopicDataService.UpdateTopic(DataSub dataSub, LeaseSub leaseSub)
        {
            var oldDataSub = ((ITopicDataService)this).GetDataSub(dataSub.TopicID);
            var oldLeaseSub = ((ITopicDataService)this).GetLeaseSub(dataSub.TopicID);

            var dataIndex = TopicRepository.Data.Subs.IndexOf(oldDataSub);
            var leaseIndex = TopicRepository.Leases.Subs.IndexOf(oldLeaseSub);

            TopicRepository.Data.Subs[dataIndex] = dataSub;
            TopicRepository.Leases.Subs[leaseIndex] = leaseSub;

            try
            {
                TopicRepository.Save(FileNames.data);
                TopicRepository.Save(FileNames.leases);
            }
            catch (Exception)
            {
                TopicRepository.Data.Subs[dataIndex] = oldDataSub;
                TopicRepository.Leases.Subs[leaseIndex] = oldLeaseSub;
                return false;
            }
            return true;
        }

        bool ITopicDataService.VerifyAdminToken(string token)
            => TopicRepository.Data.AdminToken == token;
    }
}
