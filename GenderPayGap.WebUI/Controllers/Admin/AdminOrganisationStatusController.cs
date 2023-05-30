using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
    [Route("admin")]
    public class AdminOrganisationStatusController : Controller
    {

        private readonly AuditLogger auditLogger;

        private readonly IDataRepository dataRepository;

        public AdminOrganisationStatusController(IDataRepository dataRepository,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/status")]
        public IActionResult ViewStatusHistory(long id)
        {
            var organisation = dataRepository.Get<Organisation>(id);

            return View("ViewOrganisationStatus", organisation);
        }


        [HttpGet("organisation/{id}/status/change")]
        public IActionResult ChangeStatusGet(long id)
        {
            var viewModel = new AdminChangeOrganisationStatusViewModel();

            UpdateAdminChangeStatusViewModelFromOrganisation(viewModel, id);
            SetInitialValueOfNewScopes(viewModel);

            return View("ChangeStatus", viewModel);
        }

        [HttpPost("organisation/{id}/status/change")]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeStatusPost(long id, AdminChangeOrganisationStatusViewModel viewModel)
        {
            switch (viewModel.Action)
            {
                case ChangeOrganisationStatusViewModelActions.OfferNewStatusAndReason:
                    UpdateAdminChangeStatusViewModelFromOrganisation(viewModel, id);
                    if (!ModelState.IsValid)
                    {
                        return View("ChangeStatus", viewModel);
                    }

                    return View("ConfirmStatusChange", viewModel);

                case ChangeOrganisationStatusViewModelActions.ConfirmStatusChange:
                    ChangeStatus(viewModel, id);
                    return RedirectToAction("ViewStatusHistory", "AdminOrganisationStatus", new {id});
                default:
                    throw new ArgumentException("Unknown action in AdminOrganisationStatusController.ChangeStatusPost");
            }
        }

        private void UpdateAdminChangeStatusViewModelFromOrganisation(AdminChangeOrganisationStatusViewModel viewModel, long organisationId)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            
            viewModel.Organisation = organisation;

            viewModel.InactiveUserOrganisations = dataRepository.GetAll<InactiveUserOrganisation>()
                .Where(m => m.OrganisationId == organisationId).ToList();

            if (organisation.Status == OrganisationStatuses.New ||
                organisation.Status == OrganisationStatuses.Pending ||
                organisation.Status == OrganisationStatuses.Active)
            {
                foreach (int reportingYear in ReportingYearsHelper.GetReportingYears())
                {
                    if (!viewModel.Years.Any(y => y.ReportingYear == reportingYear))
                    {
                        viewModel.Years.Add(new AdminChangeOrganisationStatusReportingYearViewModel
                        {
                            ReportingYear = reportingYear
                        });
                    }
                }

                foreach (AdminChangeOrganisationStatusReportingYearViewModel yearViewModel in viewModel.Years)
                {
                    yearViewModel.CurrentScope = organisation.GetScopeStatusForYear(yearViewModel.ReportingYear);
                    yearViewModel.HasReported = organisation.HasSubmittedReturn(yearViewModel.ReportingYear);
                }

                viewModel.Years = viewModel.Years.OrderByDescending(y => y.ReportingYear).ToList();
            }
        }

        private void SetInitialValueOfNewScopes(AdminChangeOrganisationStatusViewModel viewModel)
        {
            // The admin user is Retiring / Deleting this organisation.
            // This is usually because the organisation has stopped trading.
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
            foreach (AdminChangeOrganisationStatusReportingYearViewModel yearViewModel in viewModel.Years.OrderByDescending(y => y.ReportingYear))
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
                yearViewModel.MarkAsOutOfScope = true;
                viewModel.AnyGuessedScopeChanges = true;
            }
        }

        private void ChangeStatus(AdminChangeOrganisationStatusViewModel viewModel, long organisationId)
        {
            var organisation = dataRepository.Get<Organisation>(organisationId);
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            OrganisationStatuses previousStatus = organisation.Status;
            OrganisationStatuses newStatus = viewModel.NewStatus ?? OrganisationStatuses.Unknown;

            // Update the status
            organisation.SetStatus(
                newStatus,
                currentUser.UserId,
                viewModel.Reason);
            
            // Change scopes
            foreach (AdminChangeOrganisationStatusReportingYearViewModel yearViewModel in viewModel.Years)
            {
                if (yearViewModel.MarkAsOutOfScope)
                {
                    organisation.SetScopeForYear(yearViewModel.ReportingYear, ScopeStatuses.OutOfScope, viewModel.Reason);
                }
            }

            // [In]Activate users of the organisation
            if (newStatus == OrganisationStatuses.Active)
            {
                ActivateUsersOfOrganisation(organisation);
            }

            if (previousStatus == OrganisationStatuses.Active)
            {
                InactivateUsersOfOrganisation(organisation);
            }

            dataRepository.SaveChanges();

            // Audit log
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationStatus,
                organisation,
                new {PreviousStatus = previousStatus, NewStatus = newStatus, viewModel.Reason},
                User);

        }

        private void ActivateUsersOfOrganisation(Organisation organisation)
        {
            IQueryable<InactiveUserOrganisation> inactiveUserOrganisations = dataRepository.GetAll<InactiveUserOrganisation>()
                .Where(m => m.OrganisationId == organisation.OrganisationId);
            foreach (InactiveUserOrganisation inactiveUserOrganisation in inactiveUserOrganisations)
            {
                organisation.UserOrganisations.Add(CreateUserOrganisationFromInactiveUserOrganisation(inactiveUserOrganisation));
                dataRepository.Delete(inactiveUserOrganisation);
            }
        }

        private UserOrganisation CreateUserOrganisationFromInactiveUserOrganisation(InactiveUserOrganisation inactiveUserOrganisation)
        {
            return new UserOrganisation
            {
                UserId = inactiveUserOrganisation.UserId,
                OrganisationId = inactiveUserOrganisation.OrganisationId,
                PIN = inactiveUserOrganisation.PIN,
                PINSentDate = inactiveUserOrganisation.PINSentDate,
                PITPNotifyLetterId = inactiveUserOrganisation.PITPNotifyLetterId,
                PINConfirmedDate = inactiveUserOrganisation.PINConfirmedDate,
                ConfirmAttemptDate = inactiveUserOrganisation.ConfirmAttemptDate,
                ConfirmAttempts = inactiveUserOrganisation.ConfirmAttempts,
                Method = inactiveUserOrganisation.Method
            };
        }

        private void InactivateUsersOfOrganisation(Organisation organisation)
        {
            foreach (UserOrganisation userOrganisation in organisation.UserOrganisations)
            {
                dataRepository.Insert(CreateInactiveUserOrganisationFromUserOrganisation(userOrganisation));
                dataRepository.Delete(userOrganisation);
            }
        }

        private InactiveUserOrganisation CreateInactiveUserOrganisationFromUserOrganisation(UserOrganisation userOrganisation)
        {
            return new InactiveUserOrganisation
            {
                UserId = userOrganisation.UserId,
                OrganisationId = userOrganisation.OrganisationId,
                PIN = userOrganisation.PIN,
                PINSentDate = userOrganisation.PINSentDate,
                PITPNotifyLetterId = userOrganisation.PITPNotifyLetterId,
                PINConfirmedDate = userOrganisation.PINConfirmedDate,
                ConfirmAttemptDate = userOrganisation.ConfirmAttemptDate,
                ConfirmAttempts = userOrganisation.ConfirmAttempts,
                Method = userOrganisation.Method
            };
        }

    }
}
