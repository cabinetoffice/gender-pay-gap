using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class AddFeedbackTable : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "Feedback",
                table => new {
                    FeedbackId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Difficulty = table.Column<int>(nullable: true),
                    NewsArticle = table.Column<bool>(nullable: true),
                    SocialMedia = table.Column<bool>(nullable: true),
                    CompanyIntranet = table.Column<bool>(nullable: true),
                    EmployerUnion = table.Column<bool>(nullable: true),
                    InternetSearch = table.Column<bool>(nullable: true),
                    Charity = table.Column<bool>(nullable: true),
                    LobbyGroup = table.Column<bool>(nullable: true),
                    Report = table.Column<bool>(nullable: true),
                    OtherSource = table.Column<bool>(nullable: true),
                    FindOutAboutGpg = table.Column<bool>(nullable: true),
                    ReportOrganisationGpgData = table.Column<bool>(nullable: true),
                    CloseOrganisationGpg = table.Column<bool>(nullable: true),
                    ViewSpecificOrganisationGpg = table.Column<bool>(nullable: true),
                    ActionsToCloseGpg = table.Column<bool>(nullable: true),
                    OtherReason = table.Column<bool>(nullable: true),
                    EmployeeInterestedInOrganisationData = table.Column<bool>(nullable: true),
                    ManagerInvolvedInGpgReport = table.Column<bool>(nullable: true),
                    ResponsibleForReportingGpg = table.Column<bool>(nullable: true),
                    PersonInterestedInGeneralGpg = table.Column<bool>(nullable: true),
                    PersonInterestedInSpecificOrganisationGpg = table.Column<bool>(nullable: true),
                    OtherPerson = table.Column<bool>(nullable: true),
                    Details = table.Column<string>(maxLength: 2000, nullable: true),
                    EmailAddress = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()")
                },
                constraints: table => { table.PrimaryKey("PK_Feedback", x => x.FeedbackId); });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("Feedback");
        }

    }
}
