using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Organisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.ManageOrganisations
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("manage-organisations-new")]
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
            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);
            
            // Show the privacy policy to non-admin users (if they're not being impersonated) if they haven't read it yet
            if (!LoginHelper.IsUserBeingImpersonated(User) && !user.IsAdministrator())
            {
                DateTime? hasReadPrivacy = user.AcceptedPrivacyStatement;
                if (hasReadPrivacy == null || hasReadPrivacy.Value < Global.PrivacyChangedDate)
                {
                    return RedirectToAction("PrivacyPolicyGet", "PrivacyPolicy");
                }
            }

            var viewModel = new ManageOrganisationsViewModel
            {
                UserOrganisations = user.UserOrganisations.OrderBy(uo => uo.Organisation.OrganisationName)
            };

            return View("ManageOrganisations", viewModel);

        }

    }
}
