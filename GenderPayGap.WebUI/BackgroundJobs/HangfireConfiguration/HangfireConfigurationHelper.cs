using Autofac;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GenderPayGap.WebUI.BackgroundJobs.HangfireConfiguration
{
    public static class HangfireConfigurationHelper
    {

        public static void ConfigureServices(IServiceCollection services)
        {
            if (Global.UsePostgresDb)
            {
                services.AddHangfire(x => x.UsePostgreSqlStorage(Global.DatabaseConnectionString));
            }
            else
            {
                services.AddHangfire(x => x.UseSqlServerStorage(Global.DatabaseConnectionString));
            }

            if (HangfireEnabledOnThisServer())
            {
                services.AddHangfireServer();
            }
        }

        public static void ConfigureIOC(IContainer container)
        {
            GlobalConfiguration.Configuration.UseAutofacActivator(container);
        }

        public static void ConfigureApp(IApplicationBuilder app)
        {
            app.UseHangfireDashboard(
                "/admin/hangfire",
                new DashboardOptions {Authorization = new[] {new HangfireAuthorisationFilter()}, AppPath = "/admin"});

            if (HangfireEnabledOnThisServer())
            {
                BackgroundJobsApi.InitialiseScheduledJobs();
            }
        }

        private static bool HangfireEnabledOnThisServer()
        {
            int webjobsStoppedSetting = Config.GetAppSetting("WEBJOBS_STOPPED").ToInt32(0);
            bool webJobsAreEnabled = (webjobsStoppedSetting == 0);
            return webJobsAreEnabled;
        }

    }
}
