using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class AddSynonymsToSicCodesTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Synonyms",
                table: "SicCodes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Synonyms",
                table: "SicCodes");
        }
    }
}
