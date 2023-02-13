using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Autofac.Extensions.DependencyInjection;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Sentry;

namespace GenderPayGap.WebUI
{
    public class Program
    {

        public static void Main(string[] args)
        {


            // Add sentry logging
            using (SentrySdk.Init(o =>
                {
                    o.Dsn = "https://7aa7b734d6b24c549432c396a5c42465@o4504651047501824.ingest.sentry.io/4504672991641600";
                    // When configuring for the first time, to see what the SDK is doing:
                    o.Debug = true;
                    // Set traces_sample_rate to 1.0 to capture 100% of transactions for performance monitoring.
                    // We recommend adjusting this value in production.
                    o.TracesSampleRate = 1.0;
                    // Enable Global Mode if running in a client app
                    o.IsGlobalModeEnabled = true;
                }))
            {
                // App code goes here. Dispose the SDK before exiting to flush events.
                Console.Title = "GenderPayGap.WebUI";

                // Add a handler for unhandled exceptions
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                // Culture is required so UK dates can be parsed correctly
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

                // Create the web host
                IWebHost host = BuildWebHost(args);

                // Set the minumum threads 
                Console.WriteLine(Extensions.AspNetCore.Extensions.SetThreadCount());

                // Show thread availability
                Console.WriteLine(Extensions.AspNetCore.Extensions.GetThreadCount());

                // Run the webhost
                host.Run();
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;

            Console.WriteLine($"UNHANDLED EXCEPTION ({Console.Title}): {ex.Message}{Environment.NewLine}{ex.GetDetailsText()}");
            Debug.WriteLine($"UNHANDLED EXCEPTION ({Console.Title}): {ex.Message}{Environment.NewLine}{ex.GetDetailsText()}");

            //Show thread availability
            Console.WriteLine(Extensions.AspNetCore.Extensions.GetThreadCount());

            throw ex;
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder(args);
            webHostBuilder.ConfigureKestrel(
                    options =>
                    {
                        options.AddServerHeader = false;
                        //options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
                    })
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .CaptureStartupErrors(true) // Add this line to capture startup errors
                .UseEnvironment(Config.EnvironmentName) //Set the environment name
                .UseSetting(
                    WebHostDefaults.DetailedErrorsKey,
                    "true") //When enabled (or when the Environment is set to Development), the app captures detailed exceptions.
                .ConfigureServices(
                    services => services
                        .AddAutofac()); /// This call allows for ConfigureContainer to be supported in Startup with a strongly-typed ContainerBuilder
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("PORT")))
            {
                webHostBuilder.UseUrls($"http://0.0.0.0:{Environment.GetEnvironmentVariable("PORT")}/");
            }

            webHostBuilder.UseStartup<Startup>();

            SetupSerilogLogger(webHostBuilder);

            return webHostBuilder.Build();
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
            Config.Configuration = Config.Build(configBuilder);
            Encryption.SetDefaultEncryptionKey(Global.DefaultEncryptionKey);
        }

        public static void SetupSerilogLogger(IWebHostBuilder webHostBuilder)
        {
           
            // Log to Console
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
           
            Log.Information("Serilog logger setup complete");
        }

    }
}
