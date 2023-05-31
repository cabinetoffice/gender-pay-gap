using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveOrganisationReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrganisationReferences");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrganisationReferences",
                columns: table => new
                {
                    OrganisationReferenceId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OrganisationId = table.Column<long>(type: "bigint", nullable: false),
                    ReferenceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReferenceValue = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationReferences", x => x.OrganisationReferenceId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationReferences_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_Created",
                table: "OrganisationReferences",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_OrganisationId",
                table: "OrganisationReferences",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_ReferenceName",
                table: "OrganisationReferences",
                column: "ReferenceName");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_ReferenceValue",
                table: "OrganisationReferences",
                column: "ReferenceValue");
        }
    }
}
