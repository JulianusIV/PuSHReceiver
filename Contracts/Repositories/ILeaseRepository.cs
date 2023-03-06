using Models;

namespace Contracts.Repositories
{
    public interface ILeaseRepository
    {
        public Lease? FindLease(int id);
        public IEnumerable<Lease> GetActiveExpiredLeases();
        public IEnumerable<Lease> GetRunningLeases();
        public IEnumerable<Lease> GetSubscribedLeases();
        public IEnumerable<Lease> GetLeasesByUser(User owner);
        public int CountSubscribedLeases();
    }
}
