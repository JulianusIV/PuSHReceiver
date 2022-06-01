using Data.JSONObjects;
using Services.Service.Interfaces;

namespace Services
{
    public interface ILeaseService : IService
    {
        void RegisterLease(DataSub dataSub, int leaseTime);
    }
}
