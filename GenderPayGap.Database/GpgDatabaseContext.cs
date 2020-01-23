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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GenderPayGap.Database
{
    public partial class GpgDatabaseContext : DbContext, IDbContext
    {

        public static string ConnectionString = @"Server=(localdb)\ProjectsV13;Database=GpgDatabase;Trusted_Connection=True;";

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
        public virtual DbSet<UserSetting> UserSettings { get; set; }
        public virtual DbSet<UserStatus> UserStatuses { get; set; }
        public virtual DbSet<PublicSectorType> PublicSectorTypes { get; set; }
        public virtual DbSet<OrganisationPublicSectorType> OrganisationPublicSectorTypes { get; set; }
        public virtual DbSet<Feedback> Feedback { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }

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

        /// <summary>
        ///     https://social.msdn.microsoft.com/Forums/en-US/c369c1f9-828c-480a-b1e3-14677b64a3c0/how-to-update-large-quantity-of-data-in-database-using-c
        /// </summary>
        public void UpdateChangesInBulk<TEntity>(IEnumerable<TEntity> listOfOrganisations) where TEntity : class
        {
            var dataTableOfOrganisations = new DataTable("MyDataTableOfOrganisations");
            dataTableOfOrganisations = ConvertToDataTable(listOfOrganisations);
            using (var conn = new SqlConnection(Global.DatabaseConnectionString))
            {
                using (var command = new SqlCommand(string.Empty, conn))
                {
                    try
                    {
                        conn.Open();

                        var tempTableName = "#TempBulkUpdateTable";

                        // Make sure the temp table doesn't exist
                        command.CommandText = $" IF OBJECT_ID('tempdb..{tempTableName}') IS NOT NULL DROP Table {tempTableName}; ";
                        command.ExecuteNonQuery();

                        // Create the temp table on database
                        command.CommandText = $" CREATE TABLE {tempTableName} (                     "
                                              + "     OrganisationId bigint not null,                 "
                                              + "     SecurityCode nvarchar(max) null,                "
                                              + "     SecurityCodeExpiryDateTime datetime2(7) null,   "
                                              + "     SecurityCodeCreatedDateTime datetime2(7) null   "
                                              + " ); ";
                        command.ExecuteNonQuery();

                        // Bulk insert into temp table
                        using (var bulkcopy = new SqlBulkCopy(conn))
                        {
                            bulkcopy.BulkCopyTimeout = 660;
                            bulkcopy.DestinationTableName = tempTableName;
                            bulkcopy.WriteToServer(dataTableOfOrganisations);
                            bulkcopy.Close();
                        }

                        // Updating destination table
                        command.CommandTimeout = 300;
                        command.CommandText = " Update Organisations                                                "
                                              + " set SecurityCode = tmp.SecurityCode,                                 "
                                              + "     SecurityCodeExpiryDateTime = tmp.SecurityCodeExpiryDateTime,     "
                                              + "     SecurityCodeCreatedDateTime = tmp.SecurityCodeCreatedDateTime    "
                                              + " from Organisations orgs                                              "
                                              + $" inner join {tempTableName} tmp on tmp.OrganisationId = orgs.OrganisationId ";
                        command.ExecuteNonQuery();

                        // Dropping the temp table
                        command.CommandText = $" IF OBJECT_ID('tempdb..{tempTableName}') IS NOT NULL DROP Table {tempTableName}; ";
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        string mesage = ex.Message;
                        // Handle exception properly
                    }
                    finally
                    {
                        conn.Close();
                    }
                }
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                //Setup the SQL server with automatic retry on failure
                optionsBuilder.UseSqlServer(ConnectionString, options => options.EnableRetryOnFailure());
            }

            //Use lazy loading for related virtual items
            optionsBuilder.UseLazyLoadingProxies();
        }

        /// <summary>
        ///     https://stackoverflow.com/questions/564366/convert-generic-list-enumerable-to-datatable
        /// </summary>
        /// <param name="listOfOrganisationsToConvert"></param>
        /// <returns></returns>
        private DataTable ConvertToDataTable<TEntity>(IEnumerable<TEntity> listOfOrganisationsToConvert) where TEntity : class
        {
            var table = new DataTable();

            table.Columns.Add("OrganisationId", typeof(int));
            table.Columns.Add("SecurityCode", typeof(string));
            table.Columns.Add("SecurityCodeExpiryDateTime", typeof(DateTime));
            table.Columns.Add("SecurityCodeCreatedDateTime", typeof(DateTime));

            foreach (TEntity item in listOfOrganisationsToConvert)
            {
                var itemObject = (object) item;
                var orgObject = itemObject as Organisation;

                if (orgObject != null)
                {
                    table.Rows.Add(
                        orgObject.OrganisationId,
                        orgObject.SecurityCode,
                        orgObject.SecurityCodeExpiryDateTime,
                        orgObject.SecurityCodeCreatedDateTime);
                }
            }

            return table;
        }

    }

}
