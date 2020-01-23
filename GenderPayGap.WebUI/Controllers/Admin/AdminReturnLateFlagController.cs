using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Models.Admin;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminReturnLateFlagController : Controller
    {

        private readonly AuditLogger auditLogger;
        private readonly IDataRepository dataRepository;

        public AdminReturnLateFlagController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("return/{id}/change-late-flag")]
        public IActionResult ChangeLateFlag(long id)
        {
            var specifiedReturn = dataRepository.Get<Return>(id);

            var viewModel = new AdminReturnLateFlagViewModel {Return = specifiedReturn, NewLateFlag = !specifiedReturn.IsLateSubmission};

            return View("ChangeLateFlag", viewModel);
        }

        [HttpPost("return/{id}/change-late-flag")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeLateFlag(long id, AdminReturnLateFlagViewModel viewModel)
        {
            var specifiedReturn = dataRepository.Get<Return>(id);

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                viewModel.Return = specifiedReturn;

                // If there are any errors, return the user back to the same page to correct the mistakes
                return View("ChangeLateFlag", viewModel);
            }

            if (viewModel.NewLateFlag is null)
            {
                throw new ArgumentNullException(nameof(viewModel.NewLateFlag));
            }

            specifiedReturn.IsLateSubmission = viewModel.NewLateFlag.Value;

            dataRepository.SaveChangesAsync().Wait();

            auditLogger.AuditAction(
                this,
                AuditedAction.AdminChangeLateFlag,
                specifiedReturn.OrganisationId,
                new Dictionary<string, string> {
                    {"ReturnId", $"{id}"}, {"LateFlagChangedTo", $"{viewModel.NewLateFlag}"}, {"Reason", viewModel.Reason}
                });

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation", new {id = specifiedReturn.OrganisationId});
        }

    }
}
