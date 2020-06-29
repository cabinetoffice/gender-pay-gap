using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class AddDraftReturnTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DraftReturns",
                columns: table => new
                {
                    DraftReturnId = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrganisationId = table.Column<long>(nullable: false),
                    SnapshotYear = table.Column<int>(nullable: false),
                    DiffMeanHourlyPayPercent = table.Column<decimal>(nullable: true),
                    DiffMedianHourlyPercent = table.Column<decimal>(nullable: true),
                    DiffMeanBonusPercent = table.Column<decimal>(nullable: true),
                    DiffMedianBonusPercent = table.Column<decimal>(nullable: true),
                    MaleMedianBonusPayPercent = table.Column<decimal>(nullable: true),
                    FemaleMedianBonusPayPercent = table.Column<decimal>(nullable: true),
                    MaleLowerPayBand = table.Column<decimal>(nullable: true),
                    FemaleLowerPayBand = table.Column<decimal>(nullable: true),
                    MaleMiddlePayBand = table.Column<decimal>(nullable: true),
                    FemaleMiddlePayBand = table.Column<decimal>(nullable: true),
                    MaleUpperPayBand = table.Column<decimal>(nullable: true),
                    FemaleUpperPayBand = table.Column<decimal>(nullable: true),
                    MaleUpperQuartilePayBand = table.Column<decimal>(nullable: true),
                    FemaleUpperQuartilePayBand = table.Column<decimal>(nullable: true),
                    ReturnId = table.Column<long>(nullable: true),
                    EncryptedOrganisationId = table.Column<string>(nullable: true),
                    SectorType = table.Column<int>(nullable: true),
                    AccountingDate = table.Column<DateTime>(nullable: true),
                    Modified = table.Column<DateTime>(nullable: false),
                    JobTitle = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    CompanyLinkToGPGInfo = table.Column<string>(nullable: true),
                    ReturnUrl = table.Column<string>(nullable: true),
                    OriginatingAction = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    LatestAddress = table.Column<string>(nullable: true),
                    OrganisationName = table.Column<string>(nullable: true),
                    LatestOrganisationName = table.Column<string>(nullable: true),
                    OrganisationSize = table.Column<int>(nullable: true),
                    Sector = table.Column<string>(nullable: true),
                    LatestSector = table.Column<string>(nullable: true),
                    IsDifferentFromDatabase = table.Column<bool>(nullable: true),
                    IsVoluntarySubmission = table.Column<bool>(nullable: true),
                    IsLateSubmission = table.Column<bool>(nullable: true),
                    ShouldProvideLateReason = table.Column<bool>(nullable: true),
                    IsInScopeForThisReportYear = table.Column<bool>(nullable: true),
                    LateReason = table.Column<string>(nullable: true),
                    EHRCResponse = table.Column<string>(nullable: true),
                    LastWrittenDateTime = table.Column<DateTime>(nullable: true),
                    LastWrittenByUserId = table.Column<long>(nullable: true),
                    HasDraftBeenModifiedDuringThisSession = table.Column<bool>(nullable: true),
                    ReportingStartDate = table.Column<DateTime>(nullable: true),
                    ReportModifiedDate = table.Column<DateTime>(nullable: true),
                    ReportingRequirement = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftReturns", x => x.DraftReturnId);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftReturns");
        }
    }
}
