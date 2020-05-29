using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class AddUserSettingsColumnsToUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SendUpdates",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowContact",
                table: "Users",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedPrivacyStatement",
                table: "Users",
                nullable: true);

            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.Sql(
                    "UPDATE Users"
                    + " SET SendUpdates = 1" // '1' here means 'true' in SQL Server dialect
                    + " WHERE UserId IN"
                    + " ("
                    + "     SELECT UserId"
                    + "     FROM UserSettings"
                    + "     WHERE [Key] = 1 AND [Value] = 'True'"
                    );

                migrationBuilder.Sql(
                    "UPDATE Users"
                    + " SET AllowContact = 1" // '1' here means 'true' in SQL Server dialect
                    + " WHERE UserId IN"
                    + " ("
                    + "     SELECT UserId"
                    + "     FROM UserSettings"
                    + "     WHERE [Key] = 2 AND [Value] = 'True'"
                    );

                migrationBuilder.Sql(
                    "UPDATE Users"
                    + " SET Users.AcceptedPrivacyStatement = CONVERT(DATETIME, UserSettings.[Value])"
                    + " FROM Users"
                    + " JOIN UserSettings"
                    + "   ON Users.UserId = UserSettings.UserId"
                    + "   AND UserSettings.[Key] = 4"
                    );
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedPrivacyStatement",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowContact",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SendUpdates",
                table: "Users");
        }
    }
}
