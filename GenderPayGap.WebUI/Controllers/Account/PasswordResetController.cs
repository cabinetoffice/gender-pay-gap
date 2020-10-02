using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Account;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Account
{
    public class PasswordResetController : Controller
    {

        private readonly IDataRepository dataRepository;

        public PasswordResetController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("password-reset")]
        public IActionResult PasswordResetGet()
        {
            return View("PasswordReset", new PasswordResetViewModel());
        }

        [ValidateAntiForgeryToken]
        [PreventDuplicatePost]
        [HttpPost("password-reset")]
        public IActionResult PasswordResetPost(PasswordResetViewModel viewModel)
        {
            viewModel.ParseAndValidateParameters(Request, m => m.EmailAddress);

            if (viewModel.HasAnyErrors())
            {
                return View("PasswordReset", viewModel); 
            }

            // Find user associated with email address
            User userForPasswordReset = dataRepository.GetAll<User>().FirstOrDefault(u => u.EmailAddress == viewModel.EmailAddress);

            if (userForPasswordReset.IsNull())
            {
                // Do something about it
            }

            // If password reset email has been sent (and password hasn't been changed) within the last 10 minutes
            if (userForPasswordReset.ResetSendDate.HasValue && (userForPasswordReset.ResetSendDate.Value - DateTime.Now).TotalMinutes < 10)
            {
                // TODO: Redirect to custom error page
            }
            
            return null;
        }
        
    }
}
