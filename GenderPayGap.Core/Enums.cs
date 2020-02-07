using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core
{
    public enum UserStatuses : byte
    {

        Unknown = 0,
        New = 1,
        Suspended = 2,
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
        Suspended = 2,
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

    public enum ManualActions : byte
    {

        Unknown = 0,
        Create = 1,
        Read = 2,
        Update = 3,
        Delete = 4,
        Extend = 5,
        Expire = 6

    }

    public enum UserAction : byte
    {

        CreatedAccount = 0,
        ChangedEmail = 1,
        ChangedPassword = 2,
        ChangedDetails = 3,
        Retired = 4

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
        [Display(Name = "Change late flag")]
        AdminChangeLateFlag = 0,
        [Display(Name = "Change organisation scope")]
        AdminChangeOrganisationScope = 1,
        [Display(Name = "Change companies house opting")]
        AdminChangeCompaniesHouseOpting = 2,
        [Display(Name = "Change organisation name")]
        AdminChangeOrganisationName = 3,
        [Display(Name = "Change organisation address")]
        AdminChangeOrganisationAddress = 4,
        [Display(Name = "Change organisation SIC code")]
        AdminChangeOrganisationSicCode = 5,
        [Display(Name = "Change user contact preferences")]
        AdminChangeUserContactPreferences = 6,
        [Display(Name = "Re-send verification email")]
        AdminResendVerificationEmail = 7,
        [Display(Name = "Change organisation public sector classification")]
        AdminChangeOrganisationPublicSectorClassification = 8,
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

        public const string Organisations = "GPG-Organisations.csv";
        public const string Users = "GPG-Users.csv";
        public const string Registrations = "GPG-Registrations.csv";
        public const string RegistrationAddresses = "GPG-RegistrationAddresses.csv";
        public const string UnverifiedRegistrations = "GPG-UnverifiedRegistrations.csv";
        public const string AllowFeedback = "GPG-UsersToContactForFeedback.csv";
        public const string UnfinishedOrganisations = "GPG-UnfinishedOrgs.csv";
        public const string OrphanOrganisations = "GPG-OrphanOrganisations.csv";
        public const string OrganisationScopes = "GPG-Scopes.csv";
        public const string OrganisationSubmissions = "GPG-Submissions.csv";
        public const string OrganisationLateSubmissions = "GPG-LateSubmissions.csv";
        public const string ShortCodes = "GPG-ShortCodes.csv";
        public const string SicCodes = "SicCodes.csv";
        public const string SicSections = "SicSections.csv";
        public const string SicSectorSynonyms = "GPG-SicSectorSynonyms.csv";

        // Record logs
        public const string BadSicLog = "BadSicLog.csv";
        public const string ManualChangeLog = "ManualChangeLog.csv";
        public const string RegistrationLog = "RegistrationLog.csv";
        public const string SubmissionLog = "SubmissionLog.csv";
        public const string EmailSendLog = "EmailSendLog.csv";
        public const string StannpSendLog = "StannpSendLog.csv";
        public const string SearchLog = "searchLog.csv";
        public const string UserLog = "UserLog.csv";
        
        public static string GetRootFilename(string filePath)
        {
            string path = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);
            string prefix = fileName.BeforeFirst("_");
            return $"{prefix}{extension}";
        }

    }

    public static class QueueNames
    {

        public const string ExecuteWebJob = "execute-webjob";
        public const string SendEmail = "send-email";
        public const string SendNotifyEmail = "send-notify-email";
        public const string LogEvent = "log-event";
        public const string LogRecord = "log-record";

    }

    public static class CookieNames
    {

        public const string LastCompareQuery = "compare";

    }

}
