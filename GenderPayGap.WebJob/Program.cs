using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Autofac;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see https://go.microsoft.com/fwlink/?LinkID=320976
    public class Program
    {

        public static IContainer ContainerIOC;

        private static void Main(string[] args)
        {
            Console.Title = "GenderPayGap.WebJobs";

            //Add a handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Culture is required so UK dates can be parsed correctly
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Config.GetAppSetting("Culture").ToStringOr("en-GB"));
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            if (!Config.IsProduction() && Config.GetAppSetting("DUMP_APPSETTINGS") == "1")
            {
                foreach (string key in Config.GetAppSettingKeys())
                {
                    Console.WriteLine($@"APPSETTING[""{key}""]={Config.GetAppSetting(key)}");
                }
            }

            //Build the webjob host and services
            IHost host = BuildJobHost(args);
            
            //Initialise the global file and search repositories
            Global.FileRepository = ContainerIOC.Resolve<IFileRepository>();
            Global.SearchRepository = ContainerIOC.Resolve<ISearchRepository<EmployerSearchModel>>();
            Global.SicCodeSearchRepository = ContainerIOC.Resolve<ISearchRepository<SicCodeSearchModel>>();
            
            //Leave this check here to ensure function dependencies resolve on startup rather than when each function method is invoked
            var functions = ContainerIOC.Resolve<Functions>();
            
            //Show thread availability
            Console.WriteLine(Extensions.AspNetCore.Extensions.GetThreadCount());

            host.Run();
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

        public static IHost BuildJobHost(string[] args)
        {
            IHostBuilder jobHostBuilder = new HostBuilder()
                .UseEnvironment(Config.EnvironmentName)
                .ConfigureServices(Startup.ConfigureServices);

            jobHostBuilder.ConfigureLogging(
                (context, builder) => {
                    builder.ClearProviders();
                    builder.AddConfiguration(Config.Configuration.GetSection("Logging"));
                    // If this key exists in any config, use it to enable App Insights
                    if (!string.IsNullOrEmpty(Global.APPINSIGHTS_INSTRUMENTATIONKEY))
                    {
                        builder.AddApplicationInsights(o => o.InstrumentationKey = Global.APPINSIGHTS_INSTRUMENTATIONKEY);
                    }

                    builder.AddDebug();
                    builder.AddConsole();
                    builder.AddEventSourceLogger(); //Log to windows event log
                });

            Extensions.AspNetCore.Extensions.SetupSerilogLogger();

            var settings = new Dictionary<string, string>();
            settings["ConnectionStrings:AzureWebJobsStorage"] = Global.AzureStorageConnectionString;

            jobHostBuilder.ConfigureAppConfiguration(b => { b.AddInMemoryCollection(settings); });

            jobHostBuilder.ConfigureWebJobs(
                builder => {
                    builder.AddAzureStorageCoreServices();
                    builder.AddAzureStorage(
                        queueConfig => {
                            queueConfig.BatchSize = 1; //Process queue messages 1 item per time per job function
                        },
                        blobConfig => {
                            //Configure blobs here
                        });
                    builder.AddServiceBus();
                    builder.AddEventHubs();
                    builder.AddTimers();
                });

            return jobHostBuilder
                .UseConsoleLifetime()
                .Build();
        }

    }

    public class AutofacJobActivator : IJobActivator
    {

        private readonly IContainer _container;

        public AutofacJobActivator(IContainer container)
        {
            _container = container;
        }

        public T CreateInstance<T>()
        {
            return _container.Resolve<T>();
        }

    }
}
