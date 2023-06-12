using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveOrganisationPublicSectorTypeRetired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrganisationPublicSectorTypes_Retired",
                table: "OrganisationPublicSectorTypes");

            migrationBuilder.DropColumn(
                name: "Retired",
                table: "OrganisationPublicSectorTypes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Retired",
                table: "OrganisationPublicSectorTypes",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationPublicSectorTypes_Retired",
                table: "OrganisationPublicSectorTypes",
                column: "Retired");
        }
    }
}
