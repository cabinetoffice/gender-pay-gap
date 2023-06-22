using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveObsoleteFieldsFromDraftReturn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountingDate",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "EHRCResponse",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "EncryptedOrganisationId",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "HasDraftBeenModifiedDuringThisSession",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "IsDifferentFromDatabase",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "IsInScopeForThisReportYear",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "IsLateSubmission",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "IsVoluntarySubmission",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "LastWrittenByUserId",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "LastWrittenDateTime",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "LateReason",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "LatestAddress",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "LatestOrganisationName",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "LatestSector",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "OrganisationName",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "OriginatingAction",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "ReportModifiedDate",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "ReportingRequirement",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "ReportingStartDate",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "ReturnId",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "ReturnUrl",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "Sector",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "SectorType",
                table: "DraftReturns");

            migrationBuilder.DropColumn(
                name: "ShouldProvideLateReason",
                table: "DraftReturns");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccountingDate",
                table: "DraftReturns",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EHRCResponse",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EncryptedOrganisationId",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasDraftBeenModifiedDuringThisSession",
                table: "DraftReturns",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDifferentFromDatabase",
                table: "DraftReturns",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInScopeForThisReportYear",
                table: "DraftReturns",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLateSubmission",
                table: "DraftReturns",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVoluntarySubmission",
                table: "DraftReturns",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastWrittenByUserId",
                table: "DraftReturns",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWrittenDateTime",
                table: "DraftReturns",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LateReason",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LatestAddress",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LatestOrganisationName",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LatestSector",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganisationName",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginatingAction",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReportModifiedDate",
                table: "DraftReturns",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportingRequirement",
                table: "DraftReturns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReportingStartDate",
                table: "DraftReturns",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReturnId",
                table: "DraftReturns",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReturnUrl",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "DraftReturns",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SectorType",
                table: "DraftReturns",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShouldProvideLateReason",
                table: "DraftReturns",
                type: "boolean",
                nullable: true);
        }
    }
}
