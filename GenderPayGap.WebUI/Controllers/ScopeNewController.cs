using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Scope;
using GenderPayGap.WebUI.Services;
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

        [HttpGet("{encryptedOrganisationId}/reporting-year/{reportingYear}/change-scope")]
        public IActionResult ChangeOrganisationScope(string encryptedOrganisationId, int reportingYear, ScopingViewModel viewModel)
        {
            // Decrypt organisation ID param
            if (!encryptedOrganisationId.DecryptToParams(out List<string> requestParams))
            {
                return new HttpBadRequestResult($"Cannot decrypt request parameters '{encryptedOrganisationId}'");
            }
            long organisationId = requestParams[0].ToInt64();
            
            // Check user has permissions to access this page
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);
            ControllerHelper.ThrowIfUserDoesNotHavePermissionsForGivenOrganisation(User, dataRepository, organisationId);
            
            // Get the latest scope for the reporting year
            ScopeViewModel latestScope = null;
            
            if (viewModel.ThisScope.SnapshotDate.Year == reportingYear)
            {
                latestScope = viewModel.ThisScope;
            } else if (viewModel.LastScope.SnapshotDate.Year == reportingYear)
            {
                latestScope = viewModel.LastScope;
            }
            
            //Set the in/out journey type
            viewModel.IsOutOfScopeJourney = latestScope.ScopeStatus.IsAny(ScopeStatuses.PresumedInScope, ScopeStatuses.InScope);

            return RedirectToAction(viewModel.IsOutOfScopeJourney ? "EnterOutOfScopeAnswers" : "ConfirmInScope", "ScopeNew");
        }
    }
}
