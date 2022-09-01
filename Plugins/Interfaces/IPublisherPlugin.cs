using Data.JSONObjects;

namespace Plugins.Interfaces
{
    public interface IPublisherPlugin : IBasePlugin
    {
        void Init();
        Task PublishAsync(DataSub dataSub, string user, string itemUrl, params string[] args);
    }
}
