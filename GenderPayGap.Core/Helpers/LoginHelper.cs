using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;

namespace GenderPayGap.Core.Helpers
{
    public static class LoginHelper
    {

        public static void Logout(HttpContext httpContext)
        {
            httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait();
        }

    }
}
