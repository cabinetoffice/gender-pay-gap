using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using GenderPayGap.Database.Classes;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace GenderPayGap.Database
{
    public partial class GpgDatabaseContext : DbContext
    {

        private const string expectedOldMigration = "201903261305482_Modify View OrganisationSubmissionInfoView";
        private static bool MigrationEnsured;
        private RelationalDatabaseCreator DatabaseCreator => this.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
        private bool DatabaseExists => DatabaseCreator.Exists();

        private void EnsureMigrated()
        {
            if (MigrationEnsured)
            {
                return; //This static variable is a temporary measure otherwise each request for a Database context takes a few seconds to check for migrations or if the database exists
            }

            if (DatabaseExists)
            {
                Upgrade();
            }

            Database.Migrate();
            MigrationEnsured = true;
        }

        /// <summary>
        ///     This method is used only when upgrading from EF 6 to EF Core 7
        ///     It checks if there is an old migration history table and if so creates the new hostory table and prepollates with
        ///     the initial migration.
        ///     If the old history doesnt exist it is then left to the normal migration to recreate the database and its history
        /// </summary>
        protected void Upgrade()
        {
            using (IDbConnection connection = new SqlConnection(ConnectionString))
            {
                //Don't upgrade if there isn't an existing migration history in the old format
                bool oldHistoryExists = connection.TableCount("__MigrationHistory") > 0;
                if (!oldHistoryExists)
                {
                    return;
                }

                string lastOldMigration = connection
                    .SqlQuery<string>("SELECT TOP 1 [MigrationId] FROM [dbo].[__MigrationHistory] ORDER BY [MigrationId] DESC;")
                    .FirstOrDefault();
                if (lastOldMigration != expectedOldMigration)
                {
                    throw new Exception(
                        $"Cannot upgrade to database with last migration '{lastOldMigration}'. You must amend the EntityFramework Core project to include all migrations since '{expectedOldMigration}'");
                }

                //Create the new history table if it doesnt exist
                bool newHistoryExists = connection.TableCount("__EFMigrationsHistory") > 0;
                if (!newHistoryExists)
                {
                    newHistoryExists = connection.SqlQuery<int>(
                                               "CREATE TABLE [dbo].[__EFMigrationsHistory] ([MigrationId] NVARCHAR(150) NOT NULL, [ProductVersion] NVARCHAR(32)  NOT NULL);SELECT Count(*) FROM sys.tables t JOIN sys.schemas s ON t.schema_id = s.schema_id WHERE s.name = 'dbo' AND t.name = '__EFMigrationsHistory';")
                                           .FirstOrDefault()
                                           .ToInt32()
                                       > 0;
                }

                if (!newHistoryExists)
                {
                    throw new Exception("Could not create new Migration History Table '__EFMigrationsHistory'");
                }

                //Populate the new history table
                newHistoryExists = connection.SqlQuery<int>(
                                           "SELECT COUNT(*) FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId]=N'20181104183728_InitialCreate' AND [ProductVersion]=N'2.2.0-rtm-35687';")
                                       .FirstOrDefault()
                                       .ToInt32()
                                   > 0;
                if (!newHistoryExists)
                {
                    newHistoryExists = connection.SqlQuery<int>(
                                               "INSERT INTO [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20181104183728_InitialCreate', N'2.2.0-rtm-35687');SELECT COUNT(*) FROM [dbo].[__EFMigrationsHistory] WHERE [MigrationId]=N'20181104183728_InitialCreate' AND [ProductVersion]=N'2.2.0-rtm-35687';")
                                           .FirstOrDefault()
                                           .ToInt32()
                                       > 0;
                }

                if (!newHistoryExists)
                {
                    throw new Exception("Could not create new Migration History record in Table '__EFMigrationsHistory'");
                }
            }
        }

    }

}
