using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GenderPayGap.Database.Migrations
{
    public partial class AddOrganisationSectorsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SectorType",
                table: "ReminderEmails");

            migrationBuilder.AddColumn<int>(
                name: "OrganisationSector",
                table: "ReminderEmails",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SectorDate",
                table: "Organisations",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SectorDetails",
                table: "Organisations",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrganisationSector",
                columns: table => new
                {
                    OrganisationSectorId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganisationId = table.Column<long>(nullable: false),
                    Sector = table.Column<int>(nullable: false),
                    SectorDate = table.Column<DateTime>(nullable: false),
                    SectorDetails = table.Column<string>(nullable: true),
                    ByUserId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganisationSector", x => x.OrganisationSectorId);
                    table.ForeignKey(
                        name: "FK_OrganisationSector_Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganisationSector_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSector_ByUserId",
                table: "OrganisationSector",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSector_OrganisationId",
                table: "OrganisationSector",
                column: "OrganisationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganisationSector");

            migrationBuilder.DropColumn(
                name: "OrganisationSector",
                table: "ReminderEmails");

            migrationBuilder.DropColumn(
                name: "SectorDate",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "SectorDetails",
                table: "Organisations");

            migrationBuilder.AddColumn<int>(
                name: "SectorType",
                table: "ReminderEmails",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
