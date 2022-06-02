using Data.JSONObjects;
using Services.Interfaces;

namespace Services
{
    public interface ITopicDataService : IService
    {
        DataSub GetDataSub(ulong id);
        bool VerifyAdminToken(string token);
        string GetCallback(ulong id);
        bool AddTopic(DataSub dataSub);
        bool UpdateTopic(DataSub dataSub);
        bool DeleteTopic(DataSub dataSub);
        List<DataSub> GetExpiredAndRunningSubs();
        void UpdateLease(ulong id, bool subscribe, int leaseTime = 0);
        int CountSubbedTopics();
        List<DataSub> GetSubbedTopics();
    }
}
