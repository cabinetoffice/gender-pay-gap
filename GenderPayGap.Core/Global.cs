using GenderPayGap.Extensions.AspNetCore;
using Newtonsoft.Json;

namespace GenderPayGap.Core
{
    public static class Global
    {

        #region Secrets / connection strings / API keys

        public static string DatabaseConnectionString =>
            $"Server={Config.GetAppSetting("DATABASE_HOST")};"
            + $"Port={Config.GetAppSetting("DATABASE_PORT")};"
            + $"Database={Config.GetAppSetting("DATABASE_DB_NAME")};"
            + $"User Id={Config.GetAppSetting("DATABASE_USERNAME")};"
            + $"Password={Config.GetAppSetting("DATABASE_PASSWORD")};"
            + (Config.IsLocal()
                ? ""
                : "SslMode=Require;Trust Server Certificate=true");

        public static string S3BucketName => Config.GetAppSetting("S3_BUCKET_NAME");
        public static string S3BucketAwsAccessKeyId => Config.GetAppSetting("AWS_ACCESS_KEY_ID");
        public static string S3BucketAwsSecretAccessKey => Config.GetAppSetting("AWS_SECRET_ACCESS_KEY");
        public static string S3BucketAwsRegion => Config.GetAppSetting("AWS_DEFAULT_REGION");

        public static string CompaniesHouseApiKey => Config.GetAppSetting("CompaniesHouseApiKey");
        public static string GovUkNotifyApiKey => Config.GetAppSetting("GovUkNotifyApiKey");
        public static string DefaultEncryptionKey => Config.GetAppSetting("DefaultEncryptionKey");
        public static string DefaultEncryptionIv => Config.GetAppSetting("DefaultEncryptionIv");
        public static string DataMigrationPassword => Config.GetAppSetting("DataMigrationPassword");
        public static string BasicAuthUsername => Config.GetAppSetting("BasicAuthUsername");
        public static string BasicAuthPassword => Config.GetAppSetting("BasicAuthPassword");
        public static string EhrcApiToken => Config.GetAppSetting("EhrcApiToken");

        #endregion



        #region Settings that we expect to want to update at short notice

        public static TimeSpan OffsetCurrentDateTimeForSite => TimeSpan.Parse(Config.GetAppSetting("OffsetCurrentDateTimeForSite", "0"));
        public static bool MaintenanceMode => Config.GetAppSettingBool("MaintenanceMode", defaultValue: false);
        public static DateTime? MaintenanceModeUpAgainTime => Config.GetAppSettingDateTime("MaintenanceModeUpAgainTime");
        public static List<int> ReportingStartYearsToExcludeFromLateFlagEnforcement =>
            LoadListOfIntegers("ReportingStartYearsToExcludeFromLateFlagEnforcement");
        public static List<int> ReportingStartYearsWithFurloughScheme => 
            LoadListOfIntegers("ReportingStartYearsWithFurloughScheme");
        public static string ReminderEmailDays => Config.GetAppSetting("ReminderEmailDays");
        public static bool EnableSubmitAlerts
        {
            get => Config.GetAppSettingBool("EnableSubmitAlerts", defaultValue: false);
            set => Config.SetAppSetting("EnableSubmitAlerts", value.ToString());
        }
        public static bool DisableSearchCache => Config.GetAppSettingBool("DisableSearchCache");

        #endregion



        #region Settings that only change per environment
        public static bool SendGoogleAnalyticsDataToGovUk => Config.GetAppSettingBool("SendGoogleAnalyticsDataToGovUk");
        public static int MaxNumCallsCompaniesHouseApiPerFiveMins => Config.GetAppSettingInt("MaxNumCallsCompaniesHouseApiPerFiveMins", defaultValue: 10);

        public static string GoogleAnalyticsAccountId => Config.GetAppSetting("GoogleAnalyticsAccountId");
        public static List<string> GeoDistributionList => Config.GetAppSetting("GEODistributionList").Split(";", StringSplitOptions.RemoveEmptyEntries).ToList();
        public static TimeSpan TimeToKeepBackupFiles => TimeSpan.FromDays(Config.GetAppSettingInt("DaysToKeepBackupFiles", defaultValue: 35));
        public static bool LogToSentry => Config.GetAppSettingBool("LogToSentry", defaultValue: false);
        #endregion

