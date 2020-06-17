using System;
using System.Collections.Generic;
using Autofac;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Newtonsoft.Json;

namespace GenderPayGap.Core
{
    public static class Global
    {

        public static IContainer ContainerIoC;


        #region Secrets / connection strings / API keys

        public static string DatabaseConnectionString => Config.GetConnectionString(DatabaseConnectionName);
        public static string AzureStorageConnectionString => Config.GetConnectionString("AzureStorage");
        public static string CompaniesHouseApiKey => Config.GetAppSetting("CompaniesHouseApiKey");
        public static string GovUkNotifyApiKey => Config.GetAppSetting("GovUkNotifyApiKey");
        public static int ObfuscationSeed => Config.GetAppSetting("ObfuscationSeed").ToInt32(127);
        public static string ApplicationInsightsInstrumentationKey => Config.GetAppSetting("ApplicationInsights:InstrumentationKey", Config.GetAppSetting("APPINSIGHTS-INSTRUMENTATIONKEY"));
        public static string DefaultEncryptionKey => Config.GetAppSetting("DefaultEncryptionKey");
        public static string ReportsReaderPassword => Config.GetAppSetting("ReportsReaderPassword", "Password");
        public static string AdminEmails => Config.GetAppSetting("AdminEmails");
        public static VcapServices VcapServices =>
            Config.GetAppSetting("VCAP_SERVICES") != null
                ? JsonConvert.DeserializeObject<VcapServices>(Config.GetAppSetting("VCAP_SERVICES"))
                : null;

        #endregion



        #region Settings that we expect to want to update at short notice

        public static bool MaintenanceMode => Config.GetAppSetting("MaintenanceMode").ToBoolean();
        public static List<int> ReportingStartYearsToExcludeFromLateFlagEnforcement =>
            JsonConvert.DeserializeObject<List<int>>(Config.GetAppSetting("ReportingStartYearsToExcludeFromLateFlagEnforcement"));
        public static DateTime ActionHubSwitchOverDate => Config.GetAppSetting("ActionHubSwitchOverDate").ToDateTime();
        public static string ReminderEmailDays => Config.GetAppSetting("ReminderEmailDays");

        #endregion



        #region Settings that only change per environment
        public static bool SkipSpamProtection
        {
            get => Config.GetAppSetting("TESTING-SkipSpamProtection").ToBoolean();
            set => Config.SetAppSetting("TESTING-SkipSpamProtection", value.ToString());
        }
        public static bool SendGoogleAnalyticsDataToGovUk => Config.GetAppSetting("SendGoogleAnalyticsDataToGovUk").ToBoolean();
        public static int MaxNumCallsCompaniesHouseApiPerFiveMins => Config.GetAppSetting("MaxNumCallsCompaniesHouseApiPerFiveMins").ToInt32(10);
        public static string EhrcIPRange => Config.GetAppSetting("EhrcIPRange");

        // Could be replaced by Config.IsLocal
        public static bool UseDeveloperExceptions => Config.GetAppSetting("UseDeveloperExceptions").ToBoolean();
        #endregion

        #region Settings that change per deployment slot
        public static bool BackgroundJobsEnabled => Config.GetAppSetting("WEBJOBS_STOPPED").ToInt32(0) == 0;
        #endregion

        #region Settings that change per hosting environment (Azure/PaaS)
        public static bool UsePostgresDb => Config.GetAppSetting("UsePostgresDb").ToBoolean();
        #endregion

        #region Settings that change per environment / slot / hosting environment (Azure/PaaS)
        public static bool LogToApplicationInsight => Config.GetAppSetting("LogToApplicationInsight").ToBoolean();
        #endregion



        #region Settings that change rarely, but might be used in lots of places (could be constants)

        public static string GpgReportingEmail => Config.GetAppSetting("GPGReportingEmail");
        public static string DataControllerEmail => Config.GetAppSetting("DataControllerEmail");
        public static string DataProtectionOfficerEmail => Config.GetAppSetting("DataProtectionOfficerEmail");
        public static List<string> GeoDistributionList => Config.GetAppSetting("GEODistributionList").Split(";", StringSplitOptions.RemoveEmptyEntries).ToList<string>();

        // These vary by environment, but the URL doesn't change, only whether we use it or not
        // So, really, these should be settings called something like "UseStartUrl" and "UseDoneUrl"
        public static string StartUrl => Config.GetAppSetting("StartUrl");
        public static string DoneUrl => Config.GetAppSetting("DoneUrl");

        #endregion



        #region Settings that we do not expect to change / we only expect to change alongside a code change

