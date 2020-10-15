using System.Linq;
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

            var viewModel = new ManageOrganisationsViewModel
            {
                UserOrganisations = user.UserOrganisations.OrderBy(uo => uo.Organisation.OrganisationName)
            };

            return View("ManageOrganisations", viewModel);

        }

    }
}
