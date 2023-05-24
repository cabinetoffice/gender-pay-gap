using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveUserContactX : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_ContactEmailAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContactEmailAddress",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContactFirstName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContactJobTitle",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContactLastName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ContactOrganisation",
                table: "Users");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmailAddress",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactFirstName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactJobTitle",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactLastName",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactOrganisation",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ContactEmailAddress",
                table: "Users",
                column: "ContactEmailAddress");
        }
    }
}
