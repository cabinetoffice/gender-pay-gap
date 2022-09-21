using System;
using GenderPayGap.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GenderPayGap.Database
{
    /// <summary>
    ///     This class is required for adding migrations and updating the database at design time
    /// </summary>
    public class GpgDatabaseContextFactory : IDesignTimeDbContextFactory<GpgDatabaseContext>
    {

        public GpgDatabaseContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GpgDatabaseContext>();

            // Environment variable used to specify to which DB the migration should be applied.
            // This can either be set locally, to manually specify which DB to update
            // Or in azure devops, depending on the environment.
            var connectionStringFromEnv = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            
            optionsBuilder.UseNpgsql(connectionStringFromEnv ?? GpgDatabaseContext.ConnectionString, options => options.EnableRetryOnFailure());

            return new GpgDatabaseContext(optionsBuilder.Options);
        }

    }
}
