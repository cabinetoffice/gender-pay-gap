using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminOrganisationScopeController : Controller
    {

        private readonly AuditLogger auditLogger;

        private readonly IDataRepository dataRepository;

        public AdminOrganisationScopeController(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/scope")]
        public IActionResult ViewScopeHistory(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            return View("ViewOrganisationScope", organisation);
        }

        [HttpGet("organisation/{id}/scope/change/{year}")]
        public IActionResult ChangeScopeForYearGet(long id, int year)
        {
            var organisation = dataRepository.Get<Organisation>(id);
            var currentScopeStatus = organisation.GetScopeStatusForYear(year);

            var viewModel = new AdminChangeScopeViewModel {
                OrganisationName = organisation.OrganisationName,
                OrganisationId = organisation.OrganisationId,
                ReportingYear = year,
                CurrentScopeStatus = currentScopeStatus,
                NewScopeStatus = GetNewScopeStatus(currentScopeStatus)
            };
            
            return View("ChangeScope", viewModel);
        }

        [HttpPost("organisation/{id}/scope/change/{year}")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeScopeForYearPost(long id, int year, AdminChangeScopeViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(id);
            var currentOrganisationScope = organisation.GetScopeForYear(year);
            if (!ModelState.IsValid)
            {
                // If there are any errors, return the user back to the same page to correct the mistakes
                var currentScopeStatus = organisation.GetScopeStatusForYear(year);

                viewModel.OrganisationName = organisation.OrganisationName;
                viewModel.OrganisationId = organisation.OrganisationId;
                viewModel.ReportingYear = year;
                viewModel.CurrentScopeStatus = currentScopeStatus;

                return View("ChangeScope", viewModel);
            }
            
            ScopeStatuses newScope = ConvertNewScopeStatusToScopeStatus(viewModel.NewScopeStatus);

            organisation.SetScopeForYear(year, newScope, viewModel.Reason);
            
            dataRepository.SaveChanges();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationScope,
                organisation,
                new {
                    PreviousScope = currentOrganisationScope.ScopeStatus.ToString(),
                    NewScope = newScope.ToString(),
                    Reason = viewModel.Reason
                },
                User);

            return RedirectToAction("ViewScopeHistory", "AdminOrganisationScope", new {id = organisation.OrganisationId});
        }

        private NewScopeStatus? GetNewScopeStatus(ScopeStatuses previousScopeStatus)
        {
            if (previousScopeStatus == ScopeStatuses.InScope)
            {
                return NewScopeStatus.OutOfScope;
            }

            if (previousScopeStatus == ScopeStatuses.OutOfScope)
            {
                return NewScopeStatus.InScope;
            }

            return null;
        }
        
        private ScopeStatuses ConvertNewScopeStatusToScopeStatus(NewScopeStatus? newScopeStatus)
        {
            if (newScopeStatus == NewScopeStatus.InScope)
            {
                return ScopeStatuses.InScope;
            }

            if (newScopeStatus == NewScopeStatus.OutOfScope)
            {
                return ScopeStatuses.OutOfScope;
            }
            
            throw new Exception();
        }

    }
}
