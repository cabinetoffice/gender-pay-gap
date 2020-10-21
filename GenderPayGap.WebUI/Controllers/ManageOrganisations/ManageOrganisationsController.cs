using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Organisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.ManageOrganisations
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("account/organisations")]
    public class ManageOrganisationsController : Controller
    {

        private readonly IDataRepository dataRepository;

        public ManageOrganisationsController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet]
        public IActionResult ManageOrganisationsGet()
        {
            // Check for feature flag and redirect if not enabled
            if (!FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.NewManageOrganisationsJourney))
            {
                return RedirectToAction("ManageOrganisations", "Organisation");
            }
            
            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            ControllerHelper.RedirectIfUserNeedsToReadPrivacyPolicy(User, user, Url);

            var viewModel = new ManageOrganisationsViewModel
            {
                UserOrganisations = user.UserOrganisations.OrderBy(uo => uo.Organisation.OrganisationName)
            };

            return View("ManageOrganisations", viewModel);

        }
        
        [HttpGet("{id}")]
        public IActionResult ManageOrganisationGet(string id)
        {
            // Check for feature flag and redirect if not enabled
            if (!FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.NewManageOrganisationsJourney))
            {
                return RedirectToAction("ManageOrganisation", "Organisation", new {id = id});
            }
            
            // Try to decrypt organisation id
            if (!id.DecryptToId(out long organisationId))
            {
                throw new PageNotFoundException();
            }

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            
            // Check the user has permission for this organisation
            UserOrganisation userOrganisation = user.UserOrganisations.FirstOrDefault(uo => uo.OrganisationId == organisationId);
            if (userOrganisation == null || userOrganisation.PINConfirmedDate == null)
            {
                throw new UserNotRegisteredToReportForOrganisationException();
            }
            
            // build the view model
            List<int> yearsWithDraftReturns =
                dataRepository.GetAll<DraftReturn>()
                    .Where(d => d.OrganisationId == organisationId)
                    .Select(d => d.SnapshotYear)
                    .ToList();

            var viewModel = new ManageOrganisationViewModel
            {
                Organisation = userOrganisation.Organisation,
                YearsWithDraftReturns = yearsWithDraftReturns,
                LoggedInUserId = user.UserId
            };

            return View("ManageOrganisation", viewModel);
        }

    }
}
