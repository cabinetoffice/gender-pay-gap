using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class AddOtherTextToFeedbackTable : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                "OtherSourceText",
                "Feedback",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                "OtherReasonText",
                "Feedback",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                "OtherPersonText",
                "Feedback",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                "OtherSourceText",
                "Feedback");

            migrationBuilder.DropColumn(
                "OtherReasonText",
                "Feedback");

            migrationBuilder.DropColumn(
                "OtherPersonText",
                "Feedback");
        }

    }
}
