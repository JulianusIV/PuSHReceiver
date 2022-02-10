using DataLayer.JSONObject;
using System.Threading.Tasks;

namespace ServiceLayer.Interface
{
    public interface ISubscriptionService
    {
        public void SubscribeAll();
        public void UnsubscribeAll();
        public Task<bool> SubscribeAsync(DataSub dataSub, bool subscribe = true);
    }
}
