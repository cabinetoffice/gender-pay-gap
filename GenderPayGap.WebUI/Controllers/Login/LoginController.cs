using System;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Login;
using GenderPayGap.WebUI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Login
{
    public class LoginController : Controller
    {

        private readonly UserRepository userRepository;

        public LoginController(UserRepository userRepository)
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
            if (!ModelState.IsValid)
            {
                return View("Login", viewModel);
            }

            User user = userRepository.FindByEmail(viewModel.EmailAddress, UserStatuses.New, UserStatuses.Active);

            if (user == null)
            {
                ModelState.AddModelError(nameof(viewModel.Password), "Incorrect email address or password. Please double-check and try again");
                return View("Login", viewModel);
            }

            if (LoginHelper.UserIsLockedOutBecauseOfTooManyRecentFailedLoginAttempts(user))
            {
                ModelState.AddModelError(nameof(viewModel.Password), "You have entered the email address or password wrong too many times. "
                                                                     + $"Please try again in {LoginHelper.GetMinutesUntilAccountIsUnlocked(user)} minutes");
                return View("Login", viewModel);
            }

            if (!userRepository.CheckPassword(user, viewModel.Password))
            {
                ModelState.AddModelError(nameof(viewModel.Password), "Incorrect email address or password. Please double-check and try again");
                return View("Login", viewModel);
            }

            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(user);

            string userRole = LoginHelper.GetLoginRoleFromUserRole(user);

            LoginHelper.Login(HttpContext, user.UserId, userRole);

            if (ReturnUrlIsAllowed(viewModel.ReturnUrl))
            {
                // Above condition prevents invalid return urls
                //disable:DoNotUseRedirectWithReturnUrls
                return Redirect(viewModel.ReturnUrl);
            }
            else if (user.IsFullOrReadOnlyAdministrator())
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
            string fullUrlToHomepage = Url.Action("Index", "Homepage", new { }, "https"); // i.e. (on Prod) https://gender-pay-gap.service.gov.uk/

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
                // Above condition prevents invalid return urls
                //disable:DoNotUseRedirectWithReturnUrls
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
