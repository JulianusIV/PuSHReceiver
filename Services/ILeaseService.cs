using Data.JSONObjects;
using Services.Interfaces;

namespace Services
{
    public interface ILeaseService : IService
    {
        void RegisterLease(DataSub dataSub, int leaseTime);
    }
}
