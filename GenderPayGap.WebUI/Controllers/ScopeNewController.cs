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
            OrganisationScope organisationScope = organisation.OrganisationScopes.FirstOrDefault(s => s.SnapshotDate.Year == reportingYear);
            
            OutOfScopeViewModel viewModel = new OutOfScopeViewModel {Organisation = organisation, ReportingYear = organisationScope.SnapshotDate};
            
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

            if (viewModel.HaveReadGuidance == HaveReadGuidance.HaveNotReadGuidance)
            {
                viewModel.AddErrorFor(m => m.HaveReadGuidance, "Please read the guidance before you continue.");
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

            // Get Organisation and OrganisationScope for reporting year
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            OrganisationScope organisationScope = organisation.OrganisationScopes.FirstOrDefault(s => s.SnapshotDate.Year == reportingYear);
            
            // Update OrganisationScope
            var reasonForChange = viewModel.WhyOutOfScope == WhyOutOfScope.Under250
                ? viewModel.WhyOutOfScope.ToString()
                : viewModel.WhyOutOfScopeDetails;

            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            DateTime currentSnapshotDate = organisation.SectorType.GetAccountingStartDate();
            
            // Fix for right year
            organisation.OrganisationScopes.Where(o => o.SnapshotDate.Year == reportingYear).ForEach(s => s.Status = ScopeRowStatuses.Retired);
            
            organisation.OrganisationScopes.Add(
                new OrganisationScope {
                    OrganisationId = organisation.OrganisationId,
                    ScopeStatus = ScopeStatuses.OutOfScope,
                    StatusDetails = "Generated by the system",
                    ContactFirstname = currentUser.Firstname,
                    ContactLastname = currentUser.Lastname,
                    ContactEmailAddress = currentUser.EmailAddress,
                    Status = ScopeRowStatuses.Active,
                    SnapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear),
                    Reason = reasonForChange,
                    ReadGuidance = viewModel.HaveReadGuidance == HaveReadGuidance.HaveReadGuidance
                });
            organisationScope.ScopeStatus = ScopeStatuses.OutOfScope;
            organisationScope.ScopeStatusDate = DateTime.Now;
            
            dataRepository.SaveChangesAsync().Wait();
            
            // Send emails if scope changed on current reporting year
            if (viewModel.ReportingYear == currentSnapshotDate)
            {
                IEnumerable<string> emailAddressesForOrganisation = organisation.UserOrganisations.Select(uo => uo.User.EmailAddress);
                foreach (string emailAddress in emailAddressesForOrganisation)
                {
                    emailSendingService.SendScopeChangeOutEmail(emailAddress, organisation.OrganisationName);
                }
            }

            return RedirectToAction("ManageOrganisation", "Organisation", new { id = encryptedOrganisationId });
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
