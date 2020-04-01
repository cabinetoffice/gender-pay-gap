using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class AddInactiveUserOrganisationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InactiveUserOrganisations",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    PIN = table.Column<string>(nullable: true),
                    PINSentDate = table.Column<DateTime>(nullable: true),
                    PITPNotifyLetterId = table.Column<string>(nullable: true),
                    PINConfirmedDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttemptDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttempts = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    AddressId = table.Column<long>(nullable: true),
                    MethodId = table.Column<int>(nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.InactiveUserOrganisations", x => new { x.UserId, x.OrganisationId });
                    table.ForeignKey(
                        name: "FK_InactiveUserOrganisations_OrganisationAddresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "OrganisationAddresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InactiveUserOrganisations_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InactiveUserOrganisations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressId",
                table: "InactiveUserOrganisations",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationId",
                table: "InactiveUserOrganisations",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserId",
                table: "InactiveUserOrganisations",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InactiveUserOrganisations");
        }
    }
}
