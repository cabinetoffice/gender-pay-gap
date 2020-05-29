using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GenderPayGap.Database.Migrations
{
    public partial class UpgradeToDotNetCore31 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.AlterColumn<long>(
                    name: "UserStatusId",
                    table: "UserStatus",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "UserId",
                    table: "Users",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "ReturnStatusId",
                    table: "ReturnStatus",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "ReturnId",
                    table: "Returns",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "ReminderEmailId",
                    table: "ReminderEmails",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<int>(
                    name: "PublicSectorTypeId",
                    table: "PublicSectorTypes",
                    nullable: false,
                    oldClrType: typeof(int))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationStatusId",
                    table: "OrganisationStatus",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationSicCodeId",
                    table: "OrganisationSicCodes",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationScopeId",
                    table: "OrganisationScopes",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationId",
                    table: "Organisations",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationReferenceId",
                    table: "OrganisationReferences",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationPublicSectorTypeId",
                    table: "OrganisationPublicSectorTypes",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationNameId",
                    table: "OrganisationNames",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "AddressId",
                    table: "OrganisationAddresses",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "FeedbackId",
                    table: "Feedback",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "AuditLogId",
                    table: "AuditLogs",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "AddressStatusId",
                    table: "AddressStatus",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                migrationBuilder.AlterColumn<long>(
                    name: "UserStatusId",
                    table: "UserStatus",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "UserId",
                    table: "Users",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "ReturnStatusId",
                    table: "ReturnStatus",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "ReturnId",
                    table: "Returns",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "ReminderEmailId",
                    table: "ReminderEmails",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<int>(
                    name: "PublicSectorTypeId",
                    table: "PublicSectorTypes",
                    nullable: false,
                    oldClrType: typeof(int))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationStatusId",
                    table: "OrganisationStatus",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationSicCodeId",
                    table: "OrganisationSicCodes",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationScopeId",
                    table: "OrganisationScopes",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationId",
                    table: "Organisations",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationReferenceId",
                    table: "OrganisationReferences",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationPublicSectorTypeId",
                    table: "OrganisationPublicSectorTypes",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "OrganisationNameId",
                    table: "OrganisationNames",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "AddressId",
                    table: "OrganisationAddresses",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "FeedbackId",
                    table: "Feedback",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "AuditLogId",
                    table: "AuditLogs",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                migrationBuilder.AlterColumn<long>(
                    name: "AddressStatusId",
                    table: "AddressStatus",
                    nullable: false,
                    oldClrType: typeof(long))
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                    .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            }

        }
    }
}
