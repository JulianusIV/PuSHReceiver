using Data.JSONObjects;

namespace Plugins.Interfaces
{
    public interface IPublisherPlugin : IBasePlugin
    {
        Task PublishAsync(DataSub dataSub, string user, string itemUrl, params string[] args);
    }
}
