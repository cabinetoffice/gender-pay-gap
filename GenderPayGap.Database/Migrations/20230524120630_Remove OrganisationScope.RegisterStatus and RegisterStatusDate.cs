using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveOrganisationScopeRegisterStatusandRegisterStatusDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrganisationScopes_RegisterStatusId",
                table: "OrganisationScopes");

            migrationBuilder.DropColumn(
                name: "RegisterStatusId",
                table: "OrganisationScopes");

            migrationBuilder.DropColumn(
                name: "RegisterStatusDate",
                table: "OrganisationScopes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegisterStatusId",
                table: "OrganisationScopes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisterStatusDate",
                table: "OrganisationScopes",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_RegisterStatusId",
                table: "OrganisationScopes",
                column: "RegisterStatusId");
        }
    }
}
