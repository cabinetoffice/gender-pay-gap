﻿using System;
using System.Collections.Generic;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Newtonsoft.Json;

namespace GenderPayGap.Core
{
    public class Global
    {

        public static IFileRepository FileRepository;

        public static bool SkipSpamProtection
        {
            get => Config.GetAppSetting("TESTING-SkipSpamProtection").ToBoolean();
            set => Config.SetAppSetting("TESTING-SkipSpamProtection", value.ToString());
        }

        public static int CertExpiresWarningDays => Config.GetAppSetting("CertExpiresWarningDays").ToInt32(30);

        public static string TrustedIPDomains
        {
            get => Config.GetAppSetting("TrustedIPDomains");
            set => Config.SetAppSetting("TrustedIPDomains", value);
        }

        public static bool UseDeveloperExceptions => Config.GetAppSetting("UseDeveloperExceptions").ToBoolean();
        public static string StartUrl => Config.GetAppSetting("StartUrl");
        public static string DoneUrl => Config.GetAppSetting("DoneUrl");

        public static bool EnableSubmitAlerts
        {
            get => Config.GetAppSetting("EnableSubmitAlerts").ToBoolean(true);
            set => Config.SetAppSetting("EnableSubmitAlerts", value.ToString());
        }

        public static bool MaintenanceMode => Config.GetAppSetting("MaintenanceMode").ToBoolean();
        public static int StaticCacheSeconds => Config.GetAppSetting("CacheProfileSettings:StaticDuration").ToInt32(86400);
        public static DateTime PrivacyChangedDate => Config.GetAppSetting("PrivacyChangedDate").ToDateTime();
        public static DateTime PrivateAccountingDate => Config.GetAppSetting("PrivateAccountingDate").ToDateTime();
        public static DateTime PublicAccountingDate => Config.GetAppSetting("PublicAccountingDate").ToDateTime();
        public static int EmailVerificationExpiryDays => Config.GetAppSetting("EmailVerificationExpiryDays").ToInt32(7);
        public static int EmailVerificationMinResendHours => Config.GetAppSetting("EmailVerificationMinResendHours").ToInt32();
        public static int EmployerCodeLength => Config.GetAppSetting("EmployerCodeLength").ToInt32();
        public static int EmployerPageSize => Config.GetAppSetting("EmployerPageSize").ToInt32();
        public static string ExternalHost => Config.GetAppSetting("EXTERNAL_HOST");
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
        public static bool DisablePageCaching => Config.GetAppSetting("DisablePageCaching").ToBoolean();
        public static string AdminEmails => Config.GetAppSetting("AdminEmails");

        public static string DataPath => Config.GetAppSetting("DataPath");

        public static string DownloadsLocation

        {
            get => Config.GetAppSetting("DownloadsLocation");
            set => Config.SetAppSetting("DownloadsLocation", value);
        }

        public static string EmployerCodeChars => Config.GetAppSetting("EmployerCodeChars");
        public static string PasswordRegex => Config.GetAppSetting("PasswordRegex");
        public static string PasswordRegexError => Config.GetAppSetting("PasswordRegexError");
        public static string PINChars => Config.GetAppSetting("PINChars");
        public static string PinRegex => Config.GetAppSetting("PinRegex");
        public static string PinRegexError => Config.GetAppSetting("PinRegexError");
        public static string SuperAdminEmails => Config.GetAppSetting("SuperAdminEmails");
        public static string DatabaseAdminEmails => Config.GetAppSetting("DatabaseAdminEmails");
        public static string TestPrefix => Config.GetAppSetting("TestPrefix");
        public static string WhoNeedsToReportGuidanceLink => Config.GetAppSetting("WhoNeedsToReportGuidanceLink");
        public static int CurrentAccountingYear => SectorTypes.Private.GetAccountingStartDate().Year;

        public static string CompanyNumberRegexError => Config.GetAppSetting("CompanyNumberRegexError");
        public static Version Version => Misc.GetTopAssembly().GetName().Version;
        public static DateTime AssemblyDate => Misc.GetTopAssembly().GetAssemblyCreationTime();
        public static string AssemblyCopyright => Misc.GetTopAssembly().GetAssemblyCopyright();
        public static string DatabaseConnectionName => Config.GetAppSetting("DatabaseConnectionName") ?? "GpgDatabase";
        public static string DatabaseConnectionString => Config.GetConnectionString(DatabaseConnectionName);
        public static string AzureStorageConnectionString => Config.GetConnectionString("AzureStorage");

        public static string APPINSIGHTS_INSTRUMENTATIONKEY =>
            Config.GetAppSetting("ApplicationInsights:InstrumentationKey", Config.GetAppSetting("APPINSIGHTS-INSTRUMENTATIONKEY"));

        public static string SaveDraftPath => Config.GetAppSetting("SaveDraftPath");

        public static int FirstReportingYear

        {
            get => Config.GetAppSetting("FirstReportingYear").ToInt32(2017);
            set => Config.SetAppSetting("FirstReportingYear", value.ToString());
        }

        public static List<int> ReportingStartYearsToExcludeFromLateFlagEnforcement =>
            JsonConvert.DeserializeObject<List<int>>(Config.GetAppSetting("ReportingStartYearsToExcludeFromLateFlagEnforcement"));

        public static string GpgReportingEmail => Config.GetAppSetting("GPGReportingEmail");
        public static string DataControllerEmail => Config.GetAppSetting("DataControllerEmail");
        public static string DataProtectionOfficerEmail => Config.GetAppSetting("DataProtectionOfficerEmail");

        public static string GovUkNotifyPinInThePostTemplateId => Config.GetAppSetting("GovUkNotifyPinInThePostTemplateId");

        public static DateTime ActionHubSwitchOverDate => Config.GetAppSetting("ActionHubSwitchOverDate").ToDateTime();

        public static bool SendGoogleAnalyticsDataToGovUk => Config.GetAppSetting("SendGoogleAnalyticsDataToGovUk").ToBoolean();

        public static string AzureInstanceId => Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");

        public static List<string> GeoDistributionList => Config.GetAppSetting("GEODistributionList").Split(";", StringSplitOptions.RemoveEmptyEntries).ToList<string>();

        public static bool UsePostgresDb => Config.GetAppSetting("UsePostgresDb").ToBoolean();
        
    }
}
