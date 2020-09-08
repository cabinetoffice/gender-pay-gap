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
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("organisation/{encryptedOrganisationId}/reporting-year/{reportingYear}/change-scope")]
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
        [HttpGet]
        public IActionResult ChangeOrganisationScope(string encryptedOrganisationId, int reportingYear)
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

            // Get Organisation and OrganisationScope for reporting year
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);
            OrganisationScope organisationScope = organisation.OrganisationScopes.FirstOrDefault(s => s.SnapshotDate.Year == reportingYear);

            return RedirectToAction(organisationScope.IsInScopeVariant()  ? "EnterOutOfScopeAnswers" : "ConfirmInScope", "ScopeNew");
        }

        [HttpGet("out")]
        public IActionResult EnterOutOfScopeAnswers()
        {
            // When User is Admin then redirect to Admin\Home
            User currentUser = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            if (currentUser != null && currentUser.IsAdministrator())
            {
                return RedirectToAction("Home", "Admin");
            }
            
            return View("EnterOutOfScopeAnswers");
        }

        [PreventDuplicatePost]
        [ValidateAntiForgeryToken]
        [HttpPost("{encryptedOrganisationId}/reporting-year/{reportingYear}/change-scope/out")]
        public IActionResult EnterOutOfScopeAnswers(EnterAnswersViewModel enterAnswersModel)
        {
            return null;
        }
    }
}
