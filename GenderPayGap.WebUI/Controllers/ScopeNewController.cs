using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
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
            // Decrypt organisation ID param
            if (!encryptedOrganisationId.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt request parameters '{encryptedOrganisationId}'");
            }

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
            // Decrypt organisation ID param
            if (!encryptedOrganisationId.DecryptToId(out long organisationId))
            {
                return new HttpBadRequestResult($"Cannot decrypt request parameters '{encryptedOrganisationId}'");
            }
            
            viewModel.ParseAndValidateParameters(Request, m => m.WhyOutOfScope);
            viewModel.ParseAndValidateParameters(Request, m => m.HaveReadGuidance);

            if (viewModel.WhyOutOfScope == WhyOutOfScope.Other )
            {
                viewModel.ParseAndValidateParameters(Request, m => m.WhyOutOfScopeDetails);
            }
            
            if (viewModel.HasAnyErrors())
            {
                // Get Organisation and OrganisationScope for reporting year
                Organisation organisation = dataRepository.Get<Organisation>(organisationId);
                OrganisationScope organisationScope = organisation.OrganisationScopes.FirstOrDefault(s => s.SnapshotDate.Year == reportingYear);

                viewModel.Organisation = organisation;
                viewModel.ReportingYear = organisationScope.SnapshotDate;
                
                return View("OutOfScopeQuestions", viewModel);
            }
            return View("ConfirmOutOfScope", viewModel);
        }
    }
}
