using Data.JSONObjects;
using Services.Interfaces;

namespace Services
{
    public interface ITopicDataService : IService
    {
        DataSub GetDataSub(ulong id);
        LeaseSub GetLeaseSub(ulong id);
        bool VerifyAdminToken(string token);
        string GetCallback(ulong id);
        bool AddTopic(DataSub dataSub, LeaseSub leaseSub);
        bool UpdateTopic(DataSub dataSub, LeaseSub leaseSub);
        bool DeleteTopic(DataSub dataSub, LeaseSub leaseSub);
        (List<DataSub>, List<LeaseSub>) GetExpiredAndRunningSubs();
        void UpdateLease(ulong id, bool subscribe, int leaseTime = 0);
        int CountSubbedTopics();
        List<DataSub> GetSubbedTopics();
    }
}
