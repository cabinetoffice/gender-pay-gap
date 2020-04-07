using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class RemoveUnusedRelations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.AddressStatus_dbo.Users_ByUserId",
                table: "AddressStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.OrganisationStatus_dbo.Users_ByUserId",
                table: "OrganisationStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ReturnStatus_dbo.Users_ByUserId",
                table: "ReturnStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.UserStatus_dbo.Users_ByUserId",
                table: "UserStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_AddressStatus_Users_ByUserId",
                table: "AddressStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationStatus_Users_ByUserId",
                table: "OrganisationStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReturnStatus_Users_ByUserId",
                table: "ReturnStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.AddressStatus_dbo.Users_ByUserId",
                table: "AddressStatus",
                column: "ByUserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

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
