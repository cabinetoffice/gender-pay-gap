using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class SetOptOutValue : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE Organisations SET OptedOutFromCompaniesHouseUpdate = 'true'"
                + " where OrganisationId IN (13671, 15264)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Organisations SET OptedOutFromCompaniesHouseUpdate = 'false'");
        }

    }
}
