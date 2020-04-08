using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class RemoveMisleadingProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.AddressStatus_dbo.Users_ByUserId",
                table: "AddressStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationAddresses_LatestAddressId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Organisations_dbo.Returns_LatestReturnId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationScopes_LatestScopeId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Organisations_dbo.UserOrganisations_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                table: "Organisations");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.OrganisationStatus_dbo.Users_ByUserId",
                table: "OrganisationStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ReturnStatus_dbo.Users_ByUserId",
                table: "ReturnStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.UserStatus_dbo.Users_ByUserId",
                table: "UserStatus");

            migrationBuilder.DropIndex(
                name: "IX_LatestAddressId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_LatestReturnId",
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
                name: "LatestReturnId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "LatestScopeId",
                table: "Organisations");

            migrationBuilder.AddForeignKey(
                name: "FK_AddressStatus_Users_ByUserId",
                table: "AddressStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationStatus_Users_ByUserId",
                table: "OrganisationStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnStatus_Users_ByUserId",
                table: "ReturnStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserStatus_Users_ByUserId",
                table: "UserStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddressStatus_Users_ByUserId",
                table: "AddressStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationStatus_Users_ByUserId",
                table: "OrganisationStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_ReturnStatus_Users_ByUserId",
                table: "ReturnStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_UserStatus_Users_ByUserId",
                table: "UserStatus");

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
                name: "LatestReturnId",
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
                name: "IX_LatestReturnId",
                table: "Organisations",
                column: "LatestReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_LatestScopeId",
                table: "Organisations",
                column: "LatestScopeId");

            migrationBuilder.CreateIndex(
                name: "IX_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                table: "Organisations",
                columns: new[] { "LatestRegistration_UserId", "LatestRegistration_OrganisationId" });

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.AddressStatus_dbo.Users_ByUserId",
                table: "AddressStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.OrganisationAddresses_LatestAddressId",
                table: "Organisations",
                column: "LatestAddressId",
                principalTable: "OrganisationAddresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.Returns_LatestReturnId",
                table: "Organisations",
                column: "LatestReturnId",
                principalTable: "Returns",
                principalColumn: "ReturnId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.OrganisationStatus_dbo.Users_ByUserId",
                table: "OrganisationStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ReturnStatus_dbo.Users_ByUserId",
                table: "ReturnStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.UserStatus_dbo.Users_ByUserId",
                table: "UserStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
