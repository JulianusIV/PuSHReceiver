using Models;

namespace Contracts.Repositorys
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
