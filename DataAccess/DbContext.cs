﻿using Configuration;
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

        public DbSet<Log> Logs => Set<Log>();

        //ensure latest migration is applied to the current db
        public DbContext()
            => Database.Migrate();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySql(ConfigurationManager.DbConfig.ConnectionString, ServerVersion.AutoDetect(ConfigurationManager.DbConfig.ConnectionString));
    }
}
