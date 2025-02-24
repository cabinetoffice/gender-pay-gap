﻿using System.Security.Claims;
using GenderPayGap.WebUI.Helpers;
using Hangfire.Dashboard;

namespace GenderPayGap.WebUI.BackgroundJobs.HangfireConfiguration
{
    public class HangfireAuthorisationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }

            var claims = httpContext.User.Claims;
            Claim roleClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);

            bool isAdministrator = roleClaim != null && roleClaim.Value == LoginRoles.GpgAdmin;

            return isAdministrator;
        }
    }
}
