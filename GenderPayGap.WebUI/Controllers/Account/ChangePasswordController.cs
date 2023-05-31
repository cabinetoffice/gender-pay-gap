using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    [Route("manage-account")]
    [Authorize(Roles = LoginRoles.GpgEmployer + "," + LoginRoles.GpgAdmin)]
    public class ChangePasswordController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly IUserRepository userRepository;
        private readonly EmailSendingService emailSendingService;

        public ChangePasswordController(
            IDataRepository dataRepository,
            IUserRepository userRepository,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.userRepository = userRepository;
            this.emailSendingService = emailSendingService;
        }
        
        // The user asks to change their password
        // We direct them to a page that asks for their existing and new passwords
        [HttpGet("change-password-new")]
        public IActionResult ChangePasswordGet()
        {
            ControllerHelper.ThrowIfAdminIsImpersonatingUser(User);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            // Return the Change Personal Details form
            return View("ChangePassword", new ChangePasswordViewModel());
        }

        [HttpPost("change-password-new")]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePasswordPost(ChangePasswordViewModel viewModel)
        {
            ControllerHelper.ThrowIfAdminIsImpersonatingUser(User);
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            
            // Get the current user
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            // Check that passwords are valid
            ValidatePasswords(viewModel, currentUser);
            if (currentUser.ResetAttempts == Global.MaxAuthAttempts)
            {
                currentUser.ResetAttempts = 0;
                dataRepository.SaveChanges();
                StatusMessageHelper.SetStatusMessage(Response, "You have been logged out for security reasons", Url.Action("LoggedOut", "Login"));
                return LoginHelper.Logout(HttpContext, RedirectToAction("LoggedOut", "Login"));
            }

            if (!ModelState.IsValid)
            {
                return View("ChangePassword", viewModel);
            }
            
            userRepository.UpdatePassword(currentUser, viewModel.NewPassword);
            
            // send password change notification
            emailSendingService.SendChangePasswordCompletedEmail(currentUser.EmailAddress);

            // Set up success notification on Manage Account page
            string nextPageUrl = Url.Action("LoggedOut", "Login");
            StatusMessageHelper.SetStatusMessage(Response, "Your password has been changed successfully", nextPageUrl);

            return LoginHelper.Logout(HttpContext, RedirectToAction("LoggedOut", "Login"));
        }

        public void ValidatePasswords(ChangePasswordViewModel viewModel, User currentUser)
        {
            // Check if current password is correct
            bool isValidPassword = userRepository.CheckPassword(currentUser, viewModel.CurrentPassword, true);
            if (!isValidPassword)
            {
                ModelState.AddModelError(nameof(viewModel.CurrentPassword), "Could not verify your current password.");
            }

            // Check if new password is the same as old password
            if (viewModel.NewPassword == viewModel.CurrentPassword)
            {
                ModelState.AddModelError(nameof(viewModel.NewPassword), "Your new password cannot be the same as your old password.");
            }

            // Check if new password and confirmation password match
            if (viewModel.NewPassword != viewModel.ConfirmNewPassword)
            {
                ModelState.AddModelError(nameof(viewModel.ConfirmNewPassword), "The password and confirmation do not match.");
            }
        }

    }
}
