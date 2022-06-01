using Data.JSONObjects;
using Services;

namespace ServiceLayer
{
    public class TopicDataService : ITopicDataService
    {
        public bool AddTopic(DataSub dataSub, LeaseSub leaseSub)
        {
            throw new NotImplementedException();
        }

        public int CountSubbedTopics()
        {
            throw new NotImplementedException();
        }

        public bool DeleteTopic(DataSub dataSub, LeaseSub leaseSub)
        {
            throw new NotImplementedException();
        }

        public string GetCallback(ulong id)
        {
            throw new NotImplementedException();
        }

        public DataSub GetDataSub(ulong id)
        {
            throw new NotImplementedException();
        }

        public (List<DataSub>, List<LeaseSub>) GetExpiredAndRunningSubs()
        {
            throw new NotImplementedException();
        }

        public LeaseSub GetLeaseSub(ulong id)
        {
            throw new NotImplementedException();
        }

        public List<DataSub> GetSubbedTopics()
        {
            throw new NotImplementedException();
        }

        public void UpdateLease(ulong id, bool subscribe, int leaseTime = 0)
        {
            throw new NotImplementedException();
        }

        public bool UpdateTopic(DataSub dataSub, LeaseSub leaseSub)
        {
            throw new NotImplementedException();
        }

        public bool VerifyAdminToken(string token)
        {
            throw new NotImplementedException();
        }
    }
}
