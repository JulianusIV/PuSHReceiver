using Data.JSONObjects;
using Services.Interfaces;

namespace Services
{
    public interface ISubscriptionService : IService
    {
        void SubscribeAll();
        void UnsubscribeAll();
    }
}