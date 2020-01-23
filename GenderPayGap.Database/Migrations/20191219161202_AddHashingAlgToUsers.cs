using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class AddHashingAlgToUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HashingAlgorithm",
                table: "Users",
                nullable: false,
                // Before we add this column, all passwords are stored using HashingAlgorithm.SHA512 (id 1)
                // By setting "defaultValue: 1" at this point, we are saying that all EXISTING rows use SHA512
                defaultValue: 1);

            migrationBuilder.AlterColumn<int>(
                name: "HashingAlgorithm",
                table: "Users",
                nullable: false,
                // Now, all rows have the value 1 - i.e. all passwords are hashed using HashingAlgorithm.SHA512
                // We don't want new passwords to use SHA512.
                // We want our code to decide what algorithm it is using and to save the appropriate value in the database
                // By setting "defaultValue: null" at this point, we are saying "Do not allow NULL and force the code to specify a value"
                defaultValue: null);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashingAlgorithm",
                table: "Users");
        }
    }
}
