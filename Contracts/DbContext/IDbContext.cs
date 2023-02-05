using Microsoft.EntityFrameworkCore;
using Models;

namespace Contracts.DbContext
{
    public interface IDbContext : IDisposable
    {
        public DbSet<Lease> Leases { get; }
        public DbSet<User> Users { get; }
        public int SaveChanges();
    }
}
