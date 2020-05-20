using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class AllowOrganisationStatusByUserIdToBeNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ByUserId",
                table: "OrganisationStatus",
                nullable: true,
                oldClrType: typeof(long));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "ByUserId",
                table: "OrganisationStatus",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);
        }
    }
}
