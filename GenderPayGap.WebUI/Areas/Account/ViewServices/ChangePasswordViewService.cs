using System;
using System.Threading.Tasks;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Areas.Account.Abstractions;
using GenderPayGap.WebUI.Areas.Account.Resources;
using GenderPayGap.WebUI.Areas.Account.ViewModels;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Areas.Account.ViewServices
{

    public class ChangePasswordViewService : IChangePasswordViewService
    {

        private readonly EmailSendingService emailSendingService;

        public ChangePasswordViewService(
            IUserRepository userRepo,
            EmailSendingService emailSendingService)
        {
            UserRepository = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
            this.emailSendingService = emailSendingService;
        }

        private IUserRepository UserRepository { get; }

        public async Task<ModelStateDictionary> ChangePasswordAsync(User currentUser, string currentPassword, string newPassword)
        {
            var errorState = new ModelStateDictionary();

            // check users current password
            bool checkPasswordResult = await UserRepository.CheckPasswordAsync(currentUser, currentPassword);
            if (checkPasswordResult == false)
            {
                errorState.AddModelError(nameof(ChangePasswordViewModel.CurrentPassword), "Could not verify your current password");
                return errorState;
            }

            // prevent a user from re-using the same password
            if (currentPassword.ToLower() == newPassword.ToLower())
            {
                errorState.AddModelError(nameof(ChangePasswordViewModel.NewPassword), AccountResources.ChangePasswordMustBeDifferent);
                return errorState;
            }

            // update user password
            UserRepository.UpdatePassword(currentUser, newPassword);

            // send password change notification
            emailSendingService.SendChangePasswordCompletedEmail(currentUser.EmailAddress);

            return errorState;
        }

    }

}
