using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class RemoveOrganisationLatestReturn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Organisations_dbo.Returns_LatestReturnId",
                table: "Organisations");

            migrationBuilder.DropIndex(
                name: "IX_LatestReturnId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "LatestReturnId",
                table: "Organisations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LatestReturnId",
                table: "Organisations",
                nullable: true);

            migrationBuilder.CreateIndex(
                "IX_LatestReturnId",
                "Organisations",
                "LatestReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Organisations_dbo.Returns_LatestReturnId",
                table: "Organisations",
                column: "LatestReturnId",
                principalTable: "Returns",
                principalColumn: "ReturnId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
