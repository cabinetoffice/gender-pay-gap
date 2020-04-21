using GenderPayGap.Core;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GenderPayGap.WebUI.ScheduledJobs.HangfireConfiguration
{
    public static class HangfireConfigurationHelper
    {

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(x => x.UseSqlServerStorage(Global.DatabaseConnectionString));
            services.AddHangfireServer();
        }

        public static void ConfigureApp(IApplicationBuilder app)
        {
            app.UseHangfireDashboard(
                "/admin/hangfire",
                new DashboardOptions {Authorization = new[] {new HangfireAuthorisationFilter()}, AppPath = "/admin"});
        }

    }
}
