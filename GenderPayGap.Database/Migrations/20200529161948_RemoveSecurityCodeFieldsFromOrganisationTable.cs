using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveSecurityCodeFieldsFromOrganisationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecurityCode",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "SecurityCodeCreatedDateTime",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "SecurityCodeExpiryDateTime",
                table: "Organisations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecurityCode",
                table: "Organisations",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SecurityCodeCreatedDateTime",
                table: "Organisations",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SecurityCodeExpiryDateTime",
                table: "Organisations",
                type: "timestamp without time zone",
                nullable: true);
        }
    }
}
