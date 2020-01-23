﻿using Microsoft.EntityFrameworkCore;
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

            //Setup the SQL server with automatic retry on failure
            optionsBuilder.UseSqlServer(GpgDatabaseContext.ConnectionString, options => options.EnableRetryOnFailure());
            return new GpgDatabaseContext(optionsBuilder.Options);
        }

    }
}
