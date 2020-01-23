using System;
using System.Collections.Generic;
using System.Threading;
using Autofac.Extensions.DependencyInjection;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;

namespace GenderPayGap.Extensions.AspNetCore
{
    public static partial class Extensions
    {

        /// <summary>
        ///     Build the Web Host
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="startupType"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseGpgConfiguration(this IWebHostBuilder webHostBuilder,
            Type startupType = null,
            string contentRoot = null,
            string webRoot = null)
        {
            webHostBuilder.ConfigureKestrel(
                    options => {
                        options.AddServerHeader = false;
                        //options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                    })
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .UseApplicationInsights(
                    Config.GetAppSetting("ApplicationInsights:InstrumentationKey", Config.GetAppSetting("APPINSIGHTS-INSTRUMENTATIONKEY")))
                .CaptureStartupErrors(true) // Add this line to capture startup errors
                .UseEnvironment(Config.EnvironmentName) //Set the environment name
                .UseSetting(
                    WebHostDefaults.DetailedErrorsKey,
                    "true") //When enabled (or when the Environment is set to Development), the app captures detailed exceptions.
                .ConfigureServices(
                    services => services
                        .AddAutofac()); /// This call allows for ConfigureContainer to be supported in Startup with a strongly-typed ContainerBuilder

            if (!string.IsNullOrWhiteSpace(contentRoot))
            {
                webHostBuilder.UseContentRoot(contentRoot); //Specify the root path of the content
            }

            if (!string.IsNullOrWhiteSpace(webRoot))
            {
                webHostBuilder.UseWebRoot(webRoot); //Specify the root path of the site
            }

            if (startupType != null)
            {
                webHostBuilder.UseStartup(startupType);
            }

            //Add the logging to the web host
            webHostBuilder.ConfigureLogging(
                builder => {
                    //For more info see https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-2.2
                    builder.ClearProviders();
                    builder.AddConfiguration(Config.Configuration.GetSection("Logging"));
                    builder.AddDebug();
                    builder.AddConsole();
                    builder.AddEventSourceLogger(); //Log to windows event log
                    //builder.AddAzureWebAppDiagnostics(); //Log to live azure stream
                    /* Temporarily removed this line and nuget as its still in beta and was causing a downgrade of AppInsights down to 2.8.1
                    To add nuget again see microsoft.extensions.logging.applicationinsights
                    builder.AddApplicationInsights(); 
                    */
                });

            SetupSerilogLogger();
            return webHostBuilder;
        }

        /// <summary>
        ///     Use the Config extension class for Configuration
        /// </summary>
        /// <param name="builderContext"></param>
        /// <param name="configBuilder"></param>
        private static void ConfigureAppConfiguration(WebHostBuilderContext builderContext, IConfigurationBuilder configBuilder)
        {
            Config.EnvironmentName = builderContext.HostingEnvironment.EnvironmentName;
            Console.WriteLine($"Environment: {Config.EnvironmentName}");

            //Build the configuration
            Config.Configuration = Config.Build(builder: configBuilder);
            Encryption.SetDefaultEncryptionKey(Config.GetAppSetting("DefaultEncryptionKey"));

            Console.WriteLine($"AzureStorageConnectionString: {Config.Configuration.GetConnectionString("AzureStorage")}");
            Console.WriteLine($"Authority: {Config.SiteAuthority}");
        }

        public static bool IsValidField(this ModelStateDictionary modelState, string key)
        {
            return modelState[key] == null || modelState[key].ValidationState == ModelValidationState.Valid;
        }

        public static void AddCacheProfiles(this MvcOptions options)
        {
            //Control how controller actions cache content using appsettings.json file.
            var cacheProfileSettings = new CacheProfileSettings();
            Config.Configuration.GetSection("CacheProfileSettings:CacheProfiles").Bind(cacheProfileSettings.CacheProfiles);
            foreach (KeyValuePair<string, CacheProfile> keyValuePair in cacheProfileSettings.CacheProfiles)
            {
                options.CacheProfiles.Add(keyValuePair);
            }
        }

        /// <summary>
        ///     Removes null header or ensures header is set to correct value
        ///     ///
        /// </summary>
        /// <param name="context">The HttpContext to remove the header from</param>
        /// <param name="key">The key of the header name</param>
        /// <param name="value">The value which the header should be - if empty removed the header</param>
        public static void SetResponseHeader(this HttpContext context, string key, string value = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    if (context.Response.Headers.ContainsKey(key))
                    {
                        context.Response.Headers.Remove(key);
                    }
                }
                else if (!context.Response.Headers.ContainsKey(key))
                {
                    context.Response.Headers.Add(key, value);
                }
                else if (context.Response.Headers[key] != value)
                {
                    context.Response.Headers.Remove(key); //This is required as cannot change a key once added
                    context.Response.Headers[key] = value;
                }
            }
            catch (Exception ex)
            {
                if (context.Response.Headers.ContainsKey(key))
                {
                    throw new Exception($"Could not set header '{key}' from value '{context.Response.Headers[key]}' to '{value}' ", ex);
                }

                throw new Exception($"Could not add header '{key}' to value '{value}' ", ex);
            }
        }

        public static string GetThreadCount()
        {
            ThreadPool.GetMinThreads(out int workerMin, out int ioMin);
            ThreadPool.GetMaxThreads(out int workerMax, out int ioMax);
            ThreadPool.GetAvailableThreads(out int workerFree, out int ioFree);
            return
                $"Threads (Worker busy:{workerMax - workerFree:N0} min:{workerMin:N0} max:{workerMax:N0}, I/O busy:{ioMax - ioFree:N0} min:{ioMin:N0} max:{ioMax:N0})";
        }

        public static string SetThreadCount()
        {
            int ioMin = Config.GetAppSetting("MinIOThreads").ToInt32(300);
            int workerMin = Config.GetAppSetting("MinWorkerThreads").ToInt32(300);
            ThreadPool.SetMinThreads(workerMin, ioMin);
            return $"Min Threads Set (Work:{workerMin:N0}, I/O: {ioMin:N0})";
        }

        public static void SetupSerilogLogger()
        {
            Logger log;

            if (Config.IsLocal())
            {
                log = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            }
            else
            {
                log = new LoggerConfiguration().WriteTo
                    .ApplicationInsights(TelemetryConfiguration.Active, TelemetryConverter.Traces)
                    .CreateLogger();
            }

            Log.Logger = log;
            Log.Information("Serilog logger setup complete");
        }

    }
}
