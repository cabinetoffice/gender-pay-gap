using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Migrations
{
    public partial class AddNewEmailAddressToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewEmailAddressDB",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NewEmailAddressRequestDate",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewEmailAddressDB",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NewEmailAddressRequestDate",
                table: "Users");
        }
    }
}
