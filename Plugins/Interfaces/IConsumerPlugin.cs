using Data.JSONObjects;
using Services;

namespace Plugins.Interfaces
{
    public interface IConsumerPlugin : IBasePlugin
    {
        string BaseCalllbackUrl { get; set; }
        IDataProviderService DataProviderService { get; set; }
        ILeaseService LeaseService { get; set; }

        Task<bool> SubscribeAsync(DataSub dataSub, bool subscribe = true);
    }
}
