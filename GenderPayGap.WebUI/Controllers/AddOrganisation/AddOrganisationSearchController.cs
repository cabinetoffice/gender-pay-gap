using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-organisation")]
    public class AddOrganisationSearchController : Controller
    {

        [HttpGet("{sector}/search")]
        public IActionResult Search(AddOrganisationSearchViewModel viewModel)
        {
            return View("Search", viewModel);
        }

    }
}
