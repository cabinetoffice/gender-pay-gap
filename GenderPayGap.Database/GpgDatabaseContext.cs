using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GenderPayGap.Database
{
    public partial class GpgDatabaseContext : IDbContext
    {
        
        // Switches between using Postgres and SQL Server for local DB during development
        public static string ConnectionString = Global.UsePostgresDb
            ? @"Server=127.0.0.1;Port=5432;Database=GpgDatabase;User Id=gpg_user;Password=local_gpg_database;" 
            : @"Server=(localdb)\ProjectsV13;Database=GpgDatabase;Trusted_Connection=True;";
        
        public GpgDatabaseContext(string connectionString = null, bool useMigrations = false)
        {
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                ConnectionString = connectionString;
            }

            if (useMigrations)
            {
                EnsureMigrated();
            }
        }

        public GpgDatabaseContext(DbContextOptions<GpgDatabaseContext> options, bool useMigrations = false) : base(options)
        {
            if (useMigrations)
            {
                EnsureMigrated();
            }
        }
        
        private static bool MigrationEnsured;

        private void EnsureMigrated()
        {
            if (MigrationEnsured)
            {
                //This static variable is a temporary measure otherwise each request for a Database context takes a few seconds to
                //check for migrations or if the database exists
                return; 
            }
            
            Database.Migrate();
            MigrationEnsured = true;
        }


        public virtual DbSet<AddressStatus> AddressStatus { get; set; }
        public virtual DbSet<Organisation> Organisation { get; set; }
        public virtual DbSet<OrganisationAddress> OrganisationAddress { get; set; }
        public virtual DbSet<OrganisationName> OrganisationName { get; set; }
        public virtual DbSet<OrganisationReference> OrganisationReference { get; set; }
        public virtual DbSet<OrganisationScope> OrganisationScope { get; set; }
        public virtual DbSet<OrganisationSicCode> OrganisationSicCode { get; set; }
        public virtual DbSet<OrganisationStatus> OrganisationStatus { get; set; }
        public virtual DbSet<Return> Return { get; set; }
        public virtual DbSet<ReturnStatus> ReturnStatus { get; set; }
        public virtual DbSet<SicCode> SicCodes { get; set; }
        public virtual DbSet<SicSection> SicSections { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserOrganisation> UserOrganisations { get; set; }
        public virtual DbSet<InactiveUserOrganisation> InactiveUserOrganisations { get; set; }
        public virtual DbSet<UserSetting> UserSettings { get; set; }
        public virtual DbSet<UserStatus> UserStatuses { get; set; }
        public virtual DbSet<PublicSectorType> PublicSectorTypes { get; set; }
        public virtual DbSet<OrganisationPublicSectorType> OrganisationPublicSectorTypes { get; set; }
        public virtual DbSet<Feedback> Feedback { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }
        public virtual DbSet<ReminderEmail> ReminderEmails { get; set; }

        public async Task<int> SaveChangesAsync()
        {
            #region Validate the new or changed entities

            IEnumerable<object> entities = from e in ChangeTracker.Entries()
                where e.State == EntityState.Added
                      || e.State == EntityState.Modified
                select e.Entity;

            var innerExceptions = new List<ValidationException>();
            foreach (object entity in entities)
            {
                var validationContext = new ValidationContext(entity);

                try
                {
                    Validator.ValidateObject(entity, validationContext, true);
                }
                catch (ValidationException vex)
                {
                    innerExceptions.Add(vex);
                }
            }

            if (innerExceptions.Any())
            {
                throw new AggregateException(innerExceptions);
            }

            #endregion

            return await base.SaveChangesAsync();
        }

        public DatabaseFacade GetDatabase()
        {
            return Database;
        }

        public new DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return base.Set<TEntity>();
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                if (Global.UsePostgresDb)
                {
                    optionsBuilder.UseNpgsql(ConnectionString, options => options.EnableRetryOnFailure());
                }
                else
                {
                    optionsBuilder.UseSqlServer(ConnectionString, options => options.EnableRetryOnFailure());
                }
            }

            //Use lazy loading for related virtual items
            optionsBuilder.UseLazyLoadingProxies();
        }
        
    }

}
