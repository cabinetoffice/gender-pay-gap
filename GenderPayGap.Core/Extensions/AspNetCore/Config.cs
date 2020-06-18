using System;
using System.Diagnostics;
using System.Linq;
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
            Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

            Configuration = Build();
            VirtualDateTime.Initialise(OffsetCurrentDateTimeForSite());

            Console.WriteLine($"Environment: {EnvironmentName}");
        }

        public static string EnvironmentName
        {
            get
            {
                if (_EnvironmentName == null)
                {
                    _EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                    if (string.IsNullOrWhiteSpace(_EnvironmentName))
                    {
                        _EnvironmentName = Environment.GetEnvironmentVariable("ASPNET_ENV");
                    }

                    if (string.IsNullOrWhiteSpace(_EnvironmentName))
                    {
                        _EnvironmentName = Environment.GetEnvironmentVariable("Hosting:Environment");
                    }

                    if (string.IsNullOrWhiteSpace(_EnvironmentName))
                    {
                        _EnvironmentName = Environment.GetEnvironmentVariable("AzureWebJobsEnv");
                    }

                    if (string.IsNullOrWhiteSpace(_EnvironmentName))
                    {
                        _EnvironmentName = Environment.GetEnvironmentVariable("Environment"); //This is used by webjobs SDK v3 
                    }

                    if (string.IsNullOrWhiteSpace(_EnvironmentName) && Environment.GetEnvironmentVariable("DEV_ENVIRONMENT").ToBoolean())
                    {
                        _EnvironmentName = "Local";
                    }

                    if (string.IsNullOrWhiteSpace(_EnvironmentName))
                    {
                        _EnvironmentName = "Local";
                    }
                }

                return _EnvironmentName;
            }
            set => _EnvironmentName = value;
        }


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
                if (EnvironmentName.EqualsI(environmentName))
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

        public static string GetConnectionString(string key)
        {
            var prefix = "ConnectionStrings:";
            if (key.StartsWithI(prefix))
            {
                key = key.Substring(prefix.Length);
            }

            string value = Configuration.GetConnectionString(key);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = GetAppSetting($"{prefix}{key}");
            }

            return string.IsNullOrWhiteSpace(value) ? null : value;
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
