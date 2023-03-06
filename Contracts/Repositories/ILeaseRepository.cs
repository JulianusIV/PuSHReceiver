using Models;

namespace Contracts.Repositories
{
    public interface ILeaseRepository
    {
        public void CreateLease(Lease lease);
        public void UpdateLease(Lease lease);
        public void DeleteLease(int id);
        public Lease? FindLease(int id);
        public IEnumerable<Lease> GetActiveExpiredLeases();
        public IEnumerable<Lease> GetRunningLeases();
        public IEnumerable<Lease> GetSubscribedLeases();
        public IEnumerable<Lease> GetLeasesByUser(User owner);
        public int CountSubscribedLeases();
    }
}
