using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveAddressStatusTableAndAddressModifiedColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressStatus");

            migrationBuilder.DropColumn(
                name: "Modified",
                table: "OrganisationAddresses");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Modified",
                table: "OrganisationAddresses",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "AddressStatus",
                columns: table => new
                {
                    AddressStatusId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AddressId = table.Column<long>(type: "bigint", nullable: false),
                    ByUserId = table.Column<long>(type: "bigint", nullable: false),
                    StatusId = table.Column<byte>(type: "smallint", nullable: false),
                    StatusDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusDetails = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.AddressStatus", x => x.AddressStatusId);
                    table.ForeignKey(
                        name: "FK_dbo.AddressStatus_dbo.OrganisationAddresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "OrganisationAddresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AddressStatus_Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddressStatus_AddressId",
                table: "AddressStatus",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressStatus_ByUserId",
                table: "AddressStatus",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressStatus_StatusDate",
                table: "AddressStatus",
                column: "StatusDate");
        }
    }
}
