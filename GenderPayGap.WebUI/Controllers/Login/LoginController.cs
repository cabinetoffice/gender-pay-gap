using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Models.Login;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Login
{
    public class LoginController : Controller
    {

        private readonly IUserRepository userRepository;

        public LoginController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }


        [HttpGet("login")]
        public IActionResult LoginGet(string returnUrl)
        {
            var viewModel = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View("Login", viewModel);
        }

        [HttpPost("login")]
        [ValidateAntiForgeryToken]
        public IActionResult LoginPost(LoginViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.EmailAddress);
            viewModel.ParseAndValidateParameters(Request, m => m.Password);

            if (viewModel.HasAnyErrors())
            {
                return View("Login", viewModel);
            }

            User user = userRepository.FindByEmailAsync(viewModel.EmailAddress, UserStatuses.New, UserStatuses.Active).Result;

            if (user == null)
            {
                viewModel.AddErrorFor(m => m.Password, "Incorrect email address or password. Please double-check and try again");
                return View("Login", viewModel);
            }

            if (!userRepository.CheckPasswordAsync(user, viewModel.Password).Result)
            {
                viewModel.AddErrorFor(m => m.Password, "Incorrect email address or password. Please double-check and try again");
                return View("Login", viewModel);
            }

            string userId = user.UserId.ToString();
            string userRole = user.IsAdministrator() ? "GPGadmin" : "GPGemployer";

            var claims = new List<Claim>
            {
                new Claim("user_id", userId),
                new Claim(ClaimTypes.Role, userRole),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

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

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            if (ReturnUrlIsAllowed(viewModel))
            {
                return Redirect(viewModel.ReturnUrl);
            }
            else if (user.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin");
            }
            else
            {
                return RedirectToAction("ManageOrganisations", "Organisation");
            }
        }

        private bool ReturnUrlIsAllowed(LoginViewModel viewModel)
        {
            string fullUrlToHomepage = Url.Action("Index", "Viewing", new { }, "https"); // i.e. (on Prod) https://gender-pay-gap.service.gov.uk/

            return viewModel.ReturnUrl != null
                   && (viewModel.ReturnUrl.StartsWith(fullUrlToHomepage)
                       || Url.IsLocalUrl(viewModel.ReturnUrl));
        }


        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            LoginHelper.Logout(HttpContext);

            return View("LoggedOut");
        }

    }
}
