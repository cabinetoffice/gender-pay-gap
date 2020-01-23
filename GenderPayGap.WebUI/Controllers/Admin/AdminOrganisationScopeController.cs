using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public IActionResult ChangeScopeGet(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);
            var currentScopeStatus = organisation.GetScopeStatus();

            var viewModel = new AdminChangeScopeViewModel {
                OrganisationName = organisation.OrganisationName,
                OrganisationId = organisation.OrganisationId,
                CurrentScopeStatus = currentScopeStatus,
                NewScopeStatus = GetNewScopeStatus(currentScopeStatus)
            };
            
            return View("ChangeScope", viewModel);
        }

        [HttpPost("organisation/{id}/scope/change")]
        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeScopePost(long id, AdminChangeScopeViewModel viewModel)
        {
            var organisation = dataRepository.Get<Organisation>(id);
            var previousOrganisationScope = organisation.GetCurrentScope();

            if (previousOrganisationScope.ScopeStatus != ScopeStatuses.InScope
                && previousOrganisationScope.ScopeStatus != ScopeStatuses.OutOfScope)
            {
                viewModel.ParseAndValidateParameters(Request, m => m.NewScopeStatus);
            }

            viewModel.ParseAndValidateParameters(Request, m => m.Reason);

            if (viewModel.HasAnyErrors())
            {
                // If there are any errors, return the user back to the same page to correct the mistakes
                viewModel.OrganisationId = organisation.OrganisationId;
                return View("ChangeScope", viewModel);
            }
            
            RetireOldScopesForCurrentSnapshotDate(organisation);
            
            ScopeStatuses newScope = ConvertNewScopeStatusToScopeStatus(viewModel.NewScopeStatus);

            var newOrganisationScope = new OrganisationScope {
                Organisation = organisation,
                ScopeStatus = newScope,
                ScopeStatusDate = DateTime.Now,
                ContactFirstname = previousOrganisationScope.ContactFirstname,
                ContactLastname = previousOrganisationScope.ContactLastname,
                ContactEmailAddress = previousOrganisationScope.ContactEmailAddress,
                Reason = viewModel.Reason,
                SnapshotDate = previousOrganisationScope.SnapshotDate,
                StatusDetails = "Changed by Admin",
                Status = ScopeRowStatuses.Active
            };

            dataRepository.Insert(newOrganisationScope);
            await dataRepository.SaveChangesAsync();

            organisation.LatestScope = newOrganisationScope;
            await dataRepository.SaveChangesAsync();

            auditLogger.AuditAction(
                this,
                AuditedAction.AdminChangeOrganisationScope,
                id,
                new Dictionary<string, string> {
                    {"PreviousScope", previousOrganisationScope.ScopeStatus.ToString()},
                    {"NewScope", newScope.ToString()},
                    {"Reason", viewModel.Reason}
                });

            return RedirectToAction("ViewScopeHistory", "AdminOrganisationScope", new {id = organisation.OrganisationId});
        }

        private void RetireOldScopesForCurrentSnapshotDate(Organisation organisation)
        {
            IOrderedEnumerable<OrganisationScope> organisationScopesForCurrentSnapshotDate = organisation.OrganisationScopes
                .OrderByDescending(s => s.SnapshotDate);   
            
            foreach (OrganisationScope organisationScope in organisationScopesForCurrentSnapshotDate)
            {
                organisationScope.Status = ScopeRowStatuses.Retired;
            }
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
