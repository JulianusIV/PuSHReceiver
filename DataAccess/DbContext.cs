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
        public DbSet<Role> Roles => Set<Role>();

        //ensure latest migration is applied to the current db
        public DbContext()
            => Database.Migrate();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //AutoInclude to ensure navigation properies get loaded from DB correctly
            modelBuilder.Entity<Lease>().Navigation(e => e.Owner).AutoInclude();
            modelBuilder.Entity<User>().Navigation(e => e.Roles).AutoInclude();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySql(ConfigurationManager.DbConfig.ConnectionString, ServerVersion.AutoDetect(ConfigurationManager.DbConfig.ConnectionString));
    }
}
