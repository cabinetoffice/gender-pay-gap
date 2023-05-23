using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveUserOrganisationAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InactiveUserOrganisations_OrganisationAddresses_AddressId",
                table: "InactiveUserOrganisations");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.UserOrganisations_dbo.OrganisationAddresses_AddressId",
                table: "UserOrganisations");

            migrationBuilder.DropIndex(
                name: "IX_UserOrganisations_AddressId",
                table: "UserOrganisations");

            migrationBuilder.DropIndex(
                name: "IX_InactiveUserOrganisations_AddressId",
                table: "InactiveUserOrganisations");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "UserOrganisations");

            migrationBuilder.DropColumn(
                name: "AddressId",
                table: "InactiveUserOrganisations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AddressId",
                table: "UserOrganisations",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "AddressId",
                table: "InactiveUserOrganisations",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganisations_AddressId",
                table: "UserOrganisations",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_InactiveUserOrganisations_AddressId",
                table: "InactiveUserOrganisations",
                column: "AddressId");

            migrationBuilder.AddForeignKey(
                name: "FK_InactiveUserOrganisations_OrganisationAddresses_AddressId",
                table: "InactiveUserOrganisations",
                column: "AddressId",
                principalTable: "OrganisationAddresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.UserOrganisations_dbo.OrganisationAddresses_AddressId",
                table: "UserOrganisations",
                column: "AddressId",
                principalTable: "OrganisationAddresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
