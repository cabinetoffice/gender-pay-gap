using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminUserResendVerificationEmailController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly AuditLogger auditLogger;
        private readonly EmailSendingService emailSendingService;

        public AdminUserResendVerificationEmailController(
            IDataRepository dataRepository,
            AuditLogger auditLogger,
            EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
            this.emailSendingService = emailSendingService;
        }

        [HttpGet("user/{id}/resend-verification-email")]
        public IActionResult ResendVerificationEmailGet(long id)
        {
            User user = dataRepository.Get<User>(id);

            var viewModel = new AdminResendVerificationEmailViewModel { User = user };

            if (user.EmailVerifiedDate != null)
            {
                ModelState.AddModelError(nameof(viewModel.OtherErrorMessagePlaceholder), "This user's email address has already been verified");
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
                ModelState.AddModelError(nameof(viewModel.OtherErrorMessagePlaceholder), "This user's email address has already been verified");
            }

            if (!ModelState.IsValid)
            {
                return View("ResendVerificationEmail", viewModel);
            }

            auditLogger.AuditChangeToUser(
                AuditedAction.AdminResendVerificationEmail,
                user,
                new
                {
                    viewModel.Reason
                },
                User);
            
            string verificationCode = Guid.NewGuid().ToString("N");
            string verificationUrl = Url.Action(
                "VerifyEmail",
                "AccountCreation",
                new {code = verificationCode},
                "https");
            
            emailSendingService.SendAccountVerificationEmail(user.EmailAddress, verificationUrl);
            user.EmailVerifyHash = verificationCode;
            user.EmailVerifySendDate = VirtualDateTime.Now;

            dataRepository.SaveChanges();

            return View("VerificationEmailSent", user);
        }

    }
}
