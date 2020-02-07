using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class RemovePINHashFromUserOrganisation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PINHash",
                table: "UserOrganisations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PINHash",
                table: "UserOrganisations",
                maxLength: 250,
                nullable: true);
        }
    }
}
