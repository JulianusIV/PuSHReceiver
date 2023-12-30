using Models;
using Models.ApiCommunication;
using PluginLibrary.PluginRepositories;

namespace PluginLibrary.Interfaces
{
    public interface IConsumerPlugin : IBasePlugin
    {
        public ILogRepository? LogRepository { get; set; }
        public Task<bool> SubscribeAsync(Lease lease, bool subscribe = true);

        public Response HandleGet(Lease lease, Request request);
        public Response HandlePost(Lease lease, Request request);
    }
}
