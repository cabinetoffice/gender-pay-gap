using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class RemoveOrganisationLatestX : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationAddresses_LatestAddressId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationScopes_LatestScopeId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Organisations_dbo.UserOrganisations_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_LatestAddressId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_LatestScopeId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "LatestAddressId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "LatestRegistration_OrganisationId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "LatestRegistration_UserId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "LatestScopeId",
                table: "Organisations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LatestAddressId",
                table: "Organisations",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LatestRegistration_OrganisationId",
                table: "Organisations",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LatestRegistration_UserId",
                table: "Organisations",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LatestScopeId",
                table: "Organisations",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LatestAddressId",
                table: "Organisations",
                column: "LatestAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_LatestScopeId",
                table: "Organisations",
                column: "LatestScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                table: "Organisations",
                columns: new[] { "LatestRegistration_UserId", "LatestRegistration_OrganisationId" });

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationAddresses_LatestAddressId",
                table: "Organisations",
                column: "LatestAddressId",
                principalTable: "OrganisationAddresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationScopes_LatestScopeId",
                table: "Organisations",
                column: "LatestScopeId",
                principalTable: "OrganisationScopes",
                principalColumn: "OrganisationScopeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.UserOrganisations_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                table: "Organisations",
                columns: new[] { "LatestRegistration_UserId", "LatestRegistration_OrganisationId" },
                principalTable: "UserOrganisations",
                principalColumns: new[] { "UserId", "OrganisationId" },
                onDelete: ReferentialAction.Restrict);
        }
    }
}
