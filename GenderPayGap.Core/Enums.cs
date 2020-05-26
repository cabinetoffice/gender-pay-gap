using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace GenderPayGap.Core
{
    public enum UserStatuses : byte
    {

        Unknown = 0,
        New = 1,
        Active = 3,
        Retired = 4

    }

    public enum UserSettingKeys : byte
    {

        Unknown = 0,
        SendUpdates = 1,
        AllowContact = 2,
        PendingFasttrackCodes = 3,
        AcceptedPrivacyStatement = 4

    }

    public enum OrganisationStatuses : byte
    {

        Unknown = 0,
        New = 1,
        Active = 3,
        Retired = 4,
        Pending = 5,
        Deleted = 6

    }

    public enum AddressStatuses : byte
    {

        Unknown = 0,
        New = 1,
        Suspended = 2,
        Active = 3,
        Pending = 5,
        Retired = 6

    }

    public enum RegistrationMethods
    {

        Unknown = 0,
        PinInPost = 1,
        EmailDomain = 2,
        Manual = 3,
        Fasttrack = 4

    }

    public enum ReturnStatuses : byte
    {

        Unknown = 0,
        Draft = 1,
        Suspended = 2,
        Submitted = 3,
        Retired = 4,
        Deleted = 5

    }

    public enum ScopeRowStatuses : byte
    {

        Unknown = 0,
        Active = 3,
        Retired = 4

    }

    public enum SectorTypes
    {

        Unknown = 0,
        Private = 1,
        Public = 2

    }

    public enum ScopeStatuses
    {

        [Display(Name = "In scope")]
        Unknown = 0,

        [Display(Name = "In scope")]
        InScope = 1,

        [Display(Name = "Out of scope")]
        OutOfScope = 2,

        [Display(Name = "In scope")]
        PresumedInScope = 3,

        [Display(Name = "Out of scope")]
        PresumedOutOfScope = 4

    }

    public enum RegisterStatuses
    {

        Unknown = 0,
        RegisterSkipped = 1,
        RegisterPending = 2,
        RegisterComplete = 3,
        RegisterCancelled = 4

    }



    public enum OrganisationSizes
    {

        [Display(Name = "Not Provided")]
        [Range(0, 0)]
        NotProvided = 0,

        [Display(Name = "Less than 250")]
        [Range(0, 249)]
        Employees0To249 = 1,

        [Display(Name = "250 to 499")]
        [Range(250, 499)]
        Employees250To499 = 2,

        [Display(Name = "500 to 999")]
        [Range(500, 999)]
        Employees500To999 = 3,

        [Display(Name = "1000 to 4999")]
        [Range(1000, 4999)]
        Employees1000To4999 = 4,

        [Display(Name = "5000 to 19,999")]
        [Range(5000, 19999)]
        Employees5000to19999 = 5,

        [Display(Name = "20,000 or more")]
        [Range(20000, int.MaxValue)]
        Employees20000OrMore = 6

    }

    public enum SearchType
    {

        ByEmployerName = 1,
        BySectorType = 2,
        NotSet = 99

    }

    public enum SearchReportingStatusFilter
    {

        [Display(Name = "Reported in the last 7 days")]
        ReportedInTheLast7Days,

        [Display(Name = "Reported in the last 30 days")]
        ReportedInTheLast30Days,

        [Display(Name = "Reported late")]
        ReportedLate,

        [Display(Name = "Reported with extra information")]
        ExplanationProvidedByEmployer

    }

    public enum AuditedAction
    {
        [Display(Name = "Admin changed late flag")]
        AdminChangeLateFlag = 0,
        [Display(Name = "Admin changed organisation scope")]
        AdminChangeOrganisationScope = 1,
        [Display(Name = "Admin changed companies house opting")]
        AdminChangeCompaniesHouseOpting = 2,
        [Display(Name = "Admin changed organisation name")]
        AdminChangeOrganisationName = 3,
        [Display(Name = "Admin changed organisation address")]
        AdminChangeOrganisationAddress = 4,
        [Display(Name = "Admin changed organisation SIC code")]
        AdminChangeOrganisationSicCode = 5,
        [Display(Name = "Admin changed user contact preferences")]
        AdminChangeUserContactPreferences = 6,
        [Display(Name = "Admin re-sent verification email")]
        AdminResendVerificationEmail = 7,
        [Display(Name = "Admin changed organisation public sector classification")]
        AdminChangeOrganisationPublicSectorClassification = 8,
        [Display(Name = "User changed their email address")]
        UserChangeEmailAddress = 9,
        [Display(Name = "User changed their password")]
        UserChangePassword = 10,
        [Display(Name = "User changed their name")]
        UserChangeName = 11,
        [Display(Name = "User changed their job title")]
        UserChangeJobTitle = 12,
        [Display(Name = "User changed their phone number")]
        UserChangePhoneNumber = 13,
        [Display(Name = "User changed their contact preferences")]
        UserChangeContactPreferences = 14,
        [Display(Name = "User retired their account")]
        UserRetiredThemselves = 15,
        [Display(Name = "Admin removed a user from an organisation")]
        AdminRemoveUserFromOrganisation = 16,
        [Display(Name = "Admin changed organisation status")]
        AdminChangeOrganisationStatus = 17,
        [Display(Name = "Admin deleted return")]
        AdminDeleteReturn = 18,

        // Note:
        //   Lots of these Execute Manual Change enum values are no longer referenced in code
        //   However, we might have Audit Log entries in the database that reference these values
        //   So, we want to keep these values, so that they show up correctly in the Audit Log
        //   i.e. it's not possible to perform these actions NOW
        //        but that doesn't stop us having a record of them being performed in the past
        [Display(Name = "Execute Manual Change: Convert public to private")]
        ExecuteManualChangeConvertPublicToPrivate = 19,
        [Display(Name = "Execute Manual Change: Convert private to public")]
        ExecuteManualChangeConvertPrivateToPublic = 20,
        [Display(Name = "Execute Manual Change: Convert sector: set accounting date")]
        ExecuteManualChangeConvertSectorSetAccountingDate = 21,
        [Display(Name = "Execute Manual Change: Delete submissions")]
        /* See note above */ ExecuteManualChangeDeleteSubmissions = 22,
        [Display(Name = "Execute Manual Change: Add organisations latest name")]
        /* See note above */ ExecuteManualChangeAddOrganisationsLatestName = 23,
        [Display(Name = "Execute Manual Change: Reset organisation to only original name")]
        /* See note above */ ExecuteManualChangeResetOrganisationToOnlyOriginalName = 24,
        [Display(Name = "Execute Manual Change: Set organisation company number")]
        ExecuteManualChangeSetOrganisationCompanyNumber = 25,
        [Display(Name = "Execute Manual Change: Set organisation SIC codes")]
        ExecuteManualChangeSetOrganisationSicCodes = 26,
        [Display(Name = "Execute Manual Change: Set organisation addresses")]
        /* See note above */ ExecuteManualChangeSetOrganisationAddresses = 27,
        [Display(Name = "Execute Manual Change: Set public sector type")]
        /* See note above */ ExecuteManualChangeSetPublicSectorType = 28,
        [Display(Name = "Execute Manual Change: Set organisation scope")]
        /* See note above */ ExecuteManualChangeSetOrganisationScope = 29,
        [Display(Name = "Execute Manual Change: Create security code")]
        /* See note above */ ExecuteManualChangeCreateOrExtendSecurityCode = 30,
        [Display(Name = "Execute Manual Change: Extend security code")]
        /* See note above */ ExecuteManualChangeExpireSecurityCode = 31,
        [Display(Name = "Execute Manual Change: Create security codes for all active and pending orgs")]
        /* See note above */ ExecuteManualChangeCreateOrExtendSecurityCodesForAllActiveAndPendingOrgs = 32,
        [Display(Name = "Execute Manual Change: Expire security codes for all active and pending orgs")]
        /* See note above */ ExecuteManualChangeExpireSecurityCodesForAllActiveAndPendingOrgs = 33,

        [Display(Name = "Purge organisation")]
        PurgeOrganisation = 34,
        [Display(Name = "Purge registration")]
        PurgeRegistration = 35,
        [Display(Name = "Purge user")]
        PurgeUser = 36,
        [Display(Name = "Registraion log")]
        RegistrationLog = 37,

        [Display(Name = "Admin deleted organisation previous name")]
        AdminDeleteOrganisationPreviousName = 38
    }

    public enum HashingAlgorithm
    {
        Unknown = 0,
        SHA512 = 1,
        PBKDF2 = 2,
        PBKDF2AppliedToSHA512 = 3
    }

    public enum FeedbackStatus
    {
        New = 0,
        NotSpam = 1,
        Spam = 2
    }

    public static class EnumHelper
    {

        public static string DisplayNameOf(object obj)
        {
            return obj.GetType()
                ?
                .GetMember(obj.ToString())
                ?.First()
                ?
                .GetCustomAttribute<DisplayAttribute>()
                ?.Name;
        }

    }

    public static class Filenames
    {
        public const string SicCodes = "SicCodes.csv";
        public const string SicSections = "SicSections.csv";
        public const string SicSectorSynonyms = "GPG-SicSectorSynonyms.csv";

    }

    public static class CookieNames
    {
        public const string LastCompareQuery = "compare";
    }

}
