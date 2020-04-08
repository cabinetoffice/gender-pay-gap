using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class SetMisleadingPropertiesToNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE [dbo].[Organisations]"
                + "SET"
                + "    [LatestAddressId] = NULL,"
                + "    [LatestRegistration_OrganisationId] = NULL,"
                + "    [LatestRegistration_UserId] = NULL,"
                + "    [LatestReturnId] = NULL,"
                + "    [LatestScopeId] = NULL"
                );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // There's no way to sensibly reverse this migration
        }
    }
}
