// <auto-generated />
using System;
using GenderPayGap.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace GenderPayGap.Database.Migrations
{
    [DbContext(typeof(GpgDatabaseContext))]
    partial class GpgDatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.4-servicing-10062")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("GenderPayGap.Database.AddressStatus", b =>
                {
                    b.Property<long>("AddressStatusId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AddressId");

                    b.Property<long>("ByUserId");

                    b.Property<byte>("Status")
                        .HasColumnName("StatusId");

                    b.Property<DateTime>("StatusDate");

                    b.Property<string>("StatusDetails")
                        .HasMaxLength(255);

                    b.HasKey("AddressStatusId")
                        .HasName("PK_dbo.AddressStatus");

                    b.HasIndex("AddressId");

                    b.HasIndex("ByUserId");

                    b.HasIndex("StatusDate");

                    b.ToTable("AddressStatus");
                });

            modelBuilder.Entity("GenderPayGap.Database.InactiveUserOrganisation", b =>
                {
                    b.Property<long>("UserId");

                    b.Property<long>("OrganisationId");

                    b.Property<long?>("AddressId");

                    b.Property<DateTime?>("ConfirmAttemptDate");

                    b.Property<int>("ConfirmAttempts");

                    b.Property<DateTime>("Created");

                    b.Property<int>("Method")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("MethodId")
                        .HasDefaultValueSql("((0))");

                    b.Property<DateTime>("Modified");

                    b.Property<string>("PIN");

                    b.Property<DateTime?>("PINConfirmedDate");

                    b.Property<DateTime?>("PINSentDate");

                    b.Property<string>("PITPNotifyLetterId");

                    b.HasKey("UserId", "OrganisationId")
                        .HasName("PK_dbo.InactiveUserOrganisations");

                    b.HasIndex("AddressId");

                    b.HasIndex("OrganisationId");

                    b.HasIndex("UserId");

                    b.ToTable("InactiveUserOrganisations");
                });

            modelBuilder.Entity("GenderPayGap.Database.Models.AuditLog", b =>
                {
                    b.Property<long>("AuditLogId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Action");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("now()");

                    b.Property<string>("DetailsString");

                    b.Property<long?>("ImpersonatedUserId");

                    b.Property<long?>("OrganisationId");

                    b.Property<long?>("OriginalUserId");

                    b.HasKey("AuditLogId");

                    b.HasIndex("ImpersonatedUserId");

                    b.HasIndex("OrganisationId");

                    b.HasIndex("OriginalUserId");

                    b.ToTable("AuditLogs");
                });

            modelBuilder.Entity("GenderPayGap.Database.Models.Feedback", b =>
                {
                    b.Property<long>("FeedbackId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool?>("ActionsToCloseGpg");

                    b.Property<bool?>("Charity");

                    b.Property<bool?>("CloseOrganisationGpg");

                    b.Property<bool?>("CompanyIntranet");

                    b.Property<DateTime>("CreatedDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("now()");

                    b.Property<string>("Details")
                        .HasMaxLength(2000);

                    b.Property<int?>("Difficulty");

                    b.Property<string>("EmailAddress");

                    b.Property<bool?>("EmployeeInterestedInOrganisationData");

                    b.Property<bool?>("EmployerUnion");

                    b.Property<int>("FeedbackStatus");

                    b.Property<bool?>("FindOutAboutGpg");

                    b.Property<bool?>("InternetSearch");

                    b.Property<bool?>("LobbyGroup");

                    b.Property<bool?>("ManagerInvolvedInGpgReport");

                    b.Property<bool?>("NewsArticle");

                    b.Property<bool?>("OtherPerson");

                    b.Property<string>("OtherPersonText")
                        .HasMaxLength(2000);

                    b.Property<bool?>("OtherReason");

                    b.Property<string>("OtherReasonText")
                        .HasMaxLength(2000);

                    b.Property<bool?>("OtherSource");

                    b.Property<string>("OtherSourceText")
                        .HasMaxLength(2000);

                    b.Property<bool?>("PersonInterestedInGeneralGpg");

                    b.Property<bool?>("PersonInterestedInSpecificOrganisationGpg");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool?>("Report");

                    b.Property<bool?>("ReportOrganisationGpgData");

                    b.Property<bool?>("ResponsibleForReportingGpg");

                    b.Property<bool?>("SocialMedia");

                    b.Property<bool?>("ViewSpecificOrganisationGpg");

                    b.HasKey("FeedbackId");

                    b.ToTable("Feedback");
                });

            modelBuilder.Entity("GenderPayGap.Database.Models.ReminderEmail", b =>
                {
                    b.Property<long>("ReminderEmailId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateChecked");

                    b.Property<bool>("EmailSent");

                    b.Property<int>("SectorType");

                    b.Property<long>("UserId");

                    b.HasKey("ReminderEmailId");

                    b.HasIndex("UserId");

                    b.ToTable("ReminderEmails");
                });

            modelBuilder.Entity("GenderPayGap.Database.Organisation", b =>
                {
                    b.Property<long>("OrganisationId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CompanyNumber")
                        .HasMaxLength(10);

                    b.Property<DateTime>("Created");

                    b.Property<DateTime?>("DateOfCessation");

                    b.Property<string>("EmployerReference")
                        .HasMaxLength(10);

                    b.Property<DateTime?>("LastCheckedAgainstCompaniesHouse");

                    b.Property<long?>("LatestPublicSectorTypeId");

                    b.Property<DateTime>("Modified");

                    b.Property<bool>("OptedOutFromCompaniesHouseUpdate");

                    b.Property<string>("OrganisationName")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<int>("SectorType")
                        .HasColumnName("SectorTypeId");

                    b.Property<string>("SecurityCode");

                    b.Property<DateTime?>("SecurityCodeCreatedDateTime");

                    b.Property<DateTime?>("SecurityCodeExpiryDateTime");

                    b.Property<byte>("Status")
                        .HasColumnName("StatusId");

                    b.Property<DateTime>("StatusDate");

                    b.Property<string>("StatusDetails")
                        .HasMaxLength(255);

                    b.HasKey("OrganisationId")
                        .HasName("PK_dbo.Organisations");

                    b.HasIndex("CompanyNumber")
                        .IsUnique()
                        .HasFilter("([CompanyNumber] IS NOT NULL)");

                    b.HasIndex("EmployerReference")
                        .IsUnique()
                        .HasFilter("([EmployerReference] IS NOT NULL)");

                    b.HasIndex("LatestPublicSectorTypeId");

                    b.HasIndex("OrganisationName");

                    b.HasIndex("SectorType");

                    b.HasIndex("Status");

                    b.ToTable("Organisations");
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationAddress", b =>
                {
                    b.Property<long>("AddressId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address1")
                        .HasMaxLength(100);

                    b.Property<string>("Address2")
                        .HasMaxLength(100);

                    b.Property<string>("Address3")
                        .HasMaxLength(100);

                    b.Property<string>("Country")
                        .HasMaxLength(100);

                    b.Property<string>("County")
                        .HasMaxLength(100);

                    b.Property<DateTime>("Created");

                    b.Property<long>("CreatedByUserId");

                    b.Property<bool?>("IsUkAddress");

                    b.Property<DateTime>("Modified");

                    b.Property<long>("OrganisationId");

                    b.Property<string>("PoBox")
                        .HasMaxLength(30);

                    b.Property<string>("PostCode")
                        .HasMaxLength(20);

                    b.Property<string>("Source")
                        .HasMaxLength(255);

                    b.Property<byte>("Status")
                        .HasColumnName("StatusId");

                    b.Property<DateTime>("StatusDate");

                    b.Property<string>("StatusDetails")
                        .HasMaxLength(255);

                    b.Property<string>("TownCity")
                        .HasMaxLength(100);

                    b.HasKey("AddressId")
                        .HasName("PK_dbo.OrganisationAddresses");

                    b.HasIndex("OrganisationId");

                    b.HasIndex("Status");

                    b.HasIndex("StatusDate");

                    b.ToTable("OrganisationAddresses");
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationName", b =>
                {
                    b.Property<long>("OrganisationNameId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<long>("OrganisationId");

                    b.Property<string>("Source")
                        .HasMaxLength(255);

                    b.HasKey("OrganisationNameId")
                        .HasName("PK_dbo.OrganisationNames");

                    b.HasIndex("Created");

                    b.HasIndex("Name");

                    b.HasIndex("OrganisationId");

                    b.ToTable("OrganisationNames");
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationPublicSectorType", b =>
                {
                    b.Property<long>("OrganisationPublicSectorTypeId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<long>("OrganisationId");

                    b.Property<int>("PublicSectorTypeId");

                    b.Property<DateTime?>("Retired");

                    b.Property<string>("Source")
                        .HasMaxLength(255);

                    b.HasKey("OrganisationPublicSectorTypeId")
                        .HasName("PK_dbo.OrganisationPublicSectorTypes");

                    b.HasIndex("Created");

                    b.HasIndex("OrganisationId");

                    b.HasIndex("PublicSectorTypeId");

                    b.HasIndex("Retired");

                    b.ToTable("OrganisationPublicSectorTypes");
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationReference", b =>
                {
                    b.Property<long>("OrganisationReferenceId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<long>("OrganisationId");

                    b.Property<string>("ReferenceName")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<string>("ReferenceValue")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.HasKey("OrganisationReferenceId")
                        .HasName("PK_dbo.OrganisationReferences");

                    b.HasIndex("Created");

                    b.HasIndex("OrganisationId");

                    b.HasIndex("ReferenceName");

                    b.HasIndex("ReferenceValue");

                    b.ToTable("OrganisationReferences");
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationScope", b =>
                {
                    b.Property<long>("OrganisationScopeId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CampaignId")
                        .HasMaxLength(50);

                    b.Property<string>("ContactEmailAddress")
                        .HasMaxLength(255);

                    b.Property<string>("ContactFirstname")
                        .HasMaxLength(50);

                    b.Property<string>("ContactLastname")
                        .HasMaxLength(50);

                    b.Property<long>("OrganisationId");

                    b.Property<bool?>("ReadGuidance");

                    b.Property<string>("Reason")
                        .HasMaxLength(1000);

                    b.Property<int>("RegisterStatus")
                        .HasColumnName("RegisterStatusId");

                    b.Property<DateTime>("RegisterStatusDate");

                    b.Property<int>("ScopeStatus")
                        .HasColumnName("ScopeStatusId");

                    b.Property<DateTime>("ScopeStatusDate");

                    b.Property<DateTime>("SnapshotDate")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("('1900-01-01T00:00:00.000')");

                    b.Property<byte>("Status")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("StatusId")
                        .HasDefaultValueSql("((0))");

                    b.Property<string>("StatusDetails")
                        .HasMaxLength(255);

                    b.HasKey("OrganisationScopeId")
                        .HasName("PK_dbo.OrganisationScopes");

                    b.HasIndex("OrganisationId");

                    b.HasIndex("RegisterStatus");

                    b.HasIndex("ScopeStatus");

                    b.HasIndex("ScopeStatusDate");

                    b.HasIndex("SnapshotDate");

                    b.HasIndex("Status");

                    b.ToTable("OrganisationScopes");
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationSicCode", b =>
                {
                    b.Property<long>("OrganisationSicCodeId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<long>("OrganisationId");

                    b.Property<DateTime?>("Retired");

                    b.Property<int>("SicCodeId");

                    b.Property<string>("Source")
                        .HasMaxLength(255);

                    b.HasKey("OrganisationSicCodeId")
                        .HasName("PK_dbo.OrganisationSicCodes");

                    b.HasIndex("Created");

                    b.HasIndex("OrganisationId");

                    b.HasIndex("Retired");

                    b.HasIndex("SicCodeId");

                    b.ToTable("OrganisationSicCodes");
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationStatus", b =>
                {
                    b.Property<long>("OrganisationStatusId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("ByUserId");

                    b.Property<long>("OrganisationId");

                    b.Property<byte>("Status")
                        .HasColumnName("StatusId");

                    b.Property<DateTime>("StatusDate");

                    b.Property<string>("StatusDetails")
                        .HasMaxLength(255);

                    b.HasKey("OrganisationStatusId")
                        .HasName("PK_dbo.OrganisationStatus");

                    b.HasIndex("ByUserId");

                    b.HasIndex("OrganisationId");

                    b.HasIndex("StatusDate");

                    b.ToTable("OrganisationStatus");
                });

            modelBuilder.Entity("GenderPayGap.Database.PublicSectorType", b =>
                {
                    b.Property<int>("PublicSectorTypeId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Created");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250);

                    b.HasKey("PublicSectorTypeId")
                        .HasName("PK_dbo.PublicSectorTypes");

                    b.ToTable("PublicSectorTypes");
                });

            modelBuilder.Entity("GenderPayGap.Database.Return", b =>
                {
                    b.Property<long>("ReturnId")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("AccountingDate");

                    b.Property<string>("CompanyLinkToGPGInfo")
                        .HasMaxLength(255);

                    b.Property<DateTime>("Created");

                    b.Property<decimal?>("DiffMeanBonusPercent");

                    b.Property<decimal>("DiffMeanHourlyPayPercent");

                    b.Property<decimal?>("DiffMedianBonusPercent");

                    b.Property<decimal>("DiffMedianHourlyPercent");

                    b.Property<bool>("EHRCResponse")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("EHRCResponse")
                        .HasDefaultValueSql("false");

                    b.Property<decimal>("FemaleLowerPayBand");

                    b.Property<decimal>("FemaleMedianBonusPayPercent");

                    b.Property<decimal>("FemaleMiddlePayBand");

                    b.Property<decimal>("FemaleUpperPayBand");

                    b.Property<decimal>("FemaleUpperQuartilePayBand");

                    b.Property<string>("FirstName")
                        .HasMaxLength(50);

                    b.Property<bool>("IsLateSubmission");

                    b.Property<string>("JobTitle")
                        .HasMaxLength(100);

                    b.Property<string>("LastName")
                        .HasMaxLength(50);

                    b.Property<string>("LateReason")
                        .HasMaxLength(200);

                    b.Property<decimal>("MaleLowerPayBand");

                    b.Property<decimal>("MaleMedianBonusPayPercent");

                    b.Property<decimal>("MaleMiddlePayBand");

                    b.Property<decimal>("MaleUpperPayBand");

                    b.Property<decimal>("MaleUpperQuartilePayBand");

                    b.Property<int>("MaxEmployees")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("((0))");

                    b.Property<int>("MinEmployees")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("((0))");

                    b.Property<string>("Modifications")
                        .HasMaxLength(200);

                    b.Property<DateTime>("Modified");

                    b.Property<long>("OrganisationId");

                    b.Property<byte>("Status")
                        .HasColumnName("StatusId");

                    b.Property<DateTime>("StatusDate");

                    b.Property<string>("StatusDetails")
                        .HasMaxLength(255);

                    b.HasKey("ReturnId")
                        .HasName("PK_dbo.Returns");

                    b.HasIndex("AccountingDate");

                    b.HasIndex("OrganisationId");

                    b.HasIndex("Status");

                    b.ToTable("Returns");
                });

            modelBuilder.Entity("GenderPayGap.Database.ReturnStatus", b =>
                {
                    b.Property<long>("ReturnStatusId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ByUserId");

                    b.Property<long>("ReturnId");

                    b.Property<byte>("Status")
                        .HasColumnName("StatusId");

                    b.Property<DateTime>("StatusDate");

                    b.Property<string>("StatusDetails")
                        .HasMaxLength(255);

                    b.HasKey("ReturnStatusId")
                        .HasName("PK_dbo.ReturnStatus");

                    b.HasIndex("ByUserId");

                    b.HasIndex("ReturnId");

                    b.HasIndex("StatusDate");

                    b.ToTable("ReturnStatus");
                });

            modelBuilder.Entity("GenderPayGap.Database.SicCode", b =>
                {
                    b.Property<int>("SicCodeId");

                    b.Property<DateTime>("Created");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250);

                    b.Property<string>("SicSectionId")
                        .IsRequired()
                        .HasMaxLength(1);

                    b.HasKey("SicCodeId")
                        .HasName("PK_dbo.SicCodes");

                    b.HasIndex("SicSectionId");

                    b.ToTable("SicCodes");
                });

            modelBuilder.Entity("GenderPayGap.Database.SicSection", b =>
                {
                    b.Property<string>("SicSectionId")
                        .HasMaxLength(1);

                    b.Property<DateTime>("Created");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(250);

                    b.HasKey("SicSectionId")
                        .HasName("PK_dbo.SicSections");

                    b.ToTable("SicSections");
                });

            modelBuilder.Entity("GenderPayGap.Database.User", b =>
                {
                    b.Property<long>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContactEmailAddressDB")
                        .HasColumnName("ContactEmailAddress")
                        .HasMaxLength(255);

                    b.Property<string>("ContactFirstName")
                        .HasMaxLength(50);

                    b.Property<string>("ContactJobTitle")
                        .HasMaxLength(50);

                    b.Property<string>("ContactLastName")
                        .HasMaxLength(50);

                    b.Property<string>("ContactOrganisation")
                        .HasMaxLength(100);

                    b.Property<string>("ContactPhoneNumber")
                        .HasMaxLength(20);

                    b.Property<DateTime>("Created");

                    b.Property<string>("EmailAddressDB")
                        .IsRequired()
                        .HasColumnName("EmailAddress")
                        .HasMaxLength(255);

                    b.Property<DateTime?>("EmailVerifiedDate");

                    b.Property<string>("EmailVerifyHash")
                        .HasMaxLength(250);

                    b.Property<DateTime?>("EmailVerifySendDate");

                    b.Property<string>("Firstname")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int>("HashingAlgorithm");

                    b.Property<string>("JobTitle")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<string>("Lastname")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.Property<int>("LoginAttempts");

                    b.Property<DateTime?>("LoginDate");

                    b.Property<DateTime>("Modified");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasMaxLength(250);

                    b.Property<int>("ResetAttempts");

                    b.Property<DateTime?>("ResetSendDate");

                    b.Property<string>("Salt");

                    b.Property<byte>("Status")
                        .HasColumnName("StatusId");

                    b.Property<DateTime>("StatusDate");

                    b.Property<string>("StatusDetails")
                        .HasMaxLength(255);

                    b.Property<DateTime?>("VerifyAttemptDate");

                    b.Property<int>("VerifyAttempts");

                    b.HasKey("UserId")
                        .HasName("PK_dbo.Users");

                    b.HasIndex("ContactEmailAddressDB");

                    b.HasIndex("ContactPhoneNumber");

                    b.HasIndex("EmailAddressDB");

                    b.HasIndex("Status");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("GenderPayGap.Database.UserOrganisation", b =>
                {
                    b.Property<long>("UserId");

                    b.Property<long>("OrganisationId");

                    b.Property<long?>("AddressId");

                    b.Property<DateTime?>("ConfirmAttemptDate");

                    b.Property<int>("ConfirmAttempts");

                    b.Property<DateTime>("Created");

                    b.Property<int>("Method")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("MethodId")
                        .HasDefaultValueSql("((0))");

                    b.Property<DateTime>("Modified");

                    b.Property<string>("PIN");

                    b.Property<DateTime?>("PINConfirmedDate");

                    b.Property<DateTime?>("PINSentDate");

                    b.Property<string>("PITPNotifyLetterId");

                    b.HasKey("UserId", "OrganisationId")
                        .HasName("PK_dbo.UserOrganisations");

                    b.HasIndex("AddressId");

                    b.HasIndex("OrganisationId");

                    b.HasIndex("UserId");

                    b.ToTable("UserOrganisations");
                });

            modelBuilder.Entity("GenderPayGap.Database.UserSetting", b =>
                {
                    b.Property<long>("UserId");

                    b.Property<byte>("Key");

                    b.Property<DateTime>("Modified");

                    b.Property<string>("Value")
                        .HasMaxLength(50);

                    b.HasKey("UserId", "Key")
                        .HasName("PK_dbo.UserSettings");

                    b.HasIndex("UserId");

                    b.ToTable("UserSettings");
                });

            modelBuilder.Entity("GenderPayGap.Database.UserStatus", b =>
                {
                    b.Property<long>("UserStatusId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("ByUserId");

                    b.Property<byte>("Status")
                        .HasColumnName("StatusId");

                    b.Property<DateTime>("StatusDate");

                    b.Property<string>("StatusDetails")
                        .HasMaxLength(255);

                    b.Property<long>("UserId");

                    b.HasKey("UserStatusId")
                        .HasName("PK_dbo.UserStatus");

                    b.HasIndex("ByUserId");

                    b.HasIndex("StatusDate");

                    b.HasIndex("UserId");

                    b.ToTable("UserStatus");
                });

            modelBuilder.Entity("GenderPayGap.Database.AddressStatus", b =>
                {
                    b.HasOne("GenderPayGap.Database.OrganisationAddress", "Address")
                        .WithMany("AddressStatuses")
                        .HasForeignKey("AddressId")
                        .HasConstraintName("FK_dbo.AddressStatus_dbo.OrganisationAddresses_AddressId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("GenderPayGap.Database.User", "ByUser")
                        .WithMany()
                        .HasForeignKey("ByUserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("GenderPayGap.Database.InactiveUserOrganisation", b =>
                {
                    b.HasOne("GenderPayGap.Database.OrganisationAddress", "Address")
                        .WithMany()
                        .HasForeignKey("AddressId");

                    b.HasOne("GenderPayGap.Database.Organisation", "Organisation")
                        .WithMany()
                        .HasForeignKey("OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GenderPayGap.Database.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.Models.AuditLog", b =>
                {
                    b.HasOne("GenderPayGap.Database.User", "ImpersonatedUser")
                        .WithMany()
                        .HasForeignKey("ImpersonatedUserId");

                    b.HasOne("GenderPayGap.Database.Organisation", "Organisation")
                        .WithMany()
                        .HasForeignKey("OrganisationId");

                    b.HasOne("GenderPayGap.Database.User", "OriginalUser")
                        .WithMany()
                        .HasForeignKey("OriginalUserId");
                });

            modelBuilder.Entity("GenderPayGap.Database.Models.ReminderEmail", b =>
                {
                    b.HasOne("GenderPayGap.Database.User", "User")
                        .WithMany("ReminderEmails")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.Organisation", b =>
                {
                    b.HasOne("GenderPayGap.Database.OrganisationPublicSectorType", "LatestPublicSectorType")
                        .WithMany("Organisations")
                        .HasForeignKey("LatestPublicSectorTypeId")
                        .HasConstraintName("FK_dbo.Organisations_dbo.OrganisationPublicSectorTypes_LatestPublicSectorTypeId");
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationAddress", b =>
                {
                    b.HasOne("GenderPayGap.Database.Organisation", "Organisation")
                        .WithMany("OrganisationAddresses")
                        .HasForeignKey("OrganisationId")
                        .HasConstraintName("FK_dbo.OrganisationAddresses_dbo.Organisations_OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationName", b =>
                {
                    b.HasOne("GenderPayGap.Database.Organisation", "Organisation")
                        .WithMany("OrganisationNames")
                        .HasForeignKey("OrganisationId")
                        .HasConstraintName("FK_dbo.OrganisationNames_dbo.Organisations_OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationPublicSectorType", b =>
                {
                    b.HasOne("GenderPayGap.Database.PublicSectorType", "PublicSectorType")
                        .WithMany()
                        .HasForeignKey("PublicSectorTypeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationReference", b =>
                {
                    b.HasOne("GenderPayGap.Database.Organisation", "Organisation")
                        .WithMany("OrganisationReferences")
                        .HasForeignKey("OrganisationId")
                        .HasConstraintName("FK_dbo.OrganisationReferences_dbo.Organisations_OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationScope", b =>
                {
                    b.HasOne("GenderPayGap.Database.Organisation", "Organisation")
                        .WithMany("OrganisationScopes")
                        .HasForeignKey("OrganisationId")
                        .HasConstraintName("FK_dbo.OrganisationScopes_dbo.Organisations_OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationSicCode", b =>
                {
                    b.HasOne("GenderPayGap.Database.Organisation", "Organisation")
                        .WithMany("OrganisationSicCodes")
                        .HasForeignKey("OrganisationId")
                        .HasConstraintName("FK_dbo.OrganisationSicCodes_dbo.Organisations_OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GenderPayGap.Database.SicCode", "SicCode")
                        .WithMany("OrganisationSicCodes")
                        .HasForeignKey("SicCodeId")
                        .HasConstraintName("FK_dbo.OrganisationSicCodes_dbo.SicCodes_SicCodeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.OrganisationStatus", b =>
                {
                    b.HasOne("GenderPayGap.Database.User", "ByUser")
                        .WithMany()
                        .HasForeignKey("ByUserId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("GenderPayGap.Database.Organisation", "Organisation")
                        .WithMany("OrganisationStatuses")
                        .HasForeignKey("OrganisationId")
                        .HasConstraintName("FK_dbo.OrganisationStatus_dbo.Organisations_OrganisationId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("GenderPayGap.Database.Return", b =>
                {
                    b.HasOne("GenderPayGap.Database.Organisation", "Organisation")
                        .WithMany("Returns")
                        .HasForeignKey("OrganisationId")
                        .HasConstraintName("FK_dbo.Returns_dbo.Organisations_OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.ReturnStatus", b =>
                {
                    b.HasOne("GenderPayGap.Database.User", "ByUser")
                        .WithMany()
                        .HasForeignKey("ByUserId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("GenderPayGap.Database.Return", "Return")
                        .WithMany("ReturnStatuses")
                        .HasForeignKey("ReturnId")
                        .HasConstraintName("FK_dbo.ReturnStatus_dbo.Returns_ReturnId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            modelBuilder.Entity("GenderPayGap.Database.SicCode", b =>
                {
                    b.HasOne("GenderPayGap.Database.SicSection", "SicSection")
                        .WithMany("SicCodes")
                        .HasForeignKey("SicSectionId")
                        .HasConstraintName("FK_dbo.SicCodes_dbo.SicSections_SicSectionId");
                });

            modelBuilder.Entity("GenderPayGap.Database.UserOrganisation", b =>
                {
                    b.HasOne("GenderPayGap.Database.OrganisationAddress", "Address")
                        .WithMany("UserOrganisations")
                        .HasForeignKey("AddressId")
                        .HasConstraintName("FK_dbo.UserOrganisations_dbo.OrganisationAddresses_AddressId");

                    b.HasOne("GenderPayGap.Database.Organisation", "Organisation")
                        .WithMany("UserOrganisations")
                        .HasForeignKey("OrganisationId")
                        .HasConstraintName("FK_dbo.UserOrganisations_dbo.Organisations_OrganisationId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GenderPayGap.Database.User", "User")
                        .WithMany("UserOrganisations")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_dbo.UserOrganisations_dbo.Users_UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.UserSetting", b =>
                {
                    b.HasOne("GenderPayGap.Database.User", "User")
                        .WithMany("UserSettings")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_dbo.UserSettings_dbo.Users_UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GenderPayGap.Database.UserStatus", b =>
                {
                    b.HasOne("GenderPayGap.Database.User", "ByUser")
                        .WithMany()
                        .HasForeignKey("ByUserId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.HasOne("GenderPayGap.Database.User", "User")
                        .WithMany("UserStatuses")
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_dbo.UserStatus_dbo.Users_UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });
#pragma warning restore 612, 618
        }
    }
}
