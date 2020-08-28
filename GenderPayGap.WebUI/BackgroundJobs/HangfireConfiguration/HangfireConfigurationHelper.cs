using Autofac;
using GenderPayGap.Core;
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
            services.AddHangfire(x => x.UsePostgreSqlStorage(Global.DatabaseConnectionString));

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
            return Global.BackgroundJobsEnabled;
        }

    }
}
