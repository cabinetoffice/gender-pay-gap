﻿using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
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
        [ValidateAntiForgeryToken]
        public IActionResult ChangeLateFlag(long id, AdminReturnLateFlagViewModel viewModel)
        {
            var specifiedReturn = dataRepository.Get<Return>(id);

            if (!ModelState.IsValid)
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

            dataRepository.SaveChanges();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeLateFlag,
                specifiedReturn.Organisation,
                new
                {
                    ReturnId = id,
                    LateFlagChangedTo = viewModel.NewLateFlag,
                    Reason = viewModel.Reason
                },
                User);

            return RedirectToAction("ViewOrganisation", "AdminViewOrganisation", new {id = specifiedReturn.OrganisationId});
        }

    }
}