        #region Settings that change per deployment slot
        public static bool BackgroundJobsEnabled => Config.GetAppSettingInt("WEBJOBS_STOPPED", defaultValue: 0) == 0;
        #endregion



        #region Settings that change rarely / we only expect to change alongside a code change, but might be used in lots of places (could be constants)

        public static string GpgReportingEmail => "gpg.reporting@cabinetoffice.gov.uk";
        public static string DataControllerEmail => "publiccorrespondence@cabinetoffice.gov.uk";
        public static string DataProtectionOfficerEmail => "dpo@cabinetoffice.gov.uk";
        public static string InformationCommissionerEmail => "casework@ico.org.uk";
        public static string TestEnvironmentEmailRecipient => "gpg-service-testing@cabinetoffice.gov.uk";
        public static int StaticCacheSeconds => 86400;
        public static DateTime PrivacyChangedDate => DateTime.Parse("2024-11-25");
        public static DateTime PrivateAccountingDate => DateTime.Parse("2017-04-05");
        public static DateTime PublicAccountingDate => DateTime.Parse("2017-03-31");
        public static int EmailVerificationExpiryDays => 7;
        public static int EmailVerificationMinResendHours => 1;
        public static int LockoutMinutes => 30;
        public static int MaxAuthAttempts => 5;
        public static int MaxPinAttempts => 3;
        public static int MinPasswordResetMinutes => 10;
        public static TimeSpan PasswordResetCodeExpiryDays => TimeSpan.FromDays(1);
        public static int PinInPostExpiryDays => 21;
        public static int PurgeUnusedOrganisationDays => 30;
        public static int PurgeUnconfirmedPinDays => PinInPostExpiryDays + 30;
        public static string DownloadsLocation => "Downloads";
        public static string PINChars => "123456789ABCDEF";
        public static string WhoNeedsToReportGuidanceLink => "https://www.gov.uk/government/publications/gender-pay-gap-reporting-guidance-for-employers/who-needs-to-report";
        public static int FirstReportingYear => 2017;
        public static int MinIOThreads => 300;
        public static int MinWorkerThreads => 300;
        public static string StartUrl =>
            Config.GetAppSettingBool("UseStartUrl")
                ? "https://www.gov.uk/report-gender-pay-gap-data"
                : null;
        public static string DoneUrl => "https://www.gov.uk/done/report-gender-pay-gap-data";
        public static int MaxCompareBasketCount => 500; // Maximum number of employers you can add to the compare basket
        public static int EditableReportCount => 4; // Specifies how many reports an employer can edit
        public static int EditableScopeCount => 2; // Specifies how many scopes an employer can edit
        public static Dictionary<string, string> SecurityHeadersToAdd =>
            new Dictionary<string, string>
            {
                {"X-Content-Type-Options", "nosniff"},
                {"X-Frame-Options", "DENY"},
                {"X-Permitted-Cross-Domain-Policies", "master-only"},
                {"X-Xss-Protection", "1; mode=block;"},
                {"Content-Security-Policy", "frame-ancestors 'none'"},
                {"X-Content-Security-Policy", "frame-ancestors 'none'"},
                {"Referrer-Policy", "origin-when-cross-origin"},
                {"Strict-Transport-Security", "max-age=31536000; includeSubDomains"},
            };
        public static List<string> SecurityHeadersToRemove =>
            new List<string>
            {
                "X-Powered-By",
                "X-AspNet-Version",
                "X-AspNetMvc-Version",
                "Server"
            };

        public static int ObfuscationSeed => 1045659205;

        #endregion
        
        #region Private

        private static List<int> LoadListOfIntegers(string setting)
        {
            return JsonConvert.DeserializeObject<List<int>>(Config.GetAppSetting(setting, "[]"));
        }

        #endregion
    }
}
