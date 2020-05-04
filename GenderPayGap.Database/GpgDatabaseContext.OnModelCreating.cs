using GenderPayGap.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.Database
{
    public partial class GpgDatabaseContext : DbContext
    {

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Map the correct entity to table names
            modelBuilder.Entity<Organisation>().ToTable("Organisations");
            modelBuilder.Entity<OrganisationAddress>().ToTable("OrganisationAddresses");
            modelBuilder.Entity<OrganisationName>().ToTable("OrganisationNames");
            modelBuilder.Entity<OrganisationReference>().ToTable("OrganisationReferences");
            modelBuilder.Entity<OrganisationScope>().ToTable("OrganisationScopes");
            modelBuilder.Entity<OrganisationSicCode>().ToTable("OrganisationSicCodes");
            modelBuilder.Entity<Return>().ToTable("Returns");
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<UserStatus>().ToTable("UserStatus");
            modelBuilder.Entity<PublicSectorType>().ToTable("PublicSectorTypes");
            modelBuilder.Entity<OrganisationPublicSectorType>().ToTable("OrganisationPublicSectorTypes");
            modelBuilder.Entity<Feedback>().ToTable("Feedback");
            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs");
            modelBuilder.Entity<ReminderEmail>().ToTable("ReminderEmails");

            #region AddressStatus

            modelBuilder.Entity<AddressStatus>(
                entity => {
                    entity.HasKey(e => e.AddressStatusId).HasName("PK_dbo.AddressStatus");

                    entity.HasIndex(e => e.AddressId)
                        .HasName("IX_AddressId");

                    entity.HasIndex(e => e.ByUserId)
                        .HasName("IX_ByUserId");

                    entity.HasIndex(e => e.StatusDate)
                        .HasName("IX_StatusDate");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);
                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.HasOne(d => d.Address)
                        .WithMany(p => p.AddressStatuses)
                        .HasForeignKey(d => d.AddressId)
                        .HasConstraintName("FK_dbo.AddressStatus_dbo.OrganisationAddresses_AddressId")
                        .OnDelete(DeleteBehavior.Restrict);
                        
                });

            #endregion

            #region OrganisationAddress

            modelBuilder.Entity<OrganisationAddress>(
                entity => {
                    entity.HasKey(e => e.AddressId).HasName("PK_dbo.OrganisationAddresses");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.StatusDate)
                        .HasName("IX_StatusDate");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.Address1).HasMaxLength(100);

                    entity.Property(e => e.Address2).HasMaxLength(100);

                    entity.Property(e => e.Address3).HasMaxLength(100);

                    entity.Property(e => e.Country).HasMaxLength(100);

                    entity.Property(e => e.County).HasMaxLength(100);

                    entity.Property(e => e.PoBox).HasMaxLength(30);

                    entity.Property(e => e.PostCode).HasMaxLength(20);

                    entity.Property(e => e.Source).HasMaxLength(255);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.Property(e => e.TownCity).HasMaxLength(100);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationAddresses)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationAddresses_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region OrganisationName

            modelBuilder.Entity<OrganisationName>(
                entity => {
                    entity.HasKey(e => e.OrganisationNameId).HasName("PK_dbo.OrganisationNames");

                    entity.HasIndex(e => e.Created)
                        .HasName("IX_Created");

                    entity.HasIndex(e => e.Name)
                        .HasName("IX_Name");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.Property(e => e.Name)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.Property(e => e.Source).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationNames)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationNames_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region OrganisationReference

            modelBuilder.Entity<OrganisationReference>(
                entity => {
                    entity.HasKey(e => e.OrganisationReferenceId).HasName("PK_dbo.OrganisationReferences");

                    entity.HasIndex(e => e.Created)
                        .HasName("IX_Created");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.ReferenceName)
                        .HasName("IX_ReferenceName");

                    entity.HasIndex(e => e.ReferenceValue)
                        .HasName("IX_ReferenceValue");

                    entity.Property(e => e.ReferenceName)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.Property(e => e.ReferenceValue)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationReferences)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationReferences_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region Organisation

            modelBuilder.Entity<Organisation>(
                entity => {
                    entity.HasKey(e => e.OrganisationId).HasName("PK_dbo.Organisations");

                    entity.HasIndex(e => e.CompanyNumber)
                        .HasName("idx_Organisations_CompanyNumber")
                        .IsUnique()
                        .HasFilter("([CompanyNumber] IS NOT NULL)");

                    entity.HasIndex(e => e.EmployerReference)
                        .HasName("idx_Organisations_EmployerReference")
                        .IsUnique()
                        .HasFilter("([EmployerReference] IS NOT NULL)");

                    entity.HasIndex(e => e.OrganisationName)
                        .HasName("IX_OrganisationName");

                    entity.HasIndex(e => e.SectorType)
                        .HasName("IX_SectorTypeId");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.Property(e => e.Status).HasColumnName("StatusId");
                    entity.Property(e => e.SectorType).HasColumnName("SectorTypeId");

                    entity.Property(e => e.CompanyNumber).HasMaxLength(10);

                    entity.Property(e => e.EmployerReference).HasMaxLength(10);

                    entity.Property(e => e.OrganisationName)
                        .IsRequired()
                        .HasMaxLength(100);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.LatestPublicSectorType)
                        .WithMany(x => x.Organisations)
                        .HasForeignKey(d => d.LatestPublicSectorTypeId)
                        .HasConstraintName("FK_dbo.Organisations_dbo.OrganisationPublicSectorTypes_LatestPublicSectorTypeId");
                });

            #endregion

            #region OrganisationScope

            modelBuilder.Entity<OrganisationScope>(
                entity => {
                    entity.HasKey(e => e.OrganisationScopeId).HasName("PK_dbo.OrganisationScopes");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.RegisterStatus)
                        .HasName("IX_RegisterStatusId");

                    entity.HasIndex(e => e.ScopeStatusDate)
                        .HasName("IX_ScopeStatusDate");

                    entity.HasIndex(e => e.ScopeStatus)
                        .HasName("IX_ScopeStatusId");

                    entity.HasIndex(e => e.SnapshotDate)
                        .HasName("IX_SnapshotDate");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.Property(e => e.ScopeStatus).HasColumnName("ScopeStatusId");
                    entity.Property(e => e.RegisterStatus).HasColumnName("RegisterStatusId");
                    entity.Property(e => e.Status).HasColumnName("StatusId").HasDefaultValueSql("((0))");

                    entity.Property(e => e.CampaignId).HasMaxLength(50);

                    entity.Property(e => e.ContactEmailAddress).HasMaxLength(255);

                    entity.Property(e => e.ContactFirstname).HasMaxLength(50);

                    entity.Property(e => e.ContactLastname).HasMaxLength(50);

                    entity.Property(e => e.Reason).HasMaxLength(1000);

                    entity.Property(e => e.SnapshotDate).HasDefaultValueSql("('1900-01-01T00:00:00.000')");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationScopes)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationScopes_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region OrganisationSicCode

            modelBuilder.Entity<OrganisationSicCode>(
                entity => {
                    entity.HasKey(e => e.OrganisationSicCodeId).HasName("PK_dbo.OrganisationSicCodes");

                    entity.HasIndex(e => e.Created)
                        .HasName("IX_Created");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.Retired)
                        .HasName("IX_Retired");

                    entity.HasIndex(e => e.SicCodeId)
                        .HasName("IX_SicCodeId");

                    entity.Property(e => e.Source).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationSicCodes)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationSicCodes_dbo.Organisations_OrganisationId");

                    entity.HasOne(d => d.SicCode)
                        .WithMany(p => p.OrganisationSicCodes)
                        .HasForeignKey(d => d.SicCodeId)
                        .HasConstraintName("FK_dbo.OrganisationSicCodes_dbo.SicCodes_SicCodeId");
                });

            #endregion

            #region OrganisationStatus

            modelBuilder.Entity<OrganisationStatus>(
                entity => {
                    entity.HasKey(e => e.OrganisationStatusId).HasName("PK_dbo.OrganisationStatus");
                    entity.HasIndex(e => e.ByUserId)
                        .HasName("IX_ByUserId");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.StatusDate)
                        .HasName("IX_StatusDate");

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.OrganisationStatuses)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.OrganisationStatus_dbo.Organisations_OrganisationId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            #endregion

            #region Return

            modelBuilder.Entity<Return>(
                entity => {
                    entity.HasKey(e => e.ReturnId).HasName("PK_dbo.Returns");

                    entity.HasIndex(e => e.AccountingDate)
                        .HasName("IX_AccountingDate");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.Property(e => e.CompanyLinkToGPGInfo)
                        .HasMaxLength(255);

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.EHRCResponse).HasColumnName("EHRCResponse").HasDefaultValueSql("((0))");
                    entity.Property(e => e.MinEmployees).HasDefaultValueSql("((0))");
                    entity.Property(e => e.MaxEmployees).HasDefaultValueSql("((0))");

                    entity.Property(e => e.FirstName).HasMaxLength(50);

                    entity.Property(e => e.JobTitle).HasMaxLength(100);

                    entity.Property(e => e.LastName).HasMaxLength(50);

                    entity.Property(e => e.LateReason).HasMaxLength(200);

                    entity.Property(e => e.Modifications).HasMaxLength(200);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.Returns)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.Returns_dbo.Organisations_OrganisationId");
                });

            #endregion

            #region ReturnStatus

            modelBuilder.Entity<ReturnStatus>(
                entity => {
                    entity.HasKey(e => e.ReturnStatusId).HasName("PK_dbo.ReturnStatus");

                    entity.HasIndex(e => e.ByUserId)
                        .HasName("IX_ByUserId");

                    entity.HasIndex(e => e.ReturnId)
                        .HasName("IX_ReturnId");

                    entity.HasIndex(e => e.StatusDate)
                        .HasName("IX_StatusDate");

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.Return)
                        .WithMany(p => p.ReturnStatuses)
                        .HasForeignKey(d => d.ReturnId)
                        .HasConstraintName("FK_dbo.ReturnStatus_dbo.Returns_ReturnId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            #endregion

            #region SicCode

            modelBuilder.Entity<SicCode>(
                entity => {
                    entity.HasKey(e => e.SicCodeId).HasName("PK_dbo.SicCodes");

                    entity.HasIndex(e => e.SicSectionId)
                        .HasName("IX_SicSectionId");

                    entity.Property(e => e.SicCodeId).ValueGeneratedNever();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(250);

                    entity.Property(e => e.SicSectionId)
                        .IsRequired()
                        .HasMaxLength(1);

                    entity.HasOne(d => d.SicSection)
                        .WithMany(p => p.SicCodes)
                        .HasForeignKey(d => d.SicSectionId)
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_dbo.SicCodes_dbo.SicSections_SicSectionId");
                });

            #endregion

            #region SicSection

            modelBuilder.Entity<SicSection>(
                entity => {
                    entity.HasKey(e => e.SicSectionId).HasName("PK_dbo.SicSections");

                    entity.Property(e => e.SicSectionId)
                        .HasMaxLength(1)
                        .ValueGeneratedNever();

                    entity.Property(e => e.Description)
                        .IsRequired()
                        .HasMaxLength(250);
                });

            #endregion

            #region UserOrganisation

            modelBuilder.Entity<UserOrganisation>(
                entity => {
                    entity.HasKey(e => new {e.UserId, e.OrganisationId}).HasName("PK_dbo.UserOrganisations");

                    entity.HasIndex(e => e.AddressId)
                        .HasName("IX_AddressId");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.UserId)
                        .HasName("IX_UserId");

                    entity.Property(e => e.Method).HasColumnName("MethodId").HasDefaultValueSql("((0))");

                    entity.HasOne(d => d.Address)
                        .WithMany(p => p.UserOrganisations)
                        .HasForeignKey(d => d.AddressId)
                        .HasConstraintName("FK_dbo.UserOrganisations_dbo.OrganisationAddresses_AddressId");

                    entity.HasOne(d => d.Organisation)
                        .WithMany(p => p.UserOrganisations)
                        .HasForeignKey(d => d.OrganisationId)
                        .HasConstraintName("FK_dbo.UserOrganisations_dbo.Organisations_OrganisationId");

                    entity.HasOne(d => d.User)
                        .WithMany(p => p.UserOrganisations)
                        .HasForeignKey(d => d.UserId)
                        .HasConstraintName("FK_dbo.UserOrganisations_dbo.Users_UserId");
                });

            #endregion

            #region InactiveUserOrganisation

            modelBuilder.Entity<InactiveUserOrganisation>(
                entity => {
                    entity.HasKey(e => new { e.UserId, e.OrganisationId }).HasName("PK_dbo.InactiveUserOrganisations");

                    entity.HasIndex(e => e.AddressId)
                        .HasName("IX_AddressId");

                    entity.HasIndex(e => e.OrganisationId)
                        .HasName("IX_OrganisationId");

                    entity.HasIndex(e => e.UserId)
                        .HasName("IX_UserId");

                    entity.Property(e => e.Method).HasColumnName("MethodId").HasDefaultValueSql("((0))");
                });

            #endregion

            #region User

            modelBuilder.Entity<User>(
                entity => {
                    entity.HasKey(e => e.UserId).HasName("PK_dbo.Users");

                    entity.HasIndex(e => e.ContactEmailAddressDB)
                        .HasName("IX_ContactEmailAddress");

                    entity.HasIndex(e => e.ContactPhoneNumber)
                        .HasName("IX_ContactPhoneNumber");

                    entity.HasIndex(e => e.EmailAddressDB)
                        .HasName("IX_EmailAddress");

                    entity.HasIndex(e => e.Status)
                        .HasName("IX_StatusId");

                    entity.Property(e => e.Status).HasColumnName("StatusId");
                    entity.Property(e => e.ContactEmailAddressDB).HasColumnName("ContactEmailAddress");
                    entity.Property(e => e.EmailAddressDB).HasColumnName("EmailAddress");

                    entity.Property(e => e.ContactEmailAddressDB).HasMaxLength(255);

                    entity.Property(e => e.ContactFirstName).HasMaxLength(50);

                    entity.Property(e => e.ContactJobTitle).HasMaxLength(50);

                    entity.Property(e => e.ContactLastName).HasMaxLength(50);

                    entity.Property(e => e.ContactOrganisation).HasMaxLength(100);

                    entity.Property(e => e.ContactPhoneNumber).HasMaxLength(20);

                    entity.Property(e => e.EmailAddressDB)
                        .IsRequired()
                        .HasMaxLength(255);

                    entity.Property(e => e.EmailVerifyHash).HasMaxLength(250);

                    entity.Property(e => e.Firstname)
                        .IsRequired()
                        .HasMaxLength(50);

                    entity.Property(e => e.JobTitle)
                        .IsRequired()
                        .HasMaxLength(50);

                    entity.Property(e => e.Lastname)
                        .IsRequired()
                        .HasMaxLength(50);

                    entity.Property(e => e.PasswordHash)
                        .IsRequired()
                        .HasMaxLength(250);

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);
                });

            #endregion

            #region UserSetting

            modelBuilder.Entity<UserSetting>(
                entity => {
                    entity.HasKey(e => new {e.UserId, e.Key}).HasName("PK_dbo.UserSettings");

                    entity.HasIndex(e => e.UserId)
                        .HasName("IX_UserId");

                    entity.Property(e => e.Value).HasMaxLength(50);

                    entity.HasOne(d => d.User)
                        .WithMany(p => p.UserSettings)
                        .HasForeignKey(d => d.UserId)
                        .HasConstraintName("FK_dbo.UserSettings_dbo.Users_UserId");
                });

            #endregion

            #region UserStatus

            modelBuilder.Entity<UserStatus>(
                entity => {
                    entity.HasKey(e => e.UserStatusId).HasName("PK_dbo.UserStatus");

                    entity.HasIndex(e => e.ByUserId)
                        .HasName("IX_ByUserId");

                    entity.HasIndex(e => e.StatusDate)
                        .HasName("IX_StatusDate");

                    entity.HasIndex(e => e.UserId)
                        .HasName("IX_UserId");

                    entity.Property(e => e.Status).HasColumnName("StatusId");

                    entity.Property(e => e.StatusDetails).HasMaxLength(255);

                    entity.HasOne(d => d.User)
                        .WithMany(p => p.UserStatuses)
                        .HasForeignKey(d => d.UserId)
                        .HasConstraintName("FK_dbo.UserStatus_dbo.Users_UserId")
                        .OnDelete(DeleteBehavior.Restrict);
                });

            #endregion

            #region PublicSectorType

            modelBuilder.Entity<PublicSectorType>(
                entity => {
                    entity.HasKey(e => e.PublicSectorTypeId).HasName("PK_dbo.PublicSectorTypes");

                    entity.Property(e => e.Description)
                        .HasMaxLength(250)
                        .IsRequired();

                    entity.Property(e => e.Created).IsRequired();
                });

            #endregion

            #region OrganisationPublicSectorType

            modelBuilder.Entity<OrganisationPublicSectorType>(
                entity => {
                    entity.HasKey(e => e.OrganisationPublicSectorTypeId).HasName("PK_dbo.OrganisationPublicSectorTypes");

                    entity.HasIndex(e => e.Created).HasName("IX_Created");

                    entity.HasIndex(e => e.Retired).HasName("IX_Retired");

                    entity.HasIndex(e => e.PublicSectorTypeId).HasName("IX_PublicSectorTypeId");

                    entity.HasIndex(e => e.OrganisationId).HasName("IX_OrganisationId");

                    entity.Property(e => e.Source).HasMaxLength(255);
                });

            #endregion

            #region Feedback

            modelBuilder.Entity<Feedback>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("getdate()");

            #endregion

            #region AuditLog

            modelBuilder.Entity<AuditLog>()
                .HasOne(e => e.OriginalUser);
            modelBuilder.Entity<AuditLog>()
                .HasOne(e => e.ImpersonatedUser);
            modelBuilder.Entity<AuditLog>()
                .HasOne(e => e.Organisation);
            modelBuilder.Entity<AuditLog>()
                .Property(e => e.CreatedDate)
                .HasDefaultValueSql("getdate()");
            modelBuilder.Entity<AuditLog>()
                .Property<string>("DetailsString")
                .HasField("_details");

            #endregion
        }

    }

}
