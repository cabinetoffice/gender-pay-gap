using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveViewsForTomsApp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[OrganisationSubmissionInfoView]");
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[OrganisationScopeAndReturnInfoView]");
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[UserLinkedOrganisationsView]");
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[OrganisationInfoView]");
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[UserStatusInfoView]");
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[UserInfoView]");
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[OrganisationSicCodeInfoView]");
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[OrganisationSearchInfoView]");
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[OrganisationScopeInfoView]");
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[OrganisationRegistrationInfoView]");
                migrationBuilder.Sql("DROP VIEW IF EXISTS dbo.[OrganisationAddressInfoView]");

                migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.[OrganisationSectorTypeIdToString]");
                migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.[OrganisationStatusIdToString]");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
