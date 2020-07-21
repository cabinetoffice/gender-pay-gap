using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GenderPayGap.Database.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FriendlyName = table.Column<string>(nullable: true),
                    Xml = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DraftReturns",
                columns: table => new
                {
                    DraftReturnId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganisationId = table.Column<long>(nullable: false),
                    SnapshotYear = table.Column<int>(nullable: false),
                    DiffMeanHourlyPayPercent = table.Column<decimal>(nullable: true),
                    DiffMedianHourlyPercent = table.Column<decimal>(nullable: true),
                    DiffMeanBonusPercent = table.Column<decimal>(nullable: true),
                    DiffMedianBonusPercent = table.Column<decimal>(nullable: true),
                    MaleMedianBonusPayPercent = table.Column<decimal>(nullable: true),
                    FemaleMedianBonusPayPercent = table.Column<decimal>(nullable: true),
                    MaleLowerPayBand = table.Column<decimal>(nullable: true),
                    FemaleLowerPayBand = table.Column<decimal>(nullable: true),
                    MaleMiddlePayBand = table.Column<decimal>(nullable: true),
                    FemaleMiddlePayBand = table.Column<decimal>(nullable: true),
                    MaleUpperPayBand = table.Column<decimal>(nullable: true),
                    FemaleUpperPayBand = table.Column<decimal>(nullable: true),
                    MaleUpperQuartilePayBand = table.Column<decimal>(nullable: true),
                    FemaleUpperQuartilePayBand = table.Column<decimal>(nullable: true),
                    ReturnId = table.Column<long>(nullable: true),
                    EncryptedOrganisationId = table.Column<string>(nullable: true),
                    SectorType = table.Column<int>(nullable: true),
                    AccountingDate = table.Column<DateTime>(nullable: true),
                    Modified = table.Column<DateTime>(nullable: false),
                    JobTitle = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: true),
                    LastName = table.Column<string>(nullable: true),
                    CompanyLinkToGPGInfo = table.Column<string>(nullable: true),
                    ReturnUrl = table.Column<string>(nullable: true),
                    OriginatingAction = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    LatestAddress = table.Column<string>(nullable: true),
                    OrganisationName = table.Column<string>(nullable: true),
                    LatestOrganisationName = table.Column<string>(nullable: true),
                    OrganisationSize = table.Column<int>(nullable: true),
                    Sector = table.Column<string>(nullable: true),
                    LatestSector = table.Column<string>(nullable: true),
                    IsDifferentFromDatabase = table.Column<bool>(nullable: true),
                    IsVoluntarySubmission = table.Column<bool>(nullable: true),
                    IsLateSubmission = table.Column<bool>(nullable: true),
                    ShouldProvideLateReason = table.Column<bool>(nullable: true),
                    IsInScopeForThisReportYear = table.Column<bool>(nullable: true),
                    LateReason = table.Column<string>(nullable: true),
                    EHRCResponse = table.Column<string>(nullable: true),
                    LastWrittenDateTime = table.Column<DateTime>(nullable: true),
                    LastWrittenByUserId = table.Column<long>(nullable: true),
                    HasDraftBeenModifiedDuringThisSession = table.Column<bool>(nullable: true),
                    ReportingStartDate = table.Column<DateTime>(nullable: true),
                    ReportModifiedDate = table.Column<DateTime>(nullable: true),
                    ReportingRequirement = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftReturns", x => x.DraftReturnId);
                });

            migrationBuilder.CreateTable(
                name: "Feedback",
                columns: table => new
                {
                    FeedbackId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Difficulty = table.Column<int>(nullable: true),
                    Details = table.Column<string>(maxLength: 2000, nullable: true),
                    EmailAddress = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    NewsArticle = table.Column<bool>(nullable: true),
                    SocialMedia = table.Column<bool>(nullable: true),
                    CompanyIntranet = table.Column<bool>(nullable: true),
                    EmployerUnion = table.Column<bool>(nullable: true),
                    InternetSearch = table.Column<bool>(nullable: true),
                    Charity = table.Column<bool>(nullable: true),
                    LobbyGroup = table.Column<bool>(nullable: true),
                    Report = table.Column<bool>(nullable: true),
                    OtherSource = table.Column<bool>(nullable: true),
                    OtherSourceText = table.Column<string>(maxLength: 2000, nullable: true),
                    FindOutAboutGpg = table.Column<bool>(nullable: true),
                    ReportOrganisationGpgData = table.Column<bool>(nullable: true),
                    CloseOrganisationGpg = table.Column<bool>(nullable: true),
                    ViewSpecificOrganisationGpg = table.Column<bool>(nullable: true),
                    ActionsToCloseGpg = table.Column<bool>(nullable: true),
                    OtherReason = table.Column<bool>(nullable: true),
                    OtherReasonText = table.Column<string>(maxLength: 2000, nullable: true),
                    EmployeeInterestedInOrganisationData = table.Column<bool>(nullable: true),
                    ManagerInvolvedInGpgReport = table.Column<bool>(nullable: true),
                    ResponsibleForReportingGpg = table.Column<bool>(nullable: true),
                    PersonInterestedInGeneralGpg = table.Column<bool>(nullable: true),
                    PersonInterestedInSpecificOrganisationGpg = table.Column<bool>(nullable: true),
                    OtherPerson = table.Column<bool>(nullable: true),
                    OtherPersonText = table.Column<string>(maxLength: 2000, nullable: true),
                    FeedbackStatus = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedback", x => x.FeedbackId);
                });

            migrationBuilder.CreateTable(
                name: "PublicSectorTypes",
                columns: table => new
                {
                    PublicSectorTypeId = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.PublicSectorTypes", x => x.PublicSectorTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SicSections",
                columns: table => new
                {
                    SicSectionId = table.Column<string>(maxLength: 1, nullable: false),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.SicSections", x => x.SicSectionId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobTitle = table.Column<string>(maxLength: 50, nullable: false),
                    Firstname = table.Column<string>(maxLength: 50, nullable: false),
                    Lastname = table.Column<string>(maxLength: 50, nullable: false),
                    EmailAddress = table.Column<string>(maxLength: 255, nullable: false),
                    ContactJobTitle = table.Column<string>(maxLength: 50, nullable: true),
                    ContactFirstName = table.Column<string>(maxLength: 50, nullable: true),
                    ContactLastName = table.Column<string>(maxLength: 50, nullable: true),
                    ContactOrganisation = table.Column<string>(maxLength: 100, nullable: true),
                    ContactEmailAddress = table.Column<string>(maxLength: 255, nullable: true),
                    ContactPhoneNumber = table.Column<string>(maxLength: 20, nullable: true),
                    PasswordHash = table.Column<string>(maxLength: 250, nullable: false),
                    Salt = table.Column<string>(nullable: true),
                    HashingAlgorithm = table.Column<int>(nullable: false),
                    EmailVerifyHash = table.Column<string>(maxLength: 250, nullable: true),
                    EmailVerifySendDate = table.Column<DateTime>(nullable: true),
                    EmailVerifiedDate = table.Column<DateTime>(nullable: true),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    LoginAttempts = table.Column<int>(nullable: false),
                    LoginDate = table.Column<DateTime>(nullable: true),
                    ResetSendDate = table.Column<DateTime>(nullable: true),
                    ResetAttempts = table.Column<int>(nullable: false),
                    VerifyAttemptDate = table.Column<DateTime>(nullable: true),
                    VerifyAttempts = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    SendUpdates = table.Column<bool>(nullable: false),
                    AllowContact = table.Column<bool>(nullable: false),
                    AcceptedPrivacyStatement = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationPublicSectorTypes",
                columns: table => new
                {
                    OrganisationPublicSectorTypeId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PublicSectorTypeId = table.Column<int>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Retired = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationPublicSectorTypes", x => x.OrganisationPublicSectorTypeId);
                    table.ForeignKey(
                        name: "FK_OrganisationPublicSectorTypes_PublicSectorTypes_PublicSecto~",
                        column: x => x.PublicSectorTypeId,
                        principalTable: "PublicSectorTypes",
                        principalColumn: "PublicSectorTypeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SicCodes",
                columns: table => new
                {
                    SicCodeId = table.Column<int>(nullable: false),
                    SicSectionId = table.Column<string>(maxLength: 1, nullable: false),
                    Description = table.Column<string>(maxLength: 250, nullable: false),
                    Synonyms = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.SicCodes", x => x.SicCodeId);
                    table.ForeignKey(
                        name: "FK_dbo.SicCodes_dbo.SicSections_SicSectionId",
                        column: x => x.SicSectionId,
                        principalTable: "SicSections",
                        principalColumn: "SicSectionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReminderEmails",
                columns: table => new
                {
                    ReminderEmailId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(nullable: false),
                    SectorType = table.Column<int>(nullable: false),
                    DateChecked = table.Column<DateTime>(nullable: false),
                    EmailSent = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderEmails", x => x.ReminderEmailId);
                    table.ForeignKey(
                        name: "FK_ReminderEmails_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserStatus",
                columns: table => new
                {
                    UserStatusId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.UserStatus", x => x.UserStatusId);
                    table.ForeignKey(
                        name: "FK_UserStatus_Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.UserStatus_dbo.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Organisations",
                columns: table => new
                {
                    OrganisationId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CompanyNumber = table.Column<string>(maxLength: 10, nullable: true),
                    OrganisationName = table.Column<string>(maxLength: 100, nullable: false),
                    SectorTypeId = table.Column<int>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    EmployerReference = table.Column<string>(maxLength: 10, nullable: true),
                    DateOfCessation = table.Column<DateTime>(nullable: true),
                    LatestPublicSectorTypeId = table.Column<long>(nullable: true),
                    LastCheckedAgainstCompaniesHouse = table.Column<DateTime>(nullable: true),
                    OptedOutFromCompaniesHouseUpdate = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Organisations", x => x.OrganisationId);
                    table.ForeignKey(
                        name: "FK_dbo.Organisations_dbo.OrganisationPublicSectorTypes_LatestPublicSectorTypeId",
                        column: x => x.LatestPublicSectorTypeId,
                        principalTable: "OrganisationPublicSectorTypes",
                        principalColumn: "OrganisationPublicSectorTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Action = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    OrganisationId = table.Column<long>(nullable: true),
                    OriginalUserId = table.Column<long>(nullable: true),
                    ImpersonatedUserId = table.Column<long>(nullable: true),
                    DetailsString = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ImpersonatedUserId",
                        column: x => x.ImpersonatedUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_OriginalUserId",
                        column: x => x.OriginalUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationAddresses",
                columns: table => new
                {
                    AddressId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedByUserId = table.Column<long>(nullable: false),
                    Address1 = table.Column<string>(maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(maxLength: 100, nullable: true),
                    Address3 = table.Column<string>(maxLength: 100, nullable: true),
                    TownCity = table.Column<string>(maxLength: 100, nullable: true),
                    County = table.Column<string>(maxLength: 100, nullable: true),
                    Country = table.Column<string>(maxLength: 100, nullable: true),
                    PoBox = table.Column<string>(maxLength: 30, nullable: true),
                    PostCode = table.Column<string>(maxLength: 20, nullable: true),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    IsUkAddress = table.Column<bool>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationAddresses", x => x.AddressId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationAddresses_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationNames",
                columns: table => new
                {
                    OrganisationNameId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganisationId = table.Column<long>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationNames", x => x.OrganisationNameId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationNames_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationReferences",
                columns: table => new
                {
                    OrganisationReferenceId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganisationId = table.Column<long>(nullable: false),
                    ReferenceName = table.Column<string>(maxLength: 100, nullable: false),
                    ReferenceValue = table.Column<string>(maxLength: 100, nullable: false),
                    Created = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationReferences", x => x.OrganisationReferenceId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationReferences_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationScopes",
                columns: table => new
                {
                    OrganisationScopeId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganisationId = table.Column<long>(nullable: false),
                    ScopeStatusId = table.Column<int>(nullable: false),
                    ScopeStatusDate = table.Column<DateTime>(nullable: false),
                    RegisterStatusId = table.Column<int>(nullable: false),
                    RegisterStatusDate = table.Column<DateTime>(nullable: false),
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
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationScopes", x => x.OrganisationScopeId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationScopes_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationSicCodes",
                columns: table => new
                {
                    OrganisationSicCodeId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SicCodeId = table.Column<int>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Source = table.Column<string>(maxLength: 255, nullable: true),
                    Retired = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationSicCodes", x => x.OrganisationSicCodeId);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationSicCodes_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationSicCodes_dbo.SicCodes_SicCodeId",
                        column: x => x.SicCodeId,
                        principalTable: "SicCodes",
                        principalColumn: "SicCodeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrganisationStatus",
                columns: table => new
                {
                    OrganisationStatusId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganisationId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.OrganisationStatus", x => x.OrganisationStatusId);
                    table.ForeignKey(
                        name: "FK_OrganisationStatus_Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.OrganisationStatus_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Returns",
                columns: table => new
                {
                    ReturnId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrganisationId = table.Column<long>(nullable: false),
                    AccountingDate = table.Column<DateTime>(nullable: false),
                    DiffMeanHourlyPayPercent = table.Column<decimal>(nullable: false),
                    DiffMedianHourlyPercent = table.Column<decimal>(nullable: false),
                    DiffMeanBonusPercent = table.Column<decimal>(nullable: true),
                    DiffMedianBonusPercent = table.Column<decimal>(nullable: true),
                    MaleMedianBonusPayPercent = table.Column<decimal>(nullable: false),
                    FemaleMedianBonusPayPercent = table.Column<decimal>(nullable: false),
                    MaleLowerPayBand = table.Column<decimal>(nullable: false),
                    FemaleLowerPayBand = table.Column<decimal>(nullable: false),
                    MaleMiddlePayBand = table.Column<decimal>(nullable: false),
                    FemaleMiddlePayBand = table.Column<decimal>(nullable: false),
                    MaleUpperPayBand = table.Column<decimal>(nullable: false),
                    FemaleUpperPayBand = table.Column<decimal>(nullable: false),
                    MaleUpperQuartilePayBand = table.Column<decimal>(nullable: false),
                    FemaleUpperQuartilePayBand = table.Column<decimal>(nullable: false),
                    CompanyLinkToGPGInfo = table.Column<string>(maxLength: 255, nullable: true),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    JobTitle = table.Column<string>(maxLength: 100, nullable: true),
                    FirstName = table.Column<string>(maxLength: 50, nullable: true),
                    LastName = table.Column<string>(maxLength: 50, nullable: true),
                    MinEmployees = table.Column<int>(nullable: false, defaultValueSql: "((0))"),
                    MaxEmployees = table.Column<int>(nullable: false, defaultValueSql: "((0))"),
                    IsLateSubmission = table.Column<bool>(nullable: false),
                    LateReason = table.Column<string>(maxLength: 200, nullable: true),
                    Modifications = table.Column<string>(maxLength: 200, nullable: true),
                    EHRCResponse = table.Column<bool>(nullable: false, defaultValueSql: "false")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Returns", x => x.ReturnId);
                    table.ForeignKey(
                        name: "FK_dbo.Returns_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InactiveUserOrganisations",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    PIN = table.Column<string>(nullable: true),
                    PINSentDate = table.Column<DateTime>(nullable: true),
                    PITPNotifyLetterId = table.Column<string>(nullable: true),
                    PINConfirmedDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttemptDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttempts = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    AddressId = table.Column<long>(nullable: true),
                    MethodId = table.Column<int>(nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.InactiveUserOrganisations", x => new { x.UserId, x.OrganisationId });
                    table.ForeignKey(
                        name: "FK_InactiveUserOrganisations_OrganisationAddresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "OrganisationAddresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InactiveUserOrganisations_Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InactiveUserOrganisations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserOrganisations",
                columns: table => new
                {
                    UserId = table.Column<long>(nullable: false),
                    OrganisationId = table.Column<long>(nullable: false),
                    PIN = table.Column<string>(nullable: true),
                    PINSentDate = table.Column<DateTime>(nullable: true),
                    PITPNotifyLetterId = table.Column<string>(nullable: true),
                    PINConfirmedDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttemptDate = table.Column<DateTime>(nullable: true),
                    ConfirmAttempts = table.Column<int>(nullable: false),
                    Created = table.Column<DateTime>(nullable: false),
                    Modified = table.Column<DateTime>(nullable: false),
                    AddressId = table.Column<long>(nullable: true),
                    MethodId = table.Column<int>(nullable: false, defaultValueSql: "((0))")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.UserOrganisations", x => new { x.UserId, x.OrganisationId });
                    table.ForeignKey(
                        name: "FK_dbo.UserOrganisations_dbo.OrganisationAddresses_AddressId",
                        column: x => x.AddressId,
                        principalTable: "OrganisationAddresses",
                        principalColumn: "AddressId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.UserOrganisations_dbo.Organisations_OrganisationId",
                        column: x => x.OrganisationId,
                        principalTable: "Organisations",
                        principalColumn: "OrganisationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.UserOrganisations_dbo.Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReturnStatus",
                columns: table => new
                {
                    ReturnStatusId = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReturnId = table.Column<long>(nullable: false),
                    StatusId = table.Column<byte>(nullable: false),
                    StatusDate = table.Column<DateTime>(nullable: false),
                    StatusDetails = table.Column<string>(maxLength: 255, nullable: true),
                    ByUserId = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ReturnStatus", x => x.ReturnStatusId);
                    table.ForeignKey(
                        name: "FK_ReturnStatus_Users_ByUserId",
                        column: x => x.ByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_dbo.ReturnStatus_dbo.Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "ReturnId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ImpersonatedUserId",
                table: "AuditLogs",
                column: "ImpersonatedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OrganisationId",
                table: "AuditLogs",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OriginalUserId",
                table: "AuditLogs",
                column: "OriginalUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InactiveUserOrganisations_AddressId",
                table: "InactiveUserOrganisations",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_InactiveUserOrganisations_OrganisationId",
                table: "InactiveUserOrganisations",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_InactiveUserOrganisations_UserId",
                table: "InactiveUserOrganisations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationAddresses_OrganisationId",
                table: "OrganisationAddresses",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationAddresses_StatusId",
                table: "OrganisationAddresses",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationAddresses_StatusDate",
                table: "OrganisationAddresses",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationNames_Created",
                table: "OrganisationNames",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationNames_Name",
                table: "OrganisationNames",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationNames_OrganisationId",
                table: "OrganisationNames",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationPublicSectorTypes_Created",
                table: "OrganisationPublicSectorTypes",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationPublicSectorTypes_OrganisationId",
                table: "OrganisationPublicSectorTypes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationPublicSectorTypes_PublicSectorTypeId",
                table: "OrganisationPublicSectorTypes",
                column: "PublicSectorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationPublicSectorTypes_Retired",
                table: "OrganisationPublicSectorTypes",
                column: "Retired");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_Created",
                table: "OrganisationReferences",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_OrganisationId",
                table: "OrganisationReferences",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_ReferenceName",
                table: "OrganisationReferences",
                column: "ReferenceName");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationReferences_ReferenceValue",
                table: "OrganisationReferences",
                column: "ReferenceValue");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_CompanyNumber",
                table: "Organisations",
                column: "CompanyNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_EmployerReference",
                table: "Organisations",
                column: "EmployerReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_LatestPublicSectorTypeId",
                table: "Organisations",
                column: "LatestPublicSectorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_OrganisationName",
                table: "Organisations",
                column: "OrganisationName");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_SectorTypeId",
                table: "Organisations",
                column: "SectorTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Organisations_StatusId",
                table: "Organisations",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_OrganisationId",
                table: "OrganisationScopes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_RegisterStatusId",
                table: "OrganisationScopes",
                column: "RegisterStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_ScopeStatusId",
                table: "OrganisationScopes",
                column: "ScopeStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_ScopeStatusDate",
                table: "OrganisationScopes",
                column: "ScopeStatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_SnapshotDate",
                table: "OrganisationScopes",
                column: "SnapshotDate");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationScopes_StatusId",
                table: "OrganisationScopes",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSicCodes_Created",
                table: "OrganisationSicCodes",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSicCodes_OrganisationId",
                table: "OrganisationSicCodes",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSicCodes_Retired",
                table: "OrganisationSicCodes",
                column: "Retired");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationSicCodes_SicCodeId",
                table: "OrganisationSicCodes",
                column: "SicCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationStatus_ByUserId",
                table: "OrganisationStatus",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationStatus_OrganisationId",
                table: "OrganisationStatus",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganisationStatus_StatusDate",
                table: "OrganisationStatus",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_ReminderEmails_UserId",
                table: "ReminderEmails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_AccountingDate",
                table: "Returns",
                column: "AccountingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_OrganisationId",
                table: "Returns",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_StatusId",
                table: "Returns",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnStatus_ByUserId",
                table: "ReturnStatus",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnStatus_ReturnId",
                table: "ReturnStatus",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnStatus_StatusDate",
                table: "ReturnStatus",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_SicCodes_SicSectionId",
                table: "SicCodes",
                column: "SicSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganisations_AddressId",
                table: "UserOrganisations",
                column: "AddressId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganisations_OrganisationId",
                table: "UserOrganisations",
                column: "OrganisationId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganisations_UserId",
                table: "UserOrganisations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ContactEmailAddress",
                table: "Users",
                column: "ContactEmailAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ContactPhoneNumber",
                table: "Users",
                column: "ContactPhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailAddress",
                table: "Users",
                column: "EmailAddress");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StatusId",
                table: "Users",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatus_ByUserId",
                table: "UserStatus",
                column: "ByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatus_StatusDate",
                table: "UserStatus",
                column: "StatusDate");

            migrationBuilder.CreateIndex(
                name: "IX_UserStatus_UserId",
                table: "UserStatus",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "DraftReturns");

            migrationBuilder.DropTable(
                name: "Feedback");

            migrationBuilder.DropTable(
                name: "InactiveUserOrganisations");

            migrationBuilder.DropTable(
                name: "OrganisationNames");

            migrationBuilder.DropTable(
                name: "OrganisationReferences");

            migrationBuilder.DropTable(
                name: "OrganisationScopes");

            migrationBuilder.DropTable(
                name: "OrganisationSicCodes");

            migrationBuilder.DropTable(
                name: "OrganisationStatus");

            migrationBuilder.DropTable(
                name: "ReminderEmails");

            migrationBuilder.DropTable(
                name: "ReturnStatus");

            migrationBuilder.DropTable(
                name: "UserOrganisations");

            migrationBuilder.DropTable(
                name: "UserStatus");

            migrationBuilder.DropTable(
                name: "SicCodes");

            migrationBuilder.DropTable(
                name: "Returns");

            migrationBuilder.DropTable(
                name: "OrganisationAddresses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "SicSections");

            migrationBuilder.DropTable(
                name: "Organisations");

            migrationBuilder.DropTable(
                name: "OrganisationPublicSectorTypes");

            migrationBuilder.DropTable(
                name: "PublicSectorTypes");
        }
    }
}
