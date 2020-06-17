using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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


        public static IConfiguration Build(Dictionary<string, string> additionalSettings = null, IConfigurationBuilder builder = null)
        {
            builder = builder ?? new ConfigurationBuilder();

            builder.AddJsonFile("appsettings.json", false, true);
            builder.AddJsonFile($"appsettings.{EnvironmentName}.json", true, true);

            builder.AddEnvironmentVariables();

            if (additionalSettings != null && additionalSettings.Any())
            {
                builder.AddInMemoryCollection(additionalSettings);
            }

            IConfigurationRoot configuration = builder.Build();

            //Add the azure key vault to configuration
            string vault = configuration["Vault"];
            if (!string.IsNullOrWhiteSpace(vault))
            {
                if (!vault.StartsWithI("http"))
                {
                    vault = $"https://{vault}.vault.azure.net/";
                }

                string clientId = configuration["ClientId"];
                string clientSecret = configuration["ClientSecret"];
                var exceptions = new List<Exception>();
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    exceptions.Add(new ArgumentNullException("ClientId is missing"));
                }

                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    exceptions.Add(new ArgumentNullException("clientSecret is missing"));
                }

                if (exceptions.Count > 0)
                {
                    throw new AggregateException(exceptions);
                }

                builder.AddAzureKeyVault(vault, clientId, clientSecret);
            }

            /* make sure these files are loaded AFTER the vault, so their keys superseed the vaults' values - that way, unit tests will pass because the obfuscation key is whatever the appSettings says it is [and not a hidden secret inside the vault])  */
            if (Debugger.IsAttached || IsEnvironment("Local"))
            {
                Assembly appAssembly = Misc.GetTopAssembly();
                if (appAssembly != null)
                {
                    builder.AddUserSecrets(appAssembly, true);
                }

                builder.AddJsonFile("appsettings.secret.json", true, true);
            }

            builder.AddJsonFile("appsettings.unittests.json", true, false);

            // override using the azure environment variables into the configuration
            builder.AddEnvironmentVariables();
            configuration = builder.Build();
            return configuration;
        }

        public static bool IsLocal()
        {
            return IsEnvironment("LOCAL");
        }

        public static bool IsDevelopment()
        {
            return IsEnvironment("DEV", "DEVELOPMENT");
        }

        public static bool IsStaging()
        {
            return IsEnvironment("STAGING");
        }

        public static bool IsPreProduction()
        {
            return IsEnvironment("PREPROD", "PREPRODUCTION");
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

        public static IEnumerable<string> GetAppSettingKeys()
        {
            return GetAppSettings().GetKeys();
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

        public static IEnumerable<string> GetKeys(this IConfiguration section)
        {
            return section.GetChildren().Select(c => c.Key);
        }

        public static bool HasKey(this IConfiguration section, string key)
        {
            return section.GetChildren().Any(c => c.Key.EqualsI(key));
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
