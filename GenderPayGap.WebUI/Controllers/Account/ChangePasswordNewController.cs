using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    [Route("manage-account")]
    public class ChangePasswordNewController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly IUserRepository userRepository;
        private readonly EmailSendingService emailSendingService;

        public ChangePasswordNewController(
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

            // Return the Change Personal Details form
            return View("ChangePasswordNew", new ChangePasswordNewViewModel());
        }

        [HttpPost("change-password-new")]
        [ValidateAntiForgeryToken]
        [PreventDuplicatePost]
        public async Task<IActionResult> ChangePasswordPost(ChangePasswordNewViewModel viewModel)
        {
            // Check all values are provided and NewPassword is at least 8 characters long
            viewModel.ParseAndValidateParameters(Request, m => m.CurrentPassword);
            viewModel.ParseAndValidateParameters(Request, m => m.NewPassword);
            viewModel.ParseAndValidateParameters(Request, m => m.ConfirmNewPassword);
            
            // Get the current user
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            
            // Check that passwords are valid
            ValidatePasswords(viewModel, currentUser);

            if (viewModel.HasAnyErrors())
            {
                return View("ChangePasswordNew", viewModel);
            }
            
            userRepository.UpdateUserPasswordUsingPBKDF2(currentUser, viewModel.NewPassword);
            
            // send password change notification
            emailSendingService.SendChangePasswordCompletedEmail(currentUser.EmailAddress);

            // Set up success notification on Manage Account page
            string nextPageUrl = Url.Action("ManageAccount", "ManageAccount", new {Area = "Account"});
            StatusMessageHelper.SetStatusMessage(Response, "Your password has been changed successfully", nextPageUrl);

            // Return user to the Manage Account page
            return LocalRedirect(nextPageUrl);
        }

        public async void ValidatePasswords(ChangePasswordNewViewModel viewModel, User currentUser)
        {
            // Check if current password is correct
            bool isValidPassword = await userRepository.CheckPasswordAsync(currentUser, viewModel.CurrentPassword);
            if (!isValidPassword)
            {
                viewModel.AddErrorFor(m => m.CurrentPassword, "Could not verify your current password.");
            }

            if (!viewModel.NewPassword.Any(char.IsUpper) || !viewModel.NewPassword.Any(char.IsLower) || !viewModel.NewPassword.Any(char.IsDigit))
            {
                viewModel.AddErrorFor(m => m.NewPassword, "New password must contain at least one upper case, 1 lower case character and 1 digit");
            }

            // Check if new password is the same as old password
            if (viewModel.CurrentPassword == viewModel.NewPassword)
            {
                viewModel.AddErrorFor(m => m.NewPassword, "The password you have entered must be different from your previous password");
            }

            // Check if new password and confirmation password match
            if (viewModel.NewPassword != viewModel.ConfirmNewPassword)
            {
                viewModel.AddErrorFor(m => m.ConfirmNewPassword, "The password and confirmation password do not match.");
            }
        }

    }
}
