using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveOrganisationScopeContactX : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmailAddress",
                table: "OrganisationScopes");

            migrationBuilder.DropColumn(
                name: "ContactFirstname",
                table: "OrganisationScopes");

            migrationBuilder.DropColumn(
                name: "ContactLastname",
                table: "OrganisationScopes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmailAddress",
                table: "OrganisationScopes",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactFirstname",
                table: "OrganisationScopes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactLastname",
                table: "OrganisationScopes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