        public static string DatabaseConnectionName => Config.GetAppSetting("DatabaseConnectionName") ?? "GpgDatabase";
        public static int StaticCacheSeconds => Config.GetAppSetting("CacheProfileSettings:StaticDuration").ToInt32(86400);
        public static DateTime PrivacyChangedDate => Config.GetAppSetting("PrivacyChangedDate").ToDateTime();
        public static DateTime PrivateAccountingDate => Config.GetAppSetting("PrivateAccountingDate").ToDateTime();
        public static DateTime PublicAccountingDate => Config.GetAppSetting("PublicAccountingDate").ToDateTime();
        public static int EmailVerificationExpiryDays => Config.GetAppSetting("EmailVerificationExpiryDays").ToInt32(7);
        public static int EmailVerificationMinResendHours => Config.GetAppSetting("EmailVerificationMinResendHours").ToInt32();
        public static int EmployerCodeLength => Config.GetAppSetting("EmployerCodeLength").ToInt32();
        public static int EmployerPageSize => Config.GetAppSetting("EmployerPageSize").ToInt32();
        public static int LevenshteinDistance => Config.GetAppSetting("LevenshteinDistance").ToInt32(5);
        public static int LockoutMinutes => Config.GetAppSetting("LockoutMinutes").ToInt32();
        public static int MaxLoginAttempts => Config.GetAppSetting("MaxLoginAttempts").ToInt32();
        public static int MaxPinAttempts => Config.GetAppSetting("MaxPinAttempts").ToInt32();
        public static int MinPasswordResetMinutes => Config.GetAppSetting("MinPasswordResetMinutes").ToInt32();
        public static int PinInPostExpiryDays => Config.GetAppSetting("PinInPostExpiryDays").ToInt32(14);
        public static DateTime PinExpiresDate => VirtualDateTime.Now.AddDays(0 - PinInPostExpiryDays);
        public static int PinInPostMinRepostDays => Config.GetAppSetting("PinInPostMinRepostDays").ToInt32();
        public static int PINLength => Config.GetAppSetting("PINLength").ToInt32();
        public static int PurgeUnusedOrganisationDays => Config.GetAppSetting("PurgeUnusedOrganisationDays").ToInt32(30);
        public static int PurgeUnconfirmedPinDays => PinInPostExpiryDays + Config.GetAppSetting("PurgeUnconfirmedPinDays").ToInt32(14);
        public static int SecurityCodeExpiryDays => Config.GetAppSetting("SecurityCodeExpiryDays").ToInt32(90);
        public static string DownloadsLocation => Config.GetAppSetting("DownloadsLocation");
        public static string EmployerCodeChars => Config.GetAppSetting("EmployerCodeChars");
        public static string PasswordRegex => Config.GetAppSetting("PasswordRegex");
        public static string PasswordRegexError => Config.GetAppSetting("PasswordRegexError");
        public static string PINChars => Config.GetAppSetting("PINChars");
        public static string PinRegex => Config.GetAppSetting("PinRegex");
        public static string PinRegexError => Config.GetAppSetting("PinRegexError");
        public static string WhoNeedsToReportGuidanceLink => Config.GetAppSetting("WhoNeedsToReportGuidanceLink");
        public static int CurrentAccountingYear => SectorTypes.Private.GetAccountingStartDate().Year;
        public static string CompanyNumberRegexError => Config.GetAppSetting("CompanyNumberRegexError");
        public static string SaveDraftPath => Config.GetAppSetting("SaveDraftPath");
        public static int FirstReportingYear

        {
            get => Config.GetAppSetting("FirstReportingYear").ToInt32(2017);
            set => Config.SetAppSetting("FirstReportingYear", value.ToString());
        }
        public static string GovUkNotifyPinInThePostTemplateId => Config.GetAppSetting("GovUkNotifyPinInThePostTemplateId");
        public static string GoogleAnalyticsAccountId => Config.GetAppSetting("GoogleAnalyticsAccountId");
        public static string Culture => Config.GetAppSetting("Culture");
        public static string CompaniesHouseApiServer => Config.GetAppSetting("CompaniesHouseApiServer");
        public static int CompaniesHouseMaxRecords => Config.GetAppSetting("CompaniesHouseMaxRecords").ToInt32(400);
        public static int MinIOThreads => Config.GetAppSetting("MinIOThreads").ToInt32(300);
        public static int MinWorkerThreads => Config.GetAppSetting("MinWorkerThreads").ToInt32(300);
        public static bool EncryptEmails => Config.GetAppSetting("EncryptEmails").ToBoolean(true);
        public static bool DisablePageCaching => Config.GetAppSetting("DisablePageCaching").ToBoolean();
        public static string DataPath => Config.GetAppSetting("DataPath");
        public static bool EnableSubmitAlerts
        {
            get => Config.GetAppSetting("EnableSubmitAlerts").ToBoolean(true);
            set => Config.SetAppSetting("EnableSubmitAlerts", value.ToString());
        }

        #endregion

    }
}
