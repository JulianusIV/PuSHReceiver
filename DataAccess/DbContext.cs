using Configuration;
using Contracts.DbContext;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext, IDbContext
    {
        public DbSet<Lease> Leases => Set<Lease>();
        public DbSet<User> Users => Set<User>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySQL(ReadConfiguration.Config.ConnectionString);
    }
}
