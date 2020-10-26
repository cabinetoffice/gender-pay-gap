using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Login;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
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

            if (LoginHelper.UserIsLockedOutBecauseOfTooManyRecentFailedLoginAttempts(user))
            {
                viewModel.AddErrorFor(
                    m => m.Password,
                    "You have entered the email address or password wrong too many times. "
                    + $"Please try again in {LoginHelper.GetMinutesUntilAccountIsUnlocked(user)} minutes");
                return View("Login", viewModel);
            }

            if (!userRepository.CheckPassword(user, viewModel.Password))
            {
                viewModel.AddErrorFor(m => m.Password, "Incorrect email address or password. Please double-check and try again");
                return View("Login", viewModel);
            }

            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(user);

            string userRole = user.IsAdministrator() ? LoginRoles.GpgAdmin : LoginRoles.GpgEmployer;

            LoginHelper.Login(HttpContext, user.UserId, userRole);

            if (ReturnUrlIsAllowed(viewModel.ReturnUrl))
            {
                return Redirect(viewModel.ReturnUrl);
            }
            else if (user.IsAdministrator())
            {
                return RedirectToAction("AdminHomePage", "AdminHomepage");
            }
            else
            {
                return RedirectToAction("ManageOrganisationsGet", "ManageOrganisations");
            }
        }

        private bool ReturnUrlIsAllowed(string returnUrl)
        {
            string fullUrlToHomepage = Url.Action("Index", "Viewing", new { }, "https"); // i.e. (on Prod) https://gender-pay-gap.service.gov.uk/

            return returnUrl != null
                   && (returnUrl.StartsWith(fullUrlToHomepage)
                       || Url.IsLocalUrl(returnUrl));
        }


        [HttpGet("logout")]
        public IActionResult Logout(string redirectUrl)
        {
            // "LoginHelper.Logout(...)" (below) logs out the user
            // But, they are still logged in for the full duration of this request
            // So, when the View is generated, is shows a "Sign out" button (because the user is still logged in when the view is generated)
            //
            // To prevent this problem, we redirect the user (so they make a new request)
            // In the second request, they are logged out, so everything is displayed properly
            IActionResult suggestedResult;
            if (ReturnUrlIsAllowed(redirectUrl))
            {
                suggestedResult = Redirect(redirectUrl);
            }
            else
            {
                suggestedResult = RedirectToAction("LoggedOut", "Login");
            }

            return LoginHelper.Logout(HttpContext, suggestedResult);
        }

        [HttpGet("logged-out")]
        public IActionResult LoggedOut()
        {
            return View("LoggedOut");
        }

    }
}
