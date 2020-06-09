using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
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

            string userRole = user.IsAdministrator() ? "GPGadmin" : "GPGemployer";

            LoginHelper.Login(HttpContext, user.UserId, userRole);

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
        public IActionResult Logout()
        {
            // "LoginHelper.Logout(...)" (below) logs out the user
            // But, they are still logged in for the full duration of this request
            // So, when the View is generated, is shows a "Sign out" button (because the user is still logged in when the view is generated)
            //
            // To prevent this problem, we redirect the user (so they make a new request)
            // In the second request, they are logged out, so everything is displayed properly
            IActionResult suggestedResult = RedirectToAction("LoggedOut", "Login");

            return LoginHelper.Logout(HttpContext, suggestedResult);
        }

        [HttpGet("logged-out")]
        public IActionResult LoggedOut()
        {
            return View("LoggedOut");
        }

    }
}
