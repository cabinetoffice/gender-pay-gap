using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Scope;
using GenderPayGap.WebUI.Models.ScopeNew;
using GenderPayGap.WebUI.Services;
using GovUkDesignSystem;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("organisation")]
    public class ScopeNewController : Controller
    {

        private readonly EmailSendingService emailSendingService;
        private readonly IDataRepository dataRepository;
        
        public ScopeNewController(EmailSendingService emailSendingService, IDataRepository dataRepository)
        {
            this.emailSendingService = emailSendingService;
            this.dataRepository = dataRepository;
        }

        [Authorize]
        [HttpGet("{encryptedOrganisationId}/reporting-year/{reportingYear}/change-scope")]
        public IActionResult ChangeOrganisationScope(string encryptedOrganisationId, int reportingYear)
        {
            long organisationId = DecryptOrganisationId(encryptedOrganisationId);

            // Check user has permissions to access this page
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);

            // Get Organisation and OrganisationScope for reporting year
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            OrganisationScope organisationScope = organisation.GetScopeForYear(reportingYear);
            
            var viewModel = new OutOfScopeViewModel {Organisation = organisation, ReportingYear = organisationScope.SnapshotDate};
            
            return View(organisationScope.IsInScopeVariant()  ? "OutOfScopeQuestions" : "ConfirmInScope", viewModel);
        }
        
        [HttpPost("{encryptedOrganisationId}/reporting-year/{reportingYear}/change-scope/out")]
        public IActionResult SubmitOutOfScopeAnswers(string encryptedOrganisationId, int reportingYear, OutOfScopeViewModel viewModel)
        {
            long organisationId = DecryptOrganisationId(encryptedOrganisationId);

            viewModel.ParseAndValidateParameters(Request, m => m.WhyOutOfScope);
            viewModel.ParseAndValidateParameters(Request, m => m.HaveReadGuidance);

            if (viewModel.WhyOutOfScope == WhyOutOfScope.Other )
            {
                viewModel.ParseAndValidateParameters(Request, m => m.WhyOutOfScopeDetails);
            }

            // Get Organisation and OrganisationScope for reporting year
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            OrganisationScope organisationScope = organisation.OrganisationScopes.FirstOrDefault(s => s.SnapshotDate.Year == reportingYear);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = organisationScope.SnapshotDate;
            
            if (viewModel.HasAnyErrors())
            {
                return View("OutOfScopeQuestions", viewModel);
            }
            return View("ConfirmOutOfScope", viewModel);
        }

        [HttpPost("{encryptedOrganisationId}/reporting-year/{reportingYear}/change-scope/out/confirm")]
        public IActionResult ConfirmOutOfScopeAnswers(string encryptedOrganisationId, int reportingYear, OutOfScopeViewModel viewModel)
        {
            long organisationId = DecryptOrganisationId(encryptedOrganisationId);

            // Get Organisation
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            
            // Update OrganisationScope
            var reasonForChange = viewModel.WhyOutOfScope == WhyOutOfScope.Under250
                ? "Under250"
                : viewModel.WhyOutOfScopeDetails;
            
            DateTime currentSnapshotDate = organisation.SectorType.GetAccountingStartDate();
            
            RetireOldScopes(organisation, reportingYear);
            
            UpdateScopes(organisation, ScopeStatuses.OutOfScope, reportingYear, reasonForChange, viewModel.HaveReadGuidance == HaveReadGuidance.Yes);

            dataRepository.SaveChangesAsync().Wait();
            
            SendScopeChangeEmails(organisation, viewModel.ReportingYear, currentSnapshotDate, false);

            OrganisationScope organisationScope = organisation.OrganisationScopes.FirstOrDefault(s => s.SnapshotDate.Year == reportingYear);

            viewModel.Organisation = organisation;
            viewModel.ReportingYear = organisationScope.SnapshotDate;

            return View("FinishOutOfScopeJourney", viewModel);
        }
        
        [HttpPost("{encryptedOrganisationId}/reporting-year/{reportingYear}/change-scope/in/confirm")]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmInScopeAnswers(string encryptedOrganisationId, int reportingYear, OutOfScopeViewModel viewModel)
        {
            long organisationId = DecryptOrganisationId(encryptedOrganisationId);

            // Get Organisation and OrganisationScope for reporting year
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            
            DateTime currentSnapshotDate = organisation.SectorType.GetAccountingStartDate();
            
            RetireOldScopes(organisation, reportingYear);
            
            UpdateScopes(organisation, ScopeStatuses.InScope, reportingYear, null, null);

            dataRepository.SaveChangesAsync().Wait();
            
            SendScopeChangeEmails(organisation, viewModel.ReportingYear, currentSnapshotDate, true);

            return RedirectToAction("ManageOrganisation", "Organisation", new { id = encryptedOrganisationId });
        }

        public void RetireOldScopes(Organisation organisation, int reportingYear)
        {
            organisation.OrganisationScopes.Where(o => o.SnapshotDate.Year == reportingYear).ForEach(s => s.Status = ScopeRowStatuses.Retired);
        }
        
        public void UpdateScopes(Organisation organisation, ScopeStatuses newStatus, int reportingYear, string reasonForChange, bool? haveReadGuidance)
        {
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            organisation.OrganisationScopes.Add(
                new OrganisationScope {
                    OrganisationId = organisation.OrganisationId,
                    ScopeStatus = newStatus,
                    StatusDetails = "Generated by the system",
                    ContactFirstname = currentUser.Firstname,
                    ContactLastname = currentUser.Lastname,
                    ContactEmailAddress = currentUser.EmailAddress,
                    Status = ScopeRowStatuses.Active,
                    SnapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear),
                    Reason = reasonForChange,
                    ReadGuidance = haveReadGuidance
                });
        }

        public void SendScopeChangeEmails(Organisation organisation, DateTime reportingYear, DateTime currentSnapshotDate, bool isInScopeChange )
        {
            // Send emails if scope changed on current or previous reporting year
            if (reportingYear == currentSnapshotDate || reportingYear == currentSnapshotDate.AddYears(-1))
            {
                // Find all email addresses associated with the organisation - only the active ones (who have confirmed their PIN)
                IEnumerable<string> emailAddressesForOrganisation = organisation.UserOrganisations
                    .Where(uo => uo.PINConfirmedDate.HasValue)
                    .Select(uo => uo.User.EmailAddress);
                
                // Send email of correct type to each email address associated with organisation
                foreach (string emailAddress in emailAddressesForOrganisation)
                {
                    if (isInScopeChange)
                    {
                        // Use Notify to send in scope email
                        emailSendingService.SendScopeChangeInEmail(emailAddress, organisation.OrganisationName);
                    }
                    else
                    {
                        // Use Notify to send out of scope email
                        emailSendingService.SendScopeChangeOutEmail(emailAddress, organisation.OrganisationName);
                    }
                    
                }
            }
        }

        public long DecryptOrganisationId(string encryptedOrganisationId)
        {
            // Decrypt organisation ID param
            if (!encryptedOrganisationId.DecryptToId(out long organisationId))
            {
                throw new PageNotFoundException();
            }

            return organisationId;
        }
    }
}
