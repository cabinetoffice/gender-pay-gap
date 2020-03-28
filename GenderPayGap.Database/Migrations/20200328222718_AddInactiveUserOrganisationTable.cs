using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class AddInactiveUserOrganisationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "InactiveUserOrganisationOrganisationId",
                table: "Organisations",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "InactiveUserOrganisationUserId",
                table: "Organisations",
                nullable: true);

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
                        name: "FK_dbo.InactiveUserOrganisations_dbo.OrganisationAddresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "OrganisationAddresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.InactiveUserOrganisations_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.InactiveUserOrganisations_dbo.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_InactiveUserOrganisationUserId_InactiveUserOrganisationOrganisationId",
                table: "Organisations",
                columns: new[] { "InactiveUserOrganisationUserId", "InactiveUserOrganisationOrganisationId" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_Organisations_InactiveUserOrganisations_InactiveUserOrganisationUserId_InactiveUserOrganisationOrganisationId",
                table: "Organisations",
                columns: new[] { "InactiveUserOrganisationUserId", "InactiveUserOrganisationOrganisationId" },
                principalTable: "InactiveUserOrganisations",
                principalColumns: new[] { "UserId", "OrganisationId" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Organisations_InactiveUserOrganisations_InactiveUserOrganisationUserId_InactiveUserOrganisationOrganisationId",
                table: "Organisations");

            migrationBuilder.DropTable(
                name: "InactiveUserOrganisations");

            migrationBuilder.DropIndex(
                name: "IX_Organisations_InactiveUserOrganisationUserId_InactiveUserOrganisationOrganisationId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "InactiveUserOrganisationOrganisationId",
                table: "Organisations");

            migrationBuilder.DropColumn(
                name: "InactiveUserOrganisationUserId",
                table: "Organisations");
        }
    }
}
