using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class RemoveUserSettingsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSettings");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSettings",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    Key = table.Column<byte>(type: "smallint", nullable: false),
                    Modified = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.UserSettings", x => new { x.UserId, x.Key });
                    table.ForeignKey(
                        name: "FK_dbo.UserSettings_dbo.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSettings_UserId",
                table: "UserSettings",
                column: "UserId");
        }
    }
}
