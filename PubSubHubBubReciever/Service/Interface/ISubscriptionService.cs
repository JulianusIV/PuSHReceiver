using PubSubHubBubReciever.JSONObject;
using System.Threading.Tasks;

namespace PubSubHubBubReciever.Service.Interface
{
    internal interface ISubscriptionService
    {
        internal void SubscribeAll();
        internal void UnsubscribeAll();
        internal Task<bool> SubscribeAsync(DataSub dataSub, bool subscribe = true);
    }
}
