using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
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

        [HttpGet("organisation/{id}/scope/change")]
        public IActionResult ChangeMultipleScopesGet(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);
            
            var viewModel = new AdminChangeMultipleScopesViewModel();
            
            UpdateAdminChangeMultipleScopesViewModelFromOrganisation(viewModel, organisation);

            if (organisation.Status == OrganisationStatuses.Retired || organisation.Status == OrganisationStatuses.Deleted)
            {
                SetInitialValueOfNewScopes(viewModel);
            }
            
            return View("ChangeMultipleScopes", viewModel);
        }

        private void UpdateAdminChangeMultipleScopesViewModelFromOrganisation(AdminChangeMultipleScopesViewModel viewModel, Organisation organisation)
        {
            viewModel.Organisation = organisation;

            foreach (int reportingYear in ReportingYearsHelper.GetReportingYears())
            {
                if (!viewModel.Years.Any(y => y.ReportingYear == reportingYear))
                {
                    viewModel.Years.Add(new AdminChangeMultipleScopesReportingYearViewModel
                    {
                        ReportingYear = reportingYear
                    });
                }
            }

            foreach (AdminChangeMultipleScopesReportingYearViewModel yearViewModel in viewModel.Years)
            {
                yearViewModel.CurrentScope = organisation.GetScopeStatusForYear(yearViewModel.ReportingYear);
                yearViewModel.HasReported = organisation.HasSubmittedReturn(yearViewModel.ReportingYear);
            }

            viewModel.Years = viewModel.Years.OrderByDescending(y => y.ReportingYear).ToList();
        }

        private void SetInitialValueOfNewScopes(AdminChangeMultipleScopesViewModel viewModel)
        {
            // If the status of the organisation is Retired / Deleted, then we can guess at the new scopes.
            // An organisation is usually Retired / Deleted because the organisation has stopped trading.
            // When an organisation stops trading, the usually DON'T login to the GPG service and mark themselves as Out Of Scope! :-o
            // So, the admin team will find this a few months later and mark the organisation as Retired.
            // 
            // Often, when the admin team do this, they will also want to mark some years as Out Of Scope (typically the previous 1 or 2 years).
            // To help them, we will guess which years they will want to change and pre-tick the checkboxes next to those years.
            // But which years should we pre-tick?
            // 
            // Here is our best guess at which years they will want to mark as Our Of Scope:
            // - Work backwards through the years, starting with the current reporting year
            // - If, for a reporting year, the organisation has NOT reported and is IN Scope, we think the admin team will probably want to mark them as Out Of Scope
            // - As soon as we find a year where the organisation HAS reported, stop
            // - As soon as we find a year where the organisation is OUT Of Scope, stop
            // - In both these cases, we assume the organisation has taken this action, so this was before they stopped trading
            
            // Work backwards through the years
            foreach (AdminChangeMultipleScopesReportingYearViewModel yearViewModel in viewModel.Years.OrderByDescending(y => y.ReportingYear))
            {
                // If the organisation has reported for that year, stop
                if (yearViewModel.HasReported)
                {
                    return;
                }

                // If the organisation is IN Scope for that year, stop
                if (!yearViewModel.CurrentScope.IsInScopeVariant())
                {
                    return;
                }

                // Otherwise, pre-tick the "Change to Out Of Scope" checkbox
                yearViewModel.NewScope = ScopeStatuses.OutOfScope;
                viewModel.AnyGuessedScopeChanges = true;
            }
        }

        [HttpPost("organisation/{id}/scope/change")]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeMultipleScopesPost(long id, AdminChangeMultipleScopesViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            if (!ModelState.IsValid)
            {
                UpdateAdminChangeMultipleScopesViewModelFromOrganisation(viewModel, organisation);

                return View("ChangeMultipleScopes", viewModel);
            }

            foreach (AdminChangeMultipleScopesReportingYearViewModel yearViewModel in viewModel.Years)
            {
                if (yearViewModel.NewScope.HasValue)
                {
                    organisation.SetScopeForYear(yearViewModel.ReportingYear, yearViewModel.NewScope.Value, viewModel.Reason);
                }
            }
            
            dataRepository.SaveChanges();

            return RedirectToAction("ViewScopeHistory", "AdminOrganisationScope", new {id = organisation.OrganisationId});
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
