using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
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
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
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
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
                    m => m.OtherErrorMessagePlaceholder, 
                    "This user's email address has already been verified");
                return View("ResendVerificationEmail", viewModel);
            }

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);
            if (viewModel.HasAnyErrors())
            {
                return View("ResendVerificationEmail", viewModel);
            }

            auditLogger.AuditChangeToUser(
                this,
                AuditedAction.AdminResendVerificationEmail,
                user,
                new
                {
                    Reason = viewModel.Reason
                }
                );

            string verifyCode = Encryption.EncryptQuerystring(user.UserId + ":" + user.Created.ToSmallDateTime());

            user.EmailVerifyHash = Crypto.GetSHA512Checksum(verifyCode);
            user.EmailVerifySendDate = VirtualDateTime.Now;
            dataRepository.SaveChangesAsync().Wait();

            string verifyUrl = Url.Action("VerifyEmail", "Register", new { code = verifyCode }, "https");

            if (!Emails.SendCreateAccountPendingVerificationAsync(verifyUrl, user.EmailAddress).Result)
            {
                viewModel.AddErrorFor<AdminResendVerificationEmailViewModel, object>(
                    m => m.OtherErrorMessagePlaceholder,
                    "Error whilst re-sending verification email. Please try again in a few minutes.");
                return View("ResendVerificationEmail", viewModel);
            }

            return View("VerificationEmailSent", user);
        }

    }
}
