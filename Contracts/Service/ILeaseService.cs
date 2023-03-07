using Models;

namespace Contracts.Service
{
    public interface ILeaseService
    {
        void RegisterLease(Lease lease, int leaseTime = 0);
    }
}
