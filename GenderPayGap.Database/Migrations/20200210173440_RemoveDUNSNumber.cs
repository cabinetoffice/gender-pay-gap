using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class RemoveDUNSNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_Organisations_DUNSNumber",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "DUNSNumber",
                table: "Organisations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DUNSNumber",
                table: "Organisations",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_Organisations_DUNSNumber",
                table: "Organisations",
                column: "DUNSNumber",
                unique: true,
                filter: "([DUNSNumber] IS NOT NULL)");
        }
    }
}
