using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class ChangeSectorColumnToSectorTypeId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganisationSector_Organisations_OrganisationId",
                table: "OrganisationSector");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OrganisationSector",
                table: "OrganisationSector");

            migrationBuilder.RenameColumn(
                name: "Sector",
                table: "OrganisationSector",
                newName: "SectorTypeId");

            migrationBuilder.AlterColumn<string>(
                name: "SectorDetails",
                table: "OrganisationSector",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_dbo.OrganisationSector",
                table: "OrganisationSector",
                column: "OrganisationSectorId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSector_SectorDate",
                table: "OrganisationSector",
                column: "SectorDate");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.OrganisationSector_dbo.Organisations_OrganisationId",
                table: "OrganisationSector",
                column: "OrganisationId",
                principalTable: "Organisations",
                principalColumn: "OrganisationId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.OrganisationSector_dbo.Organisations_OrganisationId",
                table: "OrganisationSector");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.OrganisationSector",
                table: "OrganisationSector");

            migrationBuilder.DropIndex(
                name: "IX_OrganisationSector_SectorDate",
                table: "OrganisationSector");

            migrationBuilder.RenameColumn(
                name: "SectorTypeId",
                table: "OrganisationSector",
                newName: "Sector");

            migrationBuilder.AlterColumn<string>(
                name: "SectorDetails",
                table: "OrganisationSector",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_OrganisationSector",
                table: "OrganisationSector",
                column: "OrganisationSectorId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrganisationSector_Organisations_OrganisationId",
                table: "OrganisationSector",
                column: "OrganisationId",
                principalTable: "Organisations",
                principalColumn: "OrganisationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
