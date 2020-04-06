using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Models.Admin
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminUserResendVerificationEmailController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;

        public AdminUserResendVerificationEmailController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("user/{id}/resend-verification-email")]
        public IActionResult ResendVerificationEmailGet(long id)
        {
            User user = dataRepository.Get<User>(id);

            var viewModel = new AdminResendVerificationEmailViewModel { User = user };

            if (user.EmailVerifiedDate != null)
            {
                viewModel.AddErrorFor(
                    m => m.OtherErrorMessagePlaceholder,
                    "This user's email address has already been verified");
            }

            return View("ResendVerificationEmail", viewModel);
        }

        [HttpPost("user/{id}/resend-verification-email")]
        [ValidateAntiForgeryToken]
        public IActionResult ResendVerificationEmailPost(long id, AdminResendVerificationEmailViewModel viewModel)
        {
            User user = dataRepository.Get<User>(id);
            viewModel.User = user;

            if (user.EmailVerifiedDate != null)
            {
                viewModel.AddErrorFor(
                    m => m.OtherErrorMessagePlaceholder, 
                    "This user's email address has already been verified");
                return View("ResendVerificationEmail", viewModel);
            }

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
            if (viewModel.HasAnyErrors())
            {
                return View("ResendVerificationEmail", viewModel);
            }

            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            auditLogger.AuditChangeToUser(
                AuditedAction.AdminResendVerificationEmail,
                user,
                new
                {
                    viewModel.Reason
                },
                currentUser);

            string verifyCode = Encryption.EncryptQuerystring(user.UserId + ":" + user.Created.ToSmallDateTime());

            user.EmailVerifyHash = Crypto.GetSHA512Checksum(verifyCode);
            user.EmailVerifySendDate = VirtualDateTime.Now;
            dataRepository.SaveChangesAsync().Wait();

            string verifyUrl = Url.Action("VerifyEmail", "Register", new { code = verifyCode }, "https");

            EmailSendingService.SendCreateAccountPendingVerificationEmail(user.EmailAddress, verifyUrl);

            return View("VerificationEmailSent", user);
        }

    }
}
