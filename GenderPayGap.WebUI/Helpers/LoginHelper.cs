using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using GenderPayGap.WebUI.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Helpers
{
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


        public static IActionResult Logout(HttpContext httpContext, IActionResult suggestedActionResult)
        {
            if (IsUserBeingImpersonated(httpContext.User))
            {
                // If the user is being impersonated by an administrator, then:
                // - don't log the user out completely
                // - just log them back in as the original administrator user
                long employerUserId = GetUserId(httpContext.User);

                long adminUserId = GetAdminImpersonatorUserId(httpContext.User);
                Login(httpContext, adminUserId, "GPGadmin");

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

    }
}
