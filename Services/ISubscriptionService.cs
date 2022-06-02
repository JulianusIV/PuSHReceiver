using Data.JSONObjects;
using Services.Interfaces;

namespace Services
{
    public interface ISubscriptionService : IService
    {
        void SubscribeAll();
        void UnsubscribeAll();
        Task<bool> SubscribeAsync(DataSub dataSub, bool subscribe = true);
    }
}