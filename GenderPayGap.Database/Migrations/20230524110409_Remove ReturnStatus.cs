using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveReturnStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReturnStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReturnStatus",
                columns: table => new
                {
                    ReturnStatusId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ByUserId = table.Column<long>(type: "bigint", nullable: false),
                    ReturnId = table.Column<long>(type: "bigint", nullable: false),
                    StatusId = table.Column<byte>(type: "smallint", nullable: false),
                    StatusDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusDetails = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ReturnStatus", x => x.ReturnStatusId);
                    table.ForeignKey(
                        name: "FK_ReturnStatus_Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.ReturnStatus_dbo.Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReturnStatus_ByUserId",
                table: "ReturnStatus",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnStatus_ReturnId",
                table: "ReturnStatus",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnStatus_StatusDate",
                table: "ReturnStatus",
                column: "StatusDate");
        }
    }
}
