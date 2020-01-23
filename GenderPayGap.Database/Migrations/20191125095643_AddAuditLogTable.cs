using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class AddAuditLogTable : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "AuditLogs",
                table => new {
                    AuditLogId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Action = table.Column<int>(),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "getdate()"),
                    OrganisationId = table.Column<long>(nullable: true),
                    OriginalUserId = table.Column<long>(nullable: true),
                    ImpersonatedUserId = table.Column<long>(nullable: true),
                    DetailsString = table.Column<string>(nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        "FK_AuditLogs_Users_ImpersonatedUserId",
                        x => x.ImpersonatedUserId,
                        "Users",
                        "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_AuditLogs_Organisations_OrganisationId",
                        x => x.OrganisationId,
                        "Organisations",
                        "OrganisationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_AuditLogs_Users_OriginalUserId",
                        x => x.OriginalUserId,
                        "Users",
                        "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                "IX_AuditLogs_ImpersonatedUserId",
                "AuditLogs",
                "ImpersonatedUserId");

            migrationBuilder.CreateIndex(
                "IX_AuditLogs_OrganisationId",
                "AuditLogs",
                "OrganisationId");

            migrationBuilder.CreateIndex(
                "IX_AuditLogs_OriginalUserId",
                "AuditLogs",
                "OriginalUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("AuditLogs");
        }

    }
}
