using Models;

namespace Contracts.Repositories
{
    public interface ILeaseRepository
    {
        public Lease? FindLease(int id);
        public IEnumerable<Lease> GetActiveExpiredLeases();
        public IEnumerable<Lease> GetRunningLeases();
        public IEnumerable<Lease> GetSubscribedLeases();
        public int CountSubscribedLeases();
    }
}
