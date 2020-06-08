using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class AddUserSettingsColumnsToUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.SqlServer")
            {
                migrationBuilder.AddColumn<bool>(
                    name: "SendUpdates",
                    table: "Users",
                    type: "bit",
                    nullable: false,
                    defaultValue: false);

                migrationBuilder.AddColumn<bool>(
                    name: "AllowContact",
                    table: "Users",
                    type: "bit",
                    nullable: false,
                    defaultValue: false);

                migrationBuilder.AddColumn<DateTime>(
                    name: "AcceptedPrivacyStatement",
                    table: "Users",
                    nullable: true);

                migrationBuilder.Sql(
                    "UPDATE Users"
                    + " SET SendUpdates = 1" // '1' here means 'true' in SQL Server dialect 
                    + " WHERE UserId IN"
                    + " ("
                    + "     SELECT UserId"
                    + "     FROM UserSettings"
                    + "     WHERE [Key] = 1 AND [Value] = 'True'"
                    + " )"
                    );

                migrationBuilder.Sql(
                    "UPDATE Users"
                    + " SET AllowContact = 1" // '1' here means 'true' in SQL Server dialect 
                    + " WHERE UserId IN"
                    + " ("
                    + "     SELECT UserId"
                    + "     FROM UserSettings"
                    + "     WHERE [Key] = 2 AND [Value] = 'True'"
                    + " )"
                    );

                // Convert the 'AcceptedPrivacyStatement' UserSettings (Key = 4) into a standard datetime format
                migrationBuilder.Sql(
                    "UPDATE UserSettings"
                    + " SET [Value] = (SUBSTRING([Value], 7, 4) + '-' + SUBSTRING([Value], 4, 2) + '-' + SUBSTRING([Value], 1, 2) + 'T' + SUBSTRING([Value], 12, 8) + 'Z')"
                    + " WHERE [Key] = 4"
                    + " AND [Value] LIKE '[0-9][0-9]/[0-9][0-9]/[0-9][0-9][0-9][0-9]_[0-9][0-9]:[0-9][0-9]:[0-9][0-9]'"
                    );
                migrationBuilder.Sql(
                    "UPDATE UserSettings"
                    + " SET [Value] = (SUBSTRING([Value], 1, 10) + 'T' + SUBSTRING([Value], 12, 8) + 'Z')"
                    + " WHERE [Key] = 4"
                    + " AND [Value] LIKE '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]_[0-9][0-9]:[0-9][0-9]:[0-9][0-9]'"
                    );

                migrationBuilder.Sql(
                    "UPDATE Users"
                    + " SET Users.AcceptedPrivacyStatement ="
                    + "   CASE"
                    + "   WHEN ISDATE(UserSettings.[Value]) = 1 THEN CONVERT(DATETIME, UserSettings.[Value], 127)"
                    + "   ELSE NULL END"
                    + " FROM Users"
                    + " JOIN UserSettings"
                    + "   ON Users.UserId = UserSettings.UserId"
                    + "   AND UserSettings.[Key] = 4"
                    );
            }
            else if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
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
