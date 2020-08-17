using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-organisation")]
    public class AddOrganisationChooseSectorController : Controller
    {

        [HttpGet("choose-sector")]
        public IActionResult ChooseSector(AddOrganisationChooseSectorViewModel viewModel)
        {
            if (viewModel.Validate == true)
            {
                viewModel.ParseAndValidateParameters(Request, m => m.Sector);

                if (viewModel.HasAnyErrors())
                {
                    return View("ChooseSector", viewModel);
                }

                return RedirectToAction("Search", "AddOrganisationSearch",
                    new { Sector = viewModel.Sector.ToString().ToLower() });
            }

            return View("ChooseSector", viewModel);
        }

    }
}
