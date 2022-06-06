using Data.JSONObjects;

namespace Plugins.Interfaces
{
    public interface IPublisherPlugin : IBasePlugin
    {
        void Init();
        void Publish(DataSub dataSub, string user, string itemUrl, params string[] args);
    }
}
