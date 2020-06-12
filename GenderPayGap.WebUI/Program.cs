using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace GenderPayGap.WebUI
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Console.Title = "GenderPayGap.WebUI";

            //Add a handler for unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            //Culture is required so UK dates can be parsed correctly
            Thread.CurrentThread.CurrentCulture = new CultureInfo(Config.GetAppSetting("Culture").ToStringOr("en-GB"));
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            //Create the web host
            IWebHost host = BuildWebHost(args);

            //Set the minumum threads 
            Console.WriteLine(Extensions.AspNetCore.Extensions.SetThreadCount());

            //Show thread availability
            Console.WriteLine(Extensions.AspNetCore.Extensions.GetThreadCount());

            //Run the webhost
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

        public static IWebHost BuildWebHost(string[] args)
        {
            IWebHostBuilder webHostBuilder = WebHost.CreateDefaultBuilder(args)
                .UseGpgConfiguration(typeof(Startup));

            return webHostBuilder.Build();
        }

    }
}
