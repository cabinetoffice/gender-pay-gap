using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class RemoveMigrationsHistoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            {
                migrationBuilder.DropTable( 
                    name: "__MigrationHistory"); 
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
