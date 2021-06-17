using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class AddStatusAndReminderDateToReminderEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReminderEmails_UserId",
                table: "ReminderEmails");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderDate",
                table: "ReminderEmails",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "Status",
                table: "ReminderEmails",
                nullable: false,
                defaultValue: (byte)1);

            migrationBuilder.CreateIndex(
                name: "IX_ReminderEmails_UserId_SectorType_ReminderDate",
                table: "ReminderEmails",
                unique: true,
                columns: new[] { "UserId", "SectorType", "ReminderDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReminderEmails_UserId_SectorType_ReminderDate",
                table: "ReminderEmails");

            migrationBuilder.DropColumn(
                name: "ReminderDate",
                table: "ReminderEmails");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ReminderEmails");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderEmails_UserId",
                table: "ReminderEmails",
                column: "UserId");
        }
    }
}
