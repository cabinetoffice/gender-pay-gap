using System;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GenderPayGap.Database.Core21.Migrations
{
    public partial class InitialCreate : Migration
    {

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "SicSections",
                table => new {
                    SicSectionId = table.Column<string>(maxLength: 1),
                    Description = table.Column<string>(maxLength: 250),
                    Created = table.Column<DateTime>()
                },
                constraints: table => { table.PrimaryKey("PK_dbo.SicSections", x => x.SicSectionId); });

            migrationBuilder.CreateTable(
                "Users",
                table => new {
                    UserId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    JobTitle = table.Column<string>(maxLength: 50),
                    Firstname = table.Column<string>(maxLength: 50),
                    Lastname = table.Column<string>(maxLength: 50),
                    EmailAddress = table.Column<string>(maxLength: 255),
                    ContactJobTitle = table.Column<string>(maxLength: 50, nullable: true),
                    ContactFirstName = table.Column<string>(maxLength: 50, nullable: true),
                    ContactLastName = table.Column<string>(maxLength: 50, nullable: true),
                    ContactOrganisation = table.Column<string>(maxLength: 100, nullable: true),
                    ContactEmailAddress = table.Column<string>(maxLength: 255, nullable: true),
                    ContactPhoneNumber = table.Column<string>(maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(maxLength: 250),
                    EmailVerifyHash = table.Column<string>(maxLength: 250, nullable: true),
                    EmailVerifySendDate = table.Column<DateTime>(nullable: true),
                    EmailVerifiedDate = table.Column<DateTime>(nullable: true),
                    StatusId = table.Column<byte>(),
                    StatusDate = table.Column<DateTime>(),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    LoginAttempts = table.Column<int>(),
                    LoginDate = table.Column<DateTime>(nullable: true),
                    ResetSendDate = table.Column<DateTime>(nullable: true),
                    ResetAttempts = table.Column<int>(),
                    VerifyAttemptDate = table.Column<DateTime>(nullable: true),
                    VerifyAttempts = table.Column<int>(),
                    Created = table.Column<DateTime>(),
                    Modified = table.Column<DateTime>()
                },
                constraints: table => { table.PrimaryKey("PK_dbo.Users", x => x.UserId); });

            migrationBuilder.CreateTable(
                "SicCodes",
                table => new {
                    SicCodeId = table.Column<int>(),
                    SicSectionId = table.Column<string>(maxLength: 1),
                    Description = table.Column<string>(maxLength: 250),
                    Created = table.Column<DateTime>()
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.SicCodes", x => x.SicCodeId);
                    table.ForeignKey(
                        "FK_dbo.SicCodes_dbo.SicSections_SicSectionId",
                        x => x.SicSectionId,
                        "SicSections",
                        "SicSectionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                "UserSettings",
                table => new {
                    UserId = table.Column<long>(),
                    Key = table.Column<byte>(),
                    Value = table.Column<string>(maxLength: 50, nullable: true),
                    Modified = table.Column<DateTime>()
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.UserSettings", x => new {x.UserId, x.Key});
                    table.ForeignKey(
                        "FK_dbo.UserSettings_dbo.Users_UserId",
                        x => x.UserId,
                        "Users",
                        "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "UserStatus",
                table => new {
                    UserStatusId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<long>(),
                    StatusId = table.Column<byte>(),
                    StatusDate = table.Column<DateTime>(),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>()
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.UserStatus", x => x.UserStatusId);
                    table.ForeignKey(
                        "FK_dbo.UserStatus_dbo.Users_ByUserId",
                        x => x.ByUserId,
                        "Users",
                        "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_dbo.UserStatus_dbo.Users_UserId",
                        x => x.UserId,
                        "Users",
                        "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "AddressStatus",
                table => new {
                    AddressStatusId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AddressId = table.Column<long>(),
                    StatusId = table.Column<byte>(),
                    StatusDate = table.Column<DateTime>(),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>()
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.AddressStatus", x => x.AddressStatusId);
                    table.ForeignKey(
                        "FK_dbo.AddressStatus_dbo.Users_ByUserId",
                        x => x.ByUserId,
                        "Users",
                        "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Organisations",
                table => new {
                    OrganisationId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CompanyNumber = table.Column<string>(maxLength: 10, nullable: true),
                    OrganisationName = table.Column<string>(maxLength: 100),
                    SectorTypeId = table.Column<int>(),
                    StatusId = table.Column<byte>(),
                    StatusDate = table.Column<DateTime>(),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(),
                    Modified = table.Column<DateTime>(),
                    DUNSNumber = table.Column<string>(maxLength: 10, nullable: true),
                    EmployerReference = table.Column<string>(maxLength: 10, nullable: true),
                    DateOfCessation = table.Column<DateTime>(nullable: true),
                    LatestAddressId = table.Column<long>(nullable: true),
                    LatestReturnId = table.Column<long>(nullable: true),
                    LatestScopeId = table.Column<long>(nullable: true),
                    LatestRegistration_UserId = table.Column<long>(nullable: true),
                    LatestRegistration_OrganisationId = table.Column<long>(nullable: true),
                    SecurityCode = table.Column<string>(nullable: true),
                    SecurityCodeExpiryDateTime = table.Column<DateTime>(nullable: true),
                    SecurityCodeCreatedDateTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_dbo.Organisations", x => x.OrganisationId); });

            migrationBuilder.CreateTable(
                "OrganisationAddresses",
                table => new {
                    AddressId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreatedByUserId = table.Column<long>(),
                    Address1 = table.Column<string>(maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(maxLength: 100, nullable: true),
                    Address3 = table.Column<string>(maxLength: 100, nullable: true),
                    TownCity = table.Column<string>(maxLength: 100, nullable: true),
                    County = table.Column<string>(maxLength: 100, nullable: true),
                    Country = table.Column<string>(maxLength: 100, nullable: true),
                    PoBox = table.Column<string>(maxLength: 30, nullable: true),
                    PostCode = table.Column<string>(maxLength: 20, nullable: true),
                    StatusId = table.Column<byte>(),
                    StatusDate = table.Column<DateTime>(),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(),
                    Modified = table.Column<DateTime>(),
                    OrganisationId = table.Column<long>(),
                    Source = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.OrganisationAddresses", x => x.AddressId);
                    table.ForeignKey(
                        "FK_dbo.OrganisationAddresses_dbo.Organisations_OrganisationId",
                        x => x.OrganisationId,
                        "Organisations",
                        "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "OrganisationNames",
                table => new {
                    OrganisationNameId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrganisationId = table.Column<long>(),
                    Name = table.Column<string>(maxLength: 100),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>()
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.OrganisationNames", x => x.OrganisationNameId);
                    table.ForeignKey(
                        "FK_dbo.OrganisationNames_dbo.Organisations_OrganisationId",
                        x => x.OrganisationId,
                        "Organisations",
                        "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "OrganisationReferences",
                table => new {
                    OrganisationReferenceId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrganisationId = table.Column<long>(),
                    ReferenceName = table.Column<string>(maxLength: 100),
                    ReferenceValue = table.Column<string>(maxLength: 100),
                    Created = table.Column<DateTime>()
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.OrganisationReferences", x => x.OrganisationReferenceId);
                    table.ForeignKey(
                        "FK_dbo.OrganisationReferences_dbo.Organisations_OrganisationId",
                        x => x.OrganisationId,
                        "Organisations",
                        "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "OrganisationScopes",
                table => new {
                    OrganisationScopeId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrganisationId = table.Column<long>(),
                    ScopeStatusId = table.Column<int>(),
                    ScopeStatusDate = table.Column<DateTime>(),
                    RegisterStatusId = table.Column<int>(),
                    RegisterStatusDate = table.Column<DateTime>(),
                    ContactFirstname = table.Column<string>(maxLength: 50, nullable: true),
                    ContactLastname = table.Column<string>(maxLength: 50, nullable: true),
                    ContactEmailAddress = table.Column<string>(maxLength: 255, nullable: true),
                    ReadGuidance = table.Column<bool>(nullable: true),
                    Reason = table.Column<string>(maxLength: 1000, nullable: true),
                    CampaignId = table.Column<string>(maxLength: 50, nullable: true),
                    SnapshotDate = table.Column<DateTime>(nullable: false, defaultValueSql: "('1900-01-01T00:00:00.000')"),
                    StatusId = table.Column<byte>(nullable: false, defaultValueSql: "((0))"),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.OrganisationScopes", x => x.OrganisationScopeId);
                    table.ForeignKey(
                        "FK_dbo.OrganisationScopes_dbo.Organisations_OrganisationId",
                        x => x.OrganisationId,
                        "Organisations",
                        "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "OrganisationSicCodes",
                table => new {
                    SicCodeId = table.Column<int>(),
                    OrganisationId = table.Column<long>(),
                    Created = table.Column<DateTime>(),
                    OrganisationSicCodeId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Retired = table.Column<DateTime>(nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.OrganisationSicCodes", x => x.OrganisationSicCodeId);
                    table.ForeignKey(
                        "FK_dbo.OrganisationSicCodes_dbo.Organisations_OrganisationId",
                        x => x.OrganisationId,
                        "Organisations",
                        "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_dbo.OrganisationSicCodes_dbo.SicCodes_SicCodeId",
                        x => x.SicCodeId,
                        "SicCodes",
                        "SicCodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "OrganisationStatus",
                table => new {
                    OrganisationStatusId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrganisationId = table.Column<long>(),
                    StatusId = table.Column<byte>(),
                    StatusDate = table.Column<DateTime>(),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>()
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.OrganisationStatus", x => x.OrganisationStatusId);
                    table.ForeignKey(
                        "FK_dbo.OrganisationStatus_dbo.Users_ByUserId",
                        x => x.ByUserId,
                        "Users",
                        "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_dbo.OrganisationStatus_dbo.Organisations_OrganisationId",
                        x => x.OrganisationId,
                        "Organisations",
                        "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "Returns",
                table => new {
                    ReturnId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    OrganisationId = table.Column<long>(),
                    AccountingDate = table.Column<DateTime>(),
                    DiffMeanHourlyPayPercent = table.Column<decimal>(),
                    DiffMedianHourlyPercent = table.Column<decimal>(),
                    DiffMeanBonusPercent = table.Column<decimal>(),
                    DiffMedianBonusPercent = table.Column<decimal>(),
                    MaleMedianBonusPayPercent = table.Column<decimal>(),
                    FemaleMedianBonusPayPercent = table.Column<decimal>(),
                    MaleLowerPayBand = table.Column<decimal>(),
                    FemaleLowerPayBand = table.Column<decimal>(),
                    MaleMiddlePayBand = table.Column<decimal>(),
                    FemaleMiddlePayBand = table.Column<decimal>(),
                    MaleUpperPayBand = table.Column<decimal>(),
                    FemaleUpperPayBand = table.Column<decimal>(),
                    MaleUpperQuartilePayBand = table.Column<decimal>(),
                    FemaleUpperQuartilePayBand = table.Column<decimal>(),
                    CompanyLinkToGPGInfo = table.Column<string>(maxLength: 255, nullable: true),
                    StatusId = table.Column<byte>(),
                    StatusDate = table.Column<DateTime>(),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(),
                    Modified = table.Column<DateTime>(),
                    JobTitle = table.Column<string>(maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(maxLength: 50, nullable: true),
                    LastName = table.Column<string>(maxLength: 50, nullable: true),
                    MinEmployees = table.Column<int>(nullable: false, defaultValueSql: "((0))"),
                    MaxEmployees = table.Column<int>(nullable: false, defaultValueSql: "((0))"),
                    LateReason = table.Column<string>(maxLength: 200, nullable: true),
                    Modifications = table.Column<string>(maxLength: 200, nullable: true),
                    EHRCResponse = table.Column<bool>(nullable: false, defaultValueSql: "((0))")
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.Returns", x => x.ReturnId);
                    table.ForeignKey(
                        "FK_dbo.Returns_dbo.Organisations_OrganisationId",
                        x => x.OrganisationId,
                        "Organisations",
                        "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "UserOrganisations",
                table => new {
                    UserId = table.Column<long>(),
                    OrganisationId = table.Column<long>(),
                    PINHash = table.Column<string>(maxLength: 250, nullable: true),
                    PINSentDate = table.Column<DateTime>(nullable: true),
                    PINConfirmedDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttemptDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttempts = table.Column<int>(),
                    Created = table.Column<DateTime>(),
                    Modified = table.Column<DateTime>(),
                    AddressId = table.Column<long>(nullable: true),
                    MethodId = table.Column<int>(nullable: false, defaultValueSql: "((0))")
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.UserOrganisations", x => new {x.UserId, x.OrganisationId});
                    table.ForeignKey(
                        "FK_dbo.UserOrganisations_dbo.OrganisationAddresses_AddressId",
                        x => x.AddressId,
                        "OrganisationAddresses",
                        "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        "FK_dbo.UserOrganisations_dbo.Organisations_OrganisationId",
                        x => x.OrganisationId,
                        "Organisations",
                        "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_dbo.UserOrganisations_dbo.Users_UserId",
                        x => x.UserId,
                        "Users",
                        "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                "ReturnStatus",
                table => new {
                    ReturnStatusId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ReturnId = table.Column<long>(),
                    StatusId = table.Column<byte>(),
                    StatusDate = table.Column<DateTime>(),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>()
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.ReturnStatus", x => x.ReturnStatusId);
                    table.ForeignKey(
                        "FK_dbo.ReturnStatus_dbo.Users_ByUserId",
                        x => x.ByUserId,
                        "Users",
                        "UserId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_dbo.ReturnStatus_dbo.Returns_ReturnId",
                        x => x.ReturnId,
                        "Returns",
                        "ReturnId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_AddressId",
                "AddressStatus",
                "AddressId");

            migrationBuilder.CreateIndex(
                "IX_ByUserId",
                "AddressStatus",
                "ByUserId");

            migrationBuilder.CreateIndex(
                "IX_StatusDate",
                "AddressStatus",
                "StatusDate");

            migrationBuilder.CreateIndex(
                "IX_OrganisationId",
                "OrganisationAddresses",
                "OrganisationId");

            migrationBuilder.CreateIndex(
                "IX_StatusId",
                "OrganisationAddresses",
                "StatusId");

            migrationBuilder.CreateIndex(
                "IX_StatusDate",
                "OrganisationAddresses",
                "StatusDate");

            migrationBuilder.CreateIndex(
                "IX_Created",
                "OrganisationNames",
                "Created");

            migrationBuilder.CreateIndex(
                "IX_Name",
                "OrganisationNames",
                "Name");

            migrationBuilder.CreateIndex(
                "IX_OrganisationId",
                "OrganisationNames",
                "OrganisationId");

            migrationBuilder.CreateIndex(
                "IX_Created",
                "OrganisationReferences",
                "Created");

            migrationBuilder.CreateIndex(
                "IX_OrganisationId",
                "OrganisationReferences",
                "OrganisationId");

            migrationBuilder.CreateIndex(
                "IX_ReferenceName",
                "OrganisationReferences",
                "ReferenceName");

            migrationBuilder.CreateIndex(
                "IX_ReferenceValue",
                "OrganisationReferences",
                "ReferenceValue");

            migrationBuilder.CreateIndex(
                "idx_Organisations_CompanyNumber",
                "Organisations",
                "CompanyNumber",
                unique: true,
                filter: "([CompanyNumber] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                "idx_Organisations_DUNSNumber",
                "Organisations",
                "DUNSNumber",
                unique: true,
                filter: "([DUNSNumber] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                "idx_Organisations_EmployerReference",
                "Organisations",
                "EmployerReference",
                unique: true,
                filter: "([EmployerReference] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                "IX_LatestAddressId",
                "Organisations",
                "LatestAddressId");

            migrationBuilder.CreateIndex(
                "IX_LatestReturnId",
                "Organisations",
                "LatestReturnId");

            migrationBuilder.CreateIndex(
                "IX_LatestScopeId",
                "Organisations",
                "LatestScopeId");

            migrationBuilder.CreateIndex(
                "IX_OrganisationName",
                "Organisations",
                "OrganisationName");

            migrationBuilder.CreateIndex(
                "IX_SectorTypeId",
                "Organisations",
                "SectorTypeId");

            migrationBuilder.CreateIndex(
                "IX_StatusId",
                "Organisations",
                "StatusId");

            migrationBuilder.CreateIndex(
                "IX_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                "Organisations",
                new[] {"LatestRegistration_UserId", "LatestRegistration_OrganisationId"});

            migrationBuilder.CreateIndex(
                "IX_OrganisationId",
                "OrganisationScopes",
                "OrganisationId");

            migrationBuilder.CreateIndex(
                "IX_RegisterStatusId",
                "OrganisationScopes",
                "RegisterStatusId");

            migrationBuilder.CreateIndex(
                "IX_ScopeStatusId",
                "OrganisationScopes",
                "ScopeStatusId");

            migrationBuilder.CreateIndex(
                "IX_ScopeStatusDate",
                "OrganisationScopes",
                "ScopeStatusDate");

            migrationBuilder.CreateIndex(
                "IX_SnapshotDate",
                "OrganisationScopes",
                "SnapshotDate");

            migrationBuilder.CreateIndex(
                "IX_StatusId",
                "OrganisationScopes",
                "StatusId");

            migrationBuilder.CreateIndex(
                "IX_Created",
                "OrganisationSicCodes",
                "Created");

            migrationBuilder.CreateIndex(
                "IX_OrganisationId",
                "OrganisationSicCodes",
                "OrganisationId");

            migrationBuilder.CreateIndex(
                "IX_Retired",
                "OrganisationSicCodes",
                "Retired");

            migrationBuilder.CreateIndex(
                "IX_SicCodeId",
                "OrganisationSicCodes",
                "SicCodeId");

            migrationBuilder.CreateIndex(
                "IX_ByUserId",
                "OrganisationStatus",
                "ByUserId");

            migrationBuilder.CreateIndex(
                "IX_OrganisationId",
                "OrganisationStatus",
                "OrganisationId");

            migrationBuilder.CreateIndex(
                "IX_StatusDate",
                "OrganisationStatus",
                "StatusDate");

            migrationBuilder.CreateIndex(
                "IX_AccountingDate",
                "Returns",
                "AccountingDate");

            migrationBuilder.CreateIndex(
                "IX_OrganisationId",
                "Returns",
                "OrganisationId");

            migrationBuilder.CreateIndex(
                "IX_StatusId",
                "Returns",
                "StatusId");

            migrationBuilder.CreateIndex(
                "IX_ByUserId",
                "ReturnStatus",
                "ByUserId");

            migrationBuilder.CreateIndex(
                "IX_ReturnId",
                "ReturnStatus",
                "ReturnId");

            migrationBuilder.CreateIndex(
                "IX_StatusDate",
                "ReturnStatus",
                "StatusDate");

            migrationBuilder.CreateIndex(
                "IX_SicSectionId",
                "SicCodes",
                "SicSectionId");

            migrationBuilder.CreateIndex(
                "IX_AddressId",
                "UserOrganisations",
                "AddressId");

            migrationBuilder.CreateIndex(
                "IX_OrganisationId",
                "UserOrganisations",
                "OrganisationId");

            migrationBuilder.CreateIndex(
                "IX_UserId",
                "UserOrganisations",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_ContactEmailAddress",
                "Users",
                "ContactEmailAddress");

            migrationBuilder.CreateIndex(
                "IX_ContactPhoneNumber",
                "Users",
                "ContactPhoneNumber");

            migrationBuilder.CreateIndex(
                "IX_EmailAddress",
                "Users",
                "EmailAddress");

            migrationBuilder.CreateIndex(
                "IX_StatusId",
                "Users",
                "StatusId");

            migrationBuilder.CreateIndex(
                "IX_UserId",
                "UserSettings",
                "UserId");

            migrationBuilder.CreateIndex(
                "IX_ByUserId",
                "UserStatus",
                "ByUserId");

            migrationBuilder.CreateIndex(
                "IX_StatusDate",
                "UserStatus",
                "StatusDate");

            migrationBuilder.CreateIndex(
                "IX_UserId",
                "UserStatus",
                "UserId");

            migrationBuilder.AddForeignKey(
                "FK_dbo.AddressStatus_dbo.OrganisationAddresses_AddressId",
                "AddressStatus",
                "AddressId",
                "OrganisationAddresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                "FK_dbo.Organisations_dbo.OrganisationAddresses_LatestAddressId",
                "Organisations",
                "LatestAddressId",
                "OrganisationAddresses",
                principalColumn: "AddressId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                "FK_dbo.Organisations_dbo.Returns_LatestReturnId",
                "Organisations",
                "LatestReturnId",
                "Returns",
                principalColumn: "ReturnId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                "FK_dbo.Organisations_dbo.OrganisationScopes_LatestScopeId",
                "Organisations",
                "LatestScopeId",
                "OrganisationScopes",
                principalColumn: "OrganisationScopeId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                "FK_dbo.Organisations_dbo.UserOrganisations_LatestRegistration_UserId_LatestRegistration_OrganisationId",
                "Organisations",
                new[] {"LatestRegistration_UserId", "LatestRegistration_OrganisationId"},
                "UserOrganisations",
                principalColumns: new[] {"UserId", "OrganisationId"},
                onDelete: ReferentialAction.Restrict);

            //migrationBuilder.Sql("DROP INDEX [idx_Organisations_EmployerReference]ON [dbo].[Organisations];");
            //migrationBuilder.Sql("CREATE UNIQUE NONCLUSTERED INDEX [idx_Organisations_EmployerReference]ON [dbo].[Organisations]([EmployerReference] ASC) WHERE ([EmployerReference] IS NOT NULL);");
            string ReportsReaderPassword = Config.GetAppSetting("ReportsReaderPassword", "Password");
            migrationBuilder.Sql(
                $"If not Exists (select loginname from master.dbo.syslogins where name = 'ReportsReaderLogin') CREATE LOGIN [ReportsReaderLogin] WITH PASSWORD = '{ReportsReaderPassword}';");

            #region 201811261437002_Add user ReportReader.cs

            migrationBuilder.Sql("CREATE USER [ReportsReaderDv] FOR LOGIN [ReportsReaderLogin];");

            #endregion

            migrationBuilder.Sql("GRANT SELECT ON OBJECT::[dbo].[OrganisationNames] TO [ReportsReaderDv] AS [dbo];");

            #region 201811261453436_Add view OrganisationAddressInfoView.cs

            migrationBuilder.Sql(
                "CREATE VIEW [dbo].[OrganisationAddressInfoView] AS SELECT organisationAdd.OrganisationId ,CASE organisationAdd.[StatusId] WHEN 0 THEN 'Unknown' WHEN 1 THEN 'New' WHEN 2 THEN 'Suspended' WHEN 3 THEN 'Active' WHEN 5 THEN 'Pending' WHEN 6 THEN 'Retired' ELSE 'Error' END + ' (' + CAST(organisationAdd.StatusId AS VARCHAR(2)) + ')' AS AddressStatus ,REPLACE(COALESCE(RTRIM(LTRIM(organisationAdd.Address1)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.Address2)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.Address3)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.TownCity)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.County)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.PoBox)), '') + COALESCE(', ' + RTRIM(LTRIM(organisationAdd.PostCode)), '') + COALESCE(' (' + RTRIM(LTRIM(organisationAdd.Country)) + ')', ''), ' ,', ',') AS FullAddress ,organisationAdd.StatusDetails AS AddressStatusDetails ,organisationAdd.StatusDate AS AddressStatusDate ,organisationAdd.Source AS AddressSource ,organisationAdd.Created AS AddressCreated ,organisationAdd.Modified AS AddressModified FROM ( SELECT AddressId ,CreatedByUserId ,CASE WHEN (LEN([Address1]) > 0) THEN [Address1] ELSE NULL END AS Address1 ,CASE WHEN (LEN([Address2]) > 0) THEN [Address2] ELSE NULL END AS Address2 ,CASE WHEN (LEN([Address3]) > 0) THEN [Address3] ELSE NULL END AS Address3 ,CASE WHEN (LEN([TownCity]) > 0) THEN [TownCity] ELSE NULL END AS TownCity ,CASE WHEN (LEN([County]) > 0) THEN [County] ELSE NULL END AS County ,CASE WHEN (LEN([Country]) > 0) THEN [Country] ELSE NULL END AS Country ,CASE WHEN (LEN([PoBox]) > 0) THEN [PoBox] ELSE NULL END AS PoBox ,CASE WHEN (LEN([PostCode]) > 0) THEN [PostCode] ELSE NULL END AS PostCode ,StatusId ,StatusDate ,StatusDetails ,Created ,Modified ,OrganisationId ,Source FROM dbo.OrganisationAddresses ) AS organisationAdd INNER JOIN dbo.Organisations AS orgs ON orgs.OrganisationId = organisationAdd.OrganisationId");

            migrationBuilder.Sql("GRANT SELECT ON OBJECT::[dbo].[OrganisationAddressInfoView] TO [ReportsReaderDv] AS [dbo];");

            #endregion

            #region 201811261523023_Add view OrganisationInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationInfoView] "
                + " AS "
                + " SELECT orgs.OrganisationId "
                + " 	,orgs.EmployerReference "
                + " 	,orgs.DUNSNumber "
                + " 	,orgs.CompanyNumber "
                + " 	,orgs.OrganisationName "
                + " 	,CASE orgs.[SectorTypeId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'Private' "
                + " 		WHEN 2 "
                + " 			THEN 'Public' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(orgs.SectorTypeId AS VARCHAR(2)) + ')' AS SectorType "
                + " 	,CASE orgs.StatusId "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'New' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Active' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		WHEN 5 "
                + " 			THEN 'Pending' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(orgs.StatusId AS VARCHAR(2)) + ')' AS OrganisationStatus "
                + " 	,orgs.[SecurityCode] "
                + " 	,orgs.[SecurityCodeExpiryDateTime] "
                + " 	,orgs.[SecurityCodeCreatedDateTime] "
                + " FROM dbo.Organisations AS orgs ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261527380_Add view OrganisationRegistrationInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationRegistrationInfoView] "
                + " AS "
                + " SELECT uo.OrganisationId "
                + " 	,u.Firstname + ' ' + u.Lastname + ' [' + u.JobTitle + ']' AS UserInfo "
                + " 	,u.ContactFirstName + ' ' + u.ContactLastName + ' [' + u.ContactJobTitle + ']' AS ContactInfo "
                + " 	,CASE uo.[MethodId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'PinInPost' "
                + " 		WHEN 2 "
                + " 			THEN 'EmailDomain' "
                + " 		WHEN 3 "
                + " 			THEN 'Manual' "
                + " 		WHEN 4 "
                + " 			THEN 'Fasttrack' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(uo.MethodId AS VARCHAR(2)) + ')' AS RegistrationMethod "
                + " 	,uo.PINSentDate "
                + " 	,uo.PINConfirmedDate "
                + " 	,uo.ConfirmAttemptDate "
                + " 	,uo.ConfirmAttempts "
                + " FROM dbo.UserOrganisations AS uo "
                + " INNER JOIN dbo.Users AS u ON u.UserId = uo.UserId");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationRegistrationInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261557329_Add view OrganisationScopeInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationScopeInfoView] "
                + " AS "
                + " SELECT OrganisationId "
                + " 	,CASE scopes.scopestatusid "
                + " 		WHEN 1 "
                + " 			THEN 'In Scope' "
                + " 		WHEN 2 "
                + " 			THEN 'Out of Scope' "
                + " 		WHEN 3 "
                + " 			THEN 'Presumed in Scope' "
                + " 		WHEN 4 "
                + " 			THEN 'Presumed out of Scope' "
                + " 		ELSE 'Unknown' "
                + " 		END + ' (' + CAST(ScopeStatusId AS VARCHAR(2)) + ')' AS ScopeStatus "
                + " 	,ScopeStatusDate "
                + " 	,CASE scopes.RegisterStatusid "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'RegisterSkipped' "
                + " 		WHEN 2 "
                + " 			THEN 'RegisterPending' "
                + " 		WHEN 3 "
                + " 			THEN 'RegisterComplete' "
                + " 		WHEN 4 "
                + " 			THEN 'RegisterCancelled' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(RegisterStatusId AS VARCHAR(2)) + ')' AS RegisterStatus "
                + " 	,RegisterStatusDate "
                + " 	,YEAR(SnapshotDate) AS snapshotYear "
                + " FROM dbo.OrganisationScopes AS scopes ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationScopeInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261601550_Add view OrganisationSearchInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationSearchInfoView] "
                + " AS "
                + " SELECT orgs.OrganisationId "
                + " 	,orgs.EmployerReference "
                + " 	,orgs.DUNSNumber "
                + " 	,orgs.CompanyNumber "
                + " 	,orgs.OrganisationName "
                + " 	,CASE orgs.[SectorTypeId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'Private' "
                + " 		WHEN 2 "
                + " 			THEN 'Public' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(orgs.SectorTypeId AS VARCHAR(2)) + ')' AS SectorType "
                + " 	,CASE orgs.StatusId "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'New' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Active' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		WHEN 5 "
                + " 			THEN 'Pending' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(orgs.StatusId AS VARCHAR(2)) + ')' AS OrganisationStatus "
                + " FROM dbo.Organisations AS orgs ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationSearchInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261605329_Add view OrganisationSubmissionInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationSubmissionInfoView] "
                + " AS "
                + " SELECT ret.OrganisationId "
                + " 	,CONVERT(DATE, ret.AccountingDate) AS LatestReturnAccountingDate "
                + " 	,DATEADD(second, - 1, DATEADD(year, 1, ret.AccountingDate)) AS ReportingDeadline "
                + " 	,CASE ret.[StatusId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'Draft' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Submitted' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		WHEN 5 "
                + " 			THEN 'Deleted' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(ret.StatusId AS VARCHAR(2)) + ')' AS latestReturnStatus "
                + " 	,ret.StatusDate AS latestReturnStatusDate "
                + " 	,ret.StatusDetails AS LatestReturnStatusDetails "
                + " 	,CASE  "
                + " 		WHEN ([Modified] > dateadd(second, - 1, dateadd(year, 1, accountingdate))) "
                + " 			THEN 'true' "
                + " 		ELSE 'false' "
                + " 		END AS ReportedLate "
                + " 	,ret.LateReason AS LatestReturnLateReason "
                + " 	,CASE retstat.[StatusId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'Draft' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Submitted' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		WHEN 5 "
                + " 			THEN 'Deleted' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(retstat.StatusId AS VARCHAR(2)) + ')' AS StatusId "
                + " 	,retstat.StatusDate "
                + " 	,retstat.StatusDetails "
                + " 	,ret.Modifications AS ReturnModifiedFields "
                + " 	,CASE ret.[EHRCResponse] "
                + " 		WHEN 1 "
                + " 			THEN 'true' "
                + " 		ELSE 'false' "
                + " 		END AS EHRCResponse "
                + " 	,ret.FirstName + ' ' + ret.LastName + ' [' + ret.JobTitle + ']' AS SubmittedBy "
                + " 	,CASE  "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 0 "
                + " 				AND [MaxEmployees] = 0 "
                + " 				) "
                + " 			THEN 'Not provided' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 0 "
                + " 				AND [MaxEmployees] = 249 "
                + " 				) "
                + " 			THEN 'Employees 0 to 249' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 250 "
                + " 				AND [MaxEmployees] = 499 "
                + " 				) "
                + " 			THEN 'Employees 250 to 499' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 500 "
                + " 				AND [MaxEmployees] = 999 "
                + " 				) "
                + " 			THEN 'Employees 500 to 999' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 1000 "
                + " 				AND [MaxEmployees] = 4999 "
                + " 				) "
                + " 			THEN 'Employees 1,000 to 4,999' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 5000 "
                + " 				AND [MaxEmployees] = 19999 "
                + " 				) "
                + " 			THEN 'Employees 5,000 to 19,999' "
                + " 		WHEN ( "
                + " 				[MinEmployees] = 20000 "
                + " 				AND [MaxEmployees] = 2147483647 "
                + " 				) "
                + " 			THEN 'Employees 20,000 or more' "
                + " 		ELSE 'Error' "
                + " 		END AS OrganisationSize "
                + " 	,ret.DiffMeanHourlyPayPercent "
                + " 	,ret.DiffMedianHourlyPercent "
                + " 	,ret.DiffMeanBonusPercent "
                + " 	,ret.DiffMedianBonusPercent "
                + " 	,ret.MaleMedianBonusPayPercent "
                + " 	,ret.FemaleMedianBonusPayPercent "
                + " 	,ret.MaleLowerPayBand "
                + " 	,ret.FemaleLowerPayBand "
                + " 	,ret.MaleMiddlePayBand "
                + " 	,ret.FemaleMiddlePayBand "
                + " 	,ret.MaleUpperPayBand "
                + " 	,ret.FemaleUpperPayBand "
                + " 	,ret.MaleUpperQuartilePayBand "
                + " 	,ret.FemaleUpperQuartilePayBand "
                + " FROM dbo.[Returns] AS ret "
                + " LEFT OUTER JOIN dbo.ReturnStatus AS retstat ON retstat.ReturnId = ret.ReturnId");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationSubmissionInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261637321_Add view OrganisationSicCodeInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[OrganisationSicCodeInfoView] "
                + " AS "
                + " SELECT osc.OrganisationId "
                + " 	,sc.[SicCodeId] "
                + " 	,rank() OVER ( "
                + " 		PARTITION BY osc.OrganisationId ORDER BY sc.[SicCodeId], osc.[Source] ASC "
                + " 		) AS SicCodeRankWithinOrganisation "
                + " 	, CASE WHEN (CHARINDEX( '@', osc.[Source]) > 0) "
                + " 			THEN 'User' "
                + " 		ELSE	 "
                + " 			osc.[Source] "
                + " 		END AS [Source] "
                + " 	,sc.[Description] AS CodeDescription "
                + " 	,ss.SicSectionId "
                + " 	,ss.[Description] AS SectionDescription "
                + " FROM [dbo].[SicSections] AS ss "
                + " RIGHT JOIN [dbo].[SicCodes] AS sc ON sc.SicSectionId = ss.SicSectionId "
                + " RIGHT JOIN ( "
                + " 	SELECT sicCodeId "
                + " 		,organisationid "
                + " 		,[source] "
                + " 	FROM dbo.[OrganisationSicCodes] "
                + " 	GROUP BY sicCodeId "
                + " 		,organisationid "
                + " 		,[source] "
                + " 	) AS osc ON osc.SicCodeId = sc.SicCodeId ");

            migrationBuilder.Sql(" GRANT SELECT ON OBJECT::dbo.[OrganisationSicCodeInfoView] TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261645007_Add view UserInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[UserInfoView] "
                + " AS "
                + " SELECT UserId "
                + " 	,CASE [StatusId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'New' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Active' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(StatusId AS VARCHAR(2)) + ')' AS StatusId "
                + " , StatusDate "
                + " , StatusDetails "
                + " , Firstname "
                + " , Lastname "
                + " , JobTitle "
                + " , ContactFirstName "
                + " , ContactLastName "
                + " , ContactJobTitle "
                + " , ContactOrganisation "
                + " , ContactPhoneNumber "
                + " , EmailVerifySendDate "
                + " , EmailVerifiedDate "
                + " , LoginAttempts "
                + " , LoginDate "
                + " , ResetSendDate "
                + " , ResetAttempts "
                + " , VerifyAttemptDate "
                + " , VerifyAttempts "
                + " FROM dbo.Users ");

            migrationBuilder.Sql("  GRANT SELECT " + " 	ON OBJECT::dbo.[UserInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201811261649336_Add view UserStatusInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[UserStatusInfoView] "
                + " AS "
                + " SELECT ust.ByUserId AS UserId "
                + " 	,modifyingUser.Firstname + ' ' + modifyingUser.Lastname + ' [' + modifyingUser.JobTitle + ']' AS UserName "
                + " 	,CASE usr.[StatusId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'New' "
                + " 		WHEN 2 "
                + " 			THEN 'Suspended' "
                + " 		WHEN 3 "
                + " 			THEN 'Active' "
                + " 		WHEN 4 "
                + " 			THEN 'Retired' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(usr.StatusId AS VARCHAR(2)) + ')' AS StatusId "
                + " 	,ust.StatusDate "
                + " 	,ust.StatusDetails "
                + " 	,usr.Firstname + ' ' + usr.Lastname + ' [' + usr.JobTitle + ']' AS StatusChangedBy "
                + " FROM dbo.UserStatus AS ust "
                + " INNER JOIN dbo.Users AS usr ON usr.UserId = ust.UserId "
                + " INNER JOIN dbo.Users AS modifyingUser ON modifyingUser.UserId = ust.ByUserId ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[UserStatusInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201812131233344_Add function OrganisationStatusIdToString.cs

            migrationBuilder.Sql(
                " CREATE FUNCTION [dbo].[OrganisationStatusIdToString]"
                + " ("
                + " 	@organisationStatusId tinyint"
                + " )"
                + " RETURNS varchar(20)"
                + " AS"
                + " BEGIN"
                + " 	DECLARE @Result varchar(20)"
                + " "
                + " 	SELECT @Result = CASE @organisationStatusId "
                + " 						WHEN 0 THEN 'Unknown' "
                + " 						WHEN 1 THEN 'New' "
                + " 						WHEN 2 THEN 'Suspended' "
                + " 						WHEN 3 THEN 'Active' "
                + " 						WHEN 4 THEN 'Retired' "
                + " 						WHEN 5 THEN 'Pending' "
                + " 						WHEN 6 THEN 'Deleted' "
                + " 						ELSE 'Error' "
                + " 					END + ' (' + CAST(@organisationStatusId AS VARCHAR(3)) + ')'"
                + " "
                + " 	RETURN @Result		"
                + " END ");

            migrationBuilder.Sql(" GRANT EXECUTE " + " 	ON OBJECT::[dbo].[OrganisationStatusIdToString] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201812131245269_Add function OrganisationSectorTypeIdToString.cs

            migrationBuilder.Sql(
                " CREATE FUNCTION [dbo].[OrganisationSectorTypeIdToString]"
                + " ("
                + " 	@organisationSectorTypeId tinyint"
                + " )"
                + " RETURNS varchar(20)"
                + " AS"
                + " BEGIN"
                + " 	DECLARE @Result varchar(20)"
                + " "
                + " 	SELECT @Result = CASE @organisationSectorTypeId"
                + " 						WHEN 0 THEN 'Unknown' "
                + " 						WHEN 1 THEN 'Private' "
                + " 						WHEN 2 THEN 'Public' "
                + " 						ELSE 'Error' "
                + " 					END + ' (' + CAST(@organisationSectorTypeId AS VARCHAR(3)) + ')'"
                + " "
                + " 	RETURN @Result"
                + " "
                + " END ");

            migrationBuilder.Sql(" GRANT EXECUTE " + " 	ON OBJECT::[dbo].[OrganisationSectorTypeIdToString] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201812131250589_Modify view OrganisationInfoView.cs

            migrationBuilder.Sql(
                " ALTER VIEW [dbo].[OrganisationInfoView] "
                + " AS "
                + " SELECT orgs.OrganisationId "
                + " 	, orgs.EmployerReference "
                + " 	, orgs.DUNSNumber "
                + " 	, orgs.CompanyNumber "
                + " 	, orgs.OrganisationName "
                + " 	, dbo.OrganisationSectorTypeIdToString(orgs.[SectorTypeId]) AS SectorType "
                + " 	, dbo.OrganisationStatusIdToString(orgs.StatusId) AS OrganisationStatus "
                + " 	, orgs.[SecurityCode] "
                + " 	, orgs.[SecurityCodeExpiryDateTime] "
                + " 	, orgs.[SecurityCodeCreatedDateTime] "
                + " FROM dbo.Organisations AS orgs ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::[dbo].[OrganisationInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region 201812131257437_Add view UserLinkedOrganisationsView.cs

            migrationBuilder.Sql(
                " CREATE VIEW [dbo].[UserLinkedOrganisationsView] "
                + " AS "
                + " 	SELECT usrOrgs.UserId "
                + " 		,orgs.OrganisationId "
                + " 		,orgs.DUNSNumber "
                + " 		,orgs.EmployerReference "
                + " 		,orgs.CompanyNumber "
                + " 		,orgs.OrganisationName "
                + " 		,dbo.OrganisationSectorTypeIdToString(orgs.SectorTypeId) AS SectorTypeId "
                + " 		,dbo.OrganisationStatusIdToString(orgs.StatusId) AS StatusId "
                + " 	FROM dbo.UserOrganisations AS usrOrgs "
                + " 	INNER JOIN dbo.Organisations AS orgs ON orgs.OrganisationId = usrOrgs.OrganisationId ");

            migrationBuilder.Sql(" GRANT SELECT " + " 	ON OBJECT::dbo.[UserLinkedOrganisationsView] " + " 	TO [ReportsReaderDv]; ");

            #endregion

            #region Synchronise GpgDatabaseContextModelSnapshot and current estate of the database

            // These instructions were run directly onto the database and were needed to be included here to synchronise the context and the database

            migrationBuilder.AlterColumn<decimal>(
                "DiffMedianBonusPercent",
                "Returns",
                nullable: true,
                oldClrType: typeof(decimal));

            migrationBuilder.AlterColumn<decimal>(
                "DiffMeanBonusPercent",
                "Returns",
                nullable: true,
                oldClrType: typeof(decimal));

            migrationBuilder.AddColumn<long>(
                "LatestPublicSectorTypeId",
                "Organisations",
                nullable: true);

            migrationBuilder.CreateTable(
                "PublicSectorTypes",
                table => new {
                    PublicSectorTypeId = table.Column<int>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(maxLength: 250),
                    Created = table.Column<DateTime>()
                },
                constraints: table => { table.PrimaryKey("PK_dbo.PublicSectorTypes", x => x.PublicSectorTypeId); });

            migrationBuilder.CreateTable(
                "OrganisationPublicSectorTypes",
                table => new {
                    OrganisationPublicSectorTypeId = table.Column<long>()
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    PublicSectorTypeId = table.Column<int>(),
                    OrganisationId = table.Column<long>(),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(),
                    Retired = table.Column<DateTime>(nullable: true)
                },
                constraints: table => {
                    table.PrimaryKey("PK_dbo.OrganisationPublicSectorTypes", x => x.OrganisationPublicSectorTypeId);
                    table.ForeignKey(
                        "FK_dbo.OrganisationPublicSectorTypes_dbo.Organisations_OrganisationId",
                        x => x.OrganisationId,
                        "Organisations",
                        "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        "FK_dbo.OrganisationPublicSectorTypes_dbo.PublicSectorTypes_PublicSectorTypeId",
                        x => x.PublicSectorTypeId,
                        "PublicSectorTypes",
                        "PublicSectorTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_LatestPublicSectorTypeId",
                "Organisations",
                "LatestPublicSectorTypeId");

            migrationBuilder.CreateIndex(
                "IX_Created",
                "OrganisationPublicSectorTypes",
                "Created");

            migrationBuilder.CreateIndex(
                "IX_OrganisationId",
                "OrganisationPublicSectorTypes",
                "OrganisationId");

            migrationBuilder.CreateIndex(
                "IX_PublicSectorTypeId",
                "OrganisationPublicSectorTypes",
                "PublicSectorTypeId");

            migrationBuilder.CreateIndex(
                "IX_Retired",
                "OrganisationPublicSectorTypes",
                "Retired");

            migrationBuilder.AddForeignKey(
                "FK_dbo.Organisations_dbo.OrganisationPublicSectorTypes_LatestPublicSectorTypeId",
                "Organisations",
                "LatestPublicSectorTypeId",
                "OrganisationPublicSectorTypes",
                principalColumn: "OrganisationPublicSectorTypeId",
                onDelete: ReferentialAction.Restrict);

            #endregion

            #region 201901161519381_Add view OrganisationScopeAndReturnInfoView.cs

            migrationBuilder.Sql(
                " CREATE VIEW dbo.OrganisationScopeAndReturnInfoView "
                + " AS "
                + " SELECT orgs.OrganisationId "
                + " 	,orgs.OrganisationName "
                + " 	,orgs.EmployerReference "
                + " 	,orgs.CompanyNumber "
                + " 	,dbo.OrganisationStatusIdToString(orgs.StatusId) AS OrganisationStatus "
                + " 	,dbo.OrganisationSectorTypeIdToString(orgs.SectorTypeId) AS SectorType "
                + " 	,CASE OrgScopes.[ScopeStatusId] "
                + " 		WHEN 0 "
                + " 			THEN 'Unknown' "
                + " 		WHEN 1 "
                + " 			THEN 'In scope' "
                + " 		WHEN 2 "
                + " 			THEN 'Out of scope' "
                + " 		WHEN 3 "
                + " 			THEN 'Presumed in scope' "
                + " 		WHEN 4 "
                + " 			THEN 'Presumed out of scope' "
                + " 		ELSE 'Error' "
                + " 		END + ' (' + CAST(OrgScopes.ScopeStatusId AS VARCHAR(3)) + ')' AS ScopeStatus "
                + " 	,YEAR(OrgScopes.SnapshotDate) AS SnapshotDate "
                + " 	,OrgSicCodeView.SectionDescription AS SicCodeSectionDescription "
                + " 	,retrns.ReturnId "
                + " 	,CASE  "
                + " 		WHEN retrns.MinEmployees = 0 "
                + " 			AND retrns.MaxEmployees = 0 "
                + " 			THEN 'Not Provided' "
                + " 		WHEN retrns.MinEmployees = 0 "
                + " 			AND retrns.MaxEmployees = 249 "
                + " 			THEN 'Less than 250' "
                + " 		WHEN ( "
                + " 				retrns.MinEmployees = 250 "
                + " 				AND retrns.MaxEmployees = 499 "
                + " 				) "
                + " 			OR ( "
                + " 				retrns.MinEmployees = 500 "
                + " 				AND retrns.MaxEmployees = 999 ) OR"
                + Environment.NewLine
                + "                         (retrns.MinEmployees = 1000 "
                + " 				AND retrns.MaxEmployees = 4999 "
                + " 				) "
                + " 			THEN CAST(retrns.MinEmployees AS VARCHAR) + ' to ' + CAST(retrns.MaxEmployees AS VARCHAR) "
                + " 		WHEN retrns.MinEmployees = 5000 "
                + " 			AND retrns.MaxEmployees = 19999 "
                + " 			THEN CAST(retrns.MinEmployees AS VARCHAR) + ' to 19,999' "
                + " 		WHEN retrns.MinEmployees = 20000 "
                + " 			AND retrns.MaxEmployees = 2147483647 "
                + " 			THEN '20,000 or more' "
                + " 		ELSE 'Error min(' + CAST(retrns.MinEmployees AS VARCHAR) + '), max (' + CAST(retrns.MaxEmployees AS VARCHAR) + ')' "
                + " 		END AS OrganisationSize "
                + " 	,OrgPubSectType.PublicSectorDescription "
                + " FROM dbo.Organisations AS orgs "
                + " INNER JOIN dbo.OrganisationScopes AS OrgScopes ON OrgScopes.OrganisationId = orgs.OrganisationId "
                + " 	AND OrgScopes.StatusId = 3 "
                + " LEFT OUTER JOIN dbo.OrganisationSicCodeInfoView AS OrgSicCodeView ON OrgSicCodeView.OrganisationId = orgs.OrganisationId "
                + " 	AND OrgSicCodeView.SicCodeRankWithinOrganisation = 1 "
                + " LEFT OUTER JOIN dbo.[Returns] AS retrns ON retrns.OrganisationId = OrgScopes.OrganisationId "
                + " 	AND retrns.AccountingDate = OrgScopes.SnapshotDate "
                + " 	AND retrns.StatusId = 3 "
                + " LEFT OUTER JOIN ( "
                + " 	SELECT opst.OrganisationId "
                + " 		,pst.Description AS PublicSectorDescription "
                + " 	FROM dbo.OrganisationPublicSectorTypes AS opst "
                + " 	INNER JOIN dbo.PublicSectorTypes AS pst ON pst.PublicSectorTypeId = opst.PublicSectorTypeId "
                + " 		AND opst.Retired IS NULL "
                + " 	) AS OrgPubSectType ON OrgPubSectType.OrganisationId = orgs.OrganisationId ");

            migrationBuilder.Sql("  GRANT SELECT " + " 	ON OBJECT::dbo.[OrganisationScopeAndReturnInfoView] " + " 	TO [ReportsReaderDv]; ");

            #endregion
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            #region Revert synchronise GpgDatabaseContextModelSnapshot

            migrationBuilder.DropForeignKey(
                "FK_dbo.Organisations_dbo.OrganisationPublicSectorTypes_LatestPublicSectorTypeId",
                "Organisations");

            migrationBuilder.DropTable("OrganisationPublicSectorTypes");

            migrationBuilder.DropTable("PublicSectorTypes");

            migrationBuilder.DropIndex(
                "IX_Organisations_LatestPublicSectorTypeId",
                "Organisations");

            migrationBuilder.DropColumn(
                "LatestPublicSectorTypeId",
                "Organisations");

            migrationBuilder.AlterColumn<decimal>(
                "DiffMedianBonusPercent",
                "Returns",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                "DiffMeanBonusPercent",
                "Returns",
                nullable: false,
                oldClrType: typeof(decimal),
                oldNullable: true);

            #endregion

            migrationBuilder.DropForeignKey(
                "FK_dbo.Organisations_dbo.OrganisationAddresses_LatestAddressId",
                "Organisations");

            migrationBuilder.DropForeignKey(
                "FK_dbo.UserOrganisations_dbo.OrganisationAddresses_AddressId",
                "UserOrganisations");

            migrationBuilder.DropForeignKey(
                "FK_dbo.UserOrganisations_dbo.Users_UserId",
                "UserOrganisations");

            migrationBuilder.DropForeignKey(
                "FK_dbo.OrganisationScopes_dbo.Organisations_OrganisationId",
                "OrganisationScopes");

            migrationBuilder.DropForeignKey(
                "FK_dbo.Returns_dbo.Organisations_OrganisationId",
                "Returns");

            migrationBuilder.DropForeignKey(
                "FK_dbo.UserOrganisations_dbo.Organisations_OrganisationId",
                "UserOrganisations");

            migrationBuilder.DropTable("AddressStatus");

            migrationBuilder.DropTable("OrganisationNames");

            migrationBuilder.DropTable("OrganisationReferences");

            migrationBuilder.DropTable("OrganisationSicCodes");

            migrationBuilder.DropTable("OrganisationStatus");

            migrationBuilder.DropTable("ReturnStatus");

            migrationBuilder.DropTable("UserSettings");

            migrationBuilder.DropTable("UserStatus");

            migrationBuilder.DropTable("SicCodes");

            migrationBuilder.DropTable("SicSections");

            migrationBuilder.DropTable("OrganisationAddresses");

            migrationBuilder.DropTable("Users");

            migrationBuilder.DropTable("Organisations");

            migrationBuilder.DropTable("Returns");

            migrationBuilder.DropTable("OrganisationScopes");

            migrationBuilder.DropTable("UserOrganisations");
        }

    }
}
