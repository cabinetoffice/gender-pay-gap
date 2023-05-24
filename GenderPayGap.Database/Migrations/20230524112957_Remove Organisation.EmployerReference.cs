using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveOrganisationEmployerReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Organisations_EmployerReference",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "EmployerReference",
                table: "Organisations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmployerReference",
                table: "Organisations",
                type: "character varying(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_EmployerReference",
                table: "Organisations",
                column: "EmployerReference",
                unique: true);
        }
    }
}
