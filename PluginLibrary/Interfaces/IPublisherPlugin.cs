using Models;

namespace PluginLibrary.Interfaces
{
    public interface IPublisherPlugin : IBasePlugin
    {
        public Task PublishAsync(Lease lease, string user, string itemUrl, string eventId, params string[] args);
    }
}
