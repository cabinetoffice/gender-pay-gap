﻿using GenderPayGap.Core;
using GenderPayGap.Extensions.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;

namespace GenderPayGap.WebUI.BackgroundJobs.HangfireConfiguration
{
    public static class HangfireConfigurationHelper
    {

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddHangfire(x => x.UsePostgreSqlStorage(Global.DatabaseConnectionString));

            if (!Config.IsProduction())
            {
                // In non-production environments, turn off job retries
                // (mainly to reduce the amount of spam the developers receive from alerts when jobs fail!)
                GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 0 });
            }

            if (HangfireEnabledOnThisServer())
            {
                services.AddHangfireServer();
            }
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
