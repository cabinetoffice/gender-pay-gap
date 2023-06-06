using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GenderPayGap.Database.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GenderPayGap.Database
{
    public partial class GpgDatabaseContext : IDbContext
    {
        
        public static string ConnectionString = @"Server=127.0.0.1;Port=5432;Database=GpgDatabase;User Id=gpg_user;Password=local_gpg_database;";

        public GpgDatabaseContext()
        {
            // This empty constructor is needed for Entity Framework to create migrations
            // When you run `dotnet ef migrations add "MigrationName"` it will fail without this constructor
        }
        
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

            Database.SetCommandTimeout(TimeSpan.FromSeconds(90));
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


        public virtual DbSet<Organisation> Organisation { get; set; }
        public virtual DbSet<OrganisationAddress> OrganisationAddress { get; set; }
        public virtual DbSet<OrganisationName> OrganisationName { get; set; }
        public virtual DbSet<OrganisationScope> OrganisationScope { get; set; }
        public virtual DbSet<OrganisationSicCode> OrganisationSicCode { get; set; }
        public virtual DbSet<OrganisationStatus> OrganisationStatus { get; set; }
        public virtual DbSet<Return> Return { get; set; }
        public virtual DbSet<SicCode> SicCodes { get; set; }
        public virtual DbSet<SicSection> SicSections { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserOrganisation> UserOrganisations { get; set; }
        public virtual DbSet<InactiveUserOrganisation> InactiveUserOrganisations { get; set; }
        public virtual DbSet<UserStatus> UserStatuses { get; set; }
        public virtual DbSet<PublicSectorType> PublicSectorTypes { get; set; }
        public virtual DbSet<OrganisationPublicSectorType> OrganisationPublicSectorTypes { get; set; }
        public virtual DbSet<Feedback> Feedback { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }
        public virtual DbSet<ReminderEmail> ReminderEmails { get; set; }
        public virtual DbSet<DraftReturn> DraftReturns { get; set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; } // This is used to store "data protection" keys - used in anti-forgery tokens


        public void SaveChanges()
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

            base.SaveChanges();
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
                optionsBuilder.UseNpgsql(ConnectionString, options => options.EnableRetryOnFailure());
            }

            //Use lazy loading for related virtual items
            optionsBuilder.UseLazyLoadingProxies();
        }

    }

}
