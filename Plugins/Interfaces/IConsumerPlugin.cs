using Data.JSONObjects;

namespace Plugins.Interfaces
{
    public interface IConsumerPlugin : IBasePlugin
    {
        void Init();

        Task<bool> SubscribeAsync(DataSub dataSub, bool subscribe = true);
    }
}
