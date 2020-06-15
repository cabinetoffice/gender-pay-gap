using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Helpers
{
    public static class LoginRoles
    {
        public const string GpgAdmin = "GPGadmin";
        public const string GpgEmployer = "GPGemployer";
    }

    public static class LoginHelper
    {

        public static void Login(HttpContext httpContext, long userId, string userRole)
        {
            var claims = new List<Claim>
            {
                new Claim("user_id", userId.ToString()),
                new Claim(ClaimTypes.Role, userRole)
            };

            Login(httpContext, claims);
        }

        public static void LoginWithImpersonation(HttpContext httpContext, long userId, string userRole, long adminImpersonatorUserId)
        {
            var claims = new List<Claim>
            {
                new Claim("user_id", userId.ToString()),
                new Claim(ClaimTypes.Role, userRole),
                new Claim("admin_impersonator_user_id", adminImpersonatorUserId.ToString())
            };

            Login(httpContext, claims);
        }

        private static void Login(HttpContext httpContext, List<Claim> claims)
        {
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                //AllowRefresh = <bool>,
                // Refreshing the authentication session should be allowed.

                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                // The time at which the authentication ticket expires. A 
                // value set here overrides the ExpireTimeSpan option of 
                // CookieAuthenticationOptions set with AddCookie.

                //IsPersistent = true,
                // Whether the authentication session is persisted across 
                // multiple requests. When used with cookies, controls
                // whether the cookie's lifetime is absolute (matching the
                // lifetime of the authentication ticket) or session-based.

                //IssuedUtc = <DateTimeOffset>,
                // The time at which the authentication ticket was issued.

                //RedirectUri = <string>
                // The full path or absolute URI to be used as an http 
                // redirect response value.
            };

            httpContext
                .SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties)
                .Wait();
        }

        public static IActionResult Logout(HttpContext httpContext, IActionResult suggestedActionResult)
        {
            if (IsUserBeingImpersonated(httpContext.User))
            {
                // If the user is being impersonated by an administrator, then:
                // - don't log the user out completely
                // - just log them back in as the original administrator user
                long employerUserId = GetUserId(httpContext.User);

                long adminUserId = GetAdminImpersonatorUserId(httpContext.User);
                Login(httpContext, adminUserId, LoginRoles.GpgAdmin);

                return new RedirectToActionResult("ViewUser", "AdminViewUser", new {id = employerUserId});
            }
            else
            {
                if (httpContext.RequestServices != null)
                {
                    httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
                }
                return suggestedActionResult;
            }
        }

        public static long GetUserId(IPrincipal principal)
        {
            string userIdString = GetClaim(principal, "user_id");
            return Convert.ToInt64(userIdString);
        }

        public static long GetAdminImpersonatorUserId(IPrincipal principal)
        {
            string adminImpersonatorUserId = GetClaim(principal, "admin_impersonator_user_id");
            return Convert.ToInt64(adminImpersonatorUserId);
        }

        public static bool IsUserBeingImpersonated(IPrincipal principal)
        {
            string adminImpersonatorUserId = GetClaim(principal, "admin_impersonator_user_id");
            bool isImpersonating = adminImpersonatorUserId != null;
            return isImpersonating;
        }

        private static string GetClaim(IPrincipal principal, string claimType)
        {
            if (principal == null || !principal.Identity.IsAuthenticated)
            {
                return null;
            }

            IEnumerable<Claim> claims = (principal as ClaimsPrincipal).Claims;

            //Use this to lookup the long UserID from the db - ignore the authProvider for now
            Claim claim = claims.FirstOrDefault(c => c.Type.ToLower() == claimType.ToLower());
            return claim == null ? null : claim.Value;
        }

        public static bool UserIsLockedOutBecauseOfTooManyRecentFailedLoginAttempts(User user)
        {
            int failedLoginAttempts = user.LoginAttempts;
            int maxAllowedFailedLoginAttempts = Global.MaxLoginAttempts;

            if (failedLoginAttempts < maxAllowedFailedLoginAttempts)
            {
                // The number of failed login attempts is acceptable (not too many)
                return false;
            }

            DateTime? mostRecentLoginAttempt = user.LoginDate;
            if (!mostRecentLoginAttempt.HasValue)
            {
                // If we don't know when the most recent login attempt is, we don't have any way of calculating when the lockout will end
                // In this case, just let them try again (this should never happen, we just need this check because mostRecentLoginAttempt is nullable!)
                return false;
            }

            int timeInMinutesUserIsLockedOut = Global.LockoutMinutes;
            DateTime lockoutEndTime = mostRecentLoginAttempt.Value.AddMinutes(timeInMinutesUserIsLockedOut);

            if (lockoutEndTime < VirtualDateTime.Now)
            {
                // The user was locked out, but the lockout time has expired, so they're allowed to try again
                return false;
            }

            // The user has been locked out and they are still within the lockout time
            return true;
        }

        public static int GetMinutesUntilAccountIsUnlocked(User user)
        {
            DateTime? mostRecentLoginAttempt = user.LoginDate;

            int timeInMinutesUserIsLockedOut = Global.LockoutMinutes;
            DateTime lockoutEndTime = mostRecentLoginAttempt.Value.AddMinutes(timeInMinutesUserIsLockedOut);

            TimeSpan timeUntilLockoutEnds = lockoutEndTime.Subtract(VirtualDateTime.Now);
            int minutesUntilLockoutEndsRoundedUp = (int) Math.Ceiling(timeUntilLockoutEnds.TotalMinutes);
            return minutesUntilLockoutEndsRoundedUp;
        }

    }
}
