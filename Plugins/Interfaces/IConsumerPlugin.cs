using Data.JSONObjects;

namespace Plugins.Interfaces
{
    public interface IConsumerPlugin : IBasePlugin
    {
        Task<bool> SubscribeAsync(DataSub dataSub, bool subscribe = true);
    }
}
