using Microsoft.AspNetCore.Mvc;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using Microsoft.AspNetCore.Authorization;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-organisation")]
    public class AddOrganisationRedirectsController : Controller
    {

        [HttpGet("choose-sector")]
        public IActionResult ChooseSector()
        {
            return RedirectToActionPermanent("ChooseSector", "AddOrganisationChooseSector");
        }

        [HttpGet("confirmation")]
        public IActionResult Confirmation()
        {
            return RedirectToActionPermanent("Confirmation", "AddOrganisationConfirmation");
        }

        [HttpGet("found")]
        public IActionResult FoundGet(AddOrganisationFoundViewModel viewModel)
        {
            return RedirectToActionPermanent("FoundGet", "AddOrganisationFound");
        }

        [HttpPost("found")]
        [ValidateAntiForgeryToken]
        public IActionResult FoundPost(AddOrganisationFoundViewModel viewModel)
        {
            return RedirectToActionPermanent("FoundPost", "AddOrganisationFound");
        }

        [HttpGet("manual/address")]
        public IActionResult ManualAddress(AddOrganisationManualViewModel viewModel)
        {
            return RedirectToActionPermanent("ManualAddress", "AddOrganisationManualAddress");
        }

        [HttpGet("manual/choose-sector")]
        public IActionResult ManualChooseSector(AddOrganisationManualViewModel viewModel)
        {
            return RedirectToActionPermanent("ManualChooseSector", "AddOrganisationManualChooseSector");
        }

        [HttpGet("manual/confirm")]
        public IActionResult ManualConfirmGet(AddOrganisationManualViewModel viewModel)
        {
            return RedirectToActionPermanent("ManualConfirmGet", "AddOrganisationManualConfirm");
        }

        [HttpPost("manual/confirm")]
        [ValidateAntiForgeryToken]
        public IActionResult ManualConfirmPost(AddOrganisationManualViewModel viewModel)
        {
            return RedirectToActionPermanent("ManualConfirmPost", "AddOrganisationManualConfirm");
        }

        [HttpGet("manual/name")]
        public IActionResult ManualName(AddOrganisationManualViewModel viewModel)
        {
            return RedirectToActionPermanent("ManualName", "AddOrganisationManualName");
        }

        [HttpGet("manual/sic-codes")]
        public IActionResult ManualSicCodes(AddOrganisationManualViewModel viewModel)
        {
            return RedirectToActionPermanent("ManualSicCodes", "AddOrganisationManualSicCodes");
        }

        [HttpGet("{sector}/search")]
        public IActionResult Search(AddOrganisationSearchViewModel viewModel)
        {
            return RedirectToActionPermanent("Search", "AddOrganisationSearch");
        }
    }
}
