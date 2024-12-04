using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace GenderPayGap.Extensions.AspNetCore
{
    public static class Config
    {

        private static string _EnvironmentName;

        public static IConfiguration Configuration;

        private static TimeSpan? SingletonOffsetCurrentDateTimeForSite;

        static Config()
        {
            Console.WriteLine($"Environment: {EnvironmentName}");

            Configuration = Build();
            VirtualDateTime.Initialise(OffsetCurrentDateTimeForSite());
        }

        public static string EnvironmentName => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        public static IConfiguration Build(IConfigurationBuilder builder = null)
        {
            builder = builder ?? new ConfigurationBuilder();

            // NOTE: The order in which these commands run determines which settings take priority

            // First, load from appsettings.json
            builder.AddJsonFile("appsettings.json", false, true);

            // Then, override this with the environment-specific settings
            builder.AddJsonFile($"appsettings.{EnvironmentName}.json", true, true);

            // If we're running the code locally, override this with appsettings.secret.json
            if (Debugger.IsAttached || IsEnvironment("Local"))
            {
                builder.AddJsonFile("appsettings.secret.json", true, true);
            }

            // Then add the unit test configuration (only used when running automated tests)
            builder.AddJsonFile("appsettings.unittests.json", true, false);

            // Environment variables are added last (so have highest priority)
            builder.AddEnvironmentVariables();

            return builder.Build();
        }

        public static bool IsLocal()
        {
            return IsEnvironment("LOCAL");
        }

        public static bool IsProduction()
        {
            return IsEnvironment("PROD", "PRODUCTION");
        }

        public static bool IsEnvironment(params string[] environmentNames)
        {
            foreach (string environmentName in environmentNames)
            {
                if (string.Equals(EnvironmentName, environmentName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static string GetAppSetting(string key, string defaultValue = null)
        {
            IConfiguration appSettings = GetAppSettings();
            string value = appSettings[key];

            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }

        public static bool GetAppSettingBool(string key, bool defaultValue = false)
        {
            string settingValue = GetAppSetting(key);

            if (bool.TryParse(settingValue, out bool settingValueBool))
            {
                return settingValueBool;
            }

            return defaultValue;
        }

        public static int GetAppSettingInt(string key, int defaultValue = 0)
        {
            string settingValue = GetAppSetting(key);

            if (int.TryParse(settingValue, out int settingValueInt))
            {
                return settingValueInt;
            }

            return defaultValue;
        }

        public static DateTime GetAppSettingDateTime(string key)
        {
            string settingValue = GetAppSetting(key);

            if (DateTime.TryParseExact(settingValue, "yyMMddHHmmss", null, DateTimeStyles.AssumeLocal, out DateTime parsedValueShortFormat))
            {
                return parsedValueShortFormat;
            }

            if (DateTime.TryParse(settingValue, out DateTime parsedValueOtherFormat))
            {
                return parsedValueOtherFormat;
            }

            return DateTime.MinValue;
        }

        private static IConfiguration GetAppSettings()
        {
            IConfiguration appSettings = Configuration.GetSection("AppSettings");
            if (!appSettings.GetChildren().Any())
            {
                appSettings = Configuration;
            }

            return appSettings;
        }

        public static void SetAppSetting(string key, string value)
        {
            IConfiguration appSettings = GetAppSettings();
            appSettings[key] = value;
        }

        public static TimeSpan OffsetCurrentDateTimeForSite()
        {
            if (SingletonOffsetCurrentDateTimeForSite == null)
            {
                SingletonOffsetCurrentDateTimeForSite = IsProduction()
                    ? TimeSpan.Zero
                    : TimeSpan.Parse(GetAppSetting("OffsetCurrentDateTimeForSite", "0"));
            }

            return (TimeSpan) SingletonOffsetCurrentDateTimeForSite;
        }

    }
}
