using Data.JSONObjects;
using Services.Interfaces;

namespace Services
{
    public interface ITopicDataService : IService
    {
        DataSub? GetDataSub(ulong id);
        bool VerifyAdminToken(string token);
        string GetCallback(ulong id);
        bool AddTopic(DataSub dataSub);
        bool UpdateTopic(DataSub dataSub);
        bool DeleteTopic(DataSub dataSub);
        IEnumerable<DataSub> GetExpiredSubs();
        IEnumerable<DataSub> GetRunningSubs();
        bool UpdateLease(ulong id, bool subscribe, int leaseTime = 0);
        int CountSubbedTopics();
        IEnumerable<DataSub> GetSubbedTopics();
    }
}
