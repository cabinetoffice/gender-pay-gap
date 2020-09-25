using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-organisation")]
    public class AddOrganisationManualChooseSectorController : Controller
    {
        
        private readonly IDataRepository dataRepository;

        public AddOrganisationManualChooseSectorController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet("manual/choose-sector")]
        public IActionResult ManualChooseSector(AddOrganisationManualViewModel viewModel)
        {
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            if (viewModel.Validate == true)
            {
                viewModel.ParseAndValidateParameters(Request, m => m.Sector);

                if (viewModel.HasAnyErrors())
                {
                    return View("ManualChooseSector", viewModel);
                }

                return ProceedToNextPage(viewModel);
            }

            return View("ManualChooseSector", viewModel);
        }

        private IActionResult ProceedToNextPage(AddOrganisationManualViewModel viewModel)
        {
            viewModel.Validate = null; // Required to prevent the next page immediately trying to validate the (empty) address
            viewModel.Editing = null; // To make the url look a bit nicer (the Review page implies we're editing so there's no need for "Editing" in the url)

            if (viewModel.Sector.Value == AddOrganisationSector.Private &&
                (viewModel.SicCodes == null || viewModel.SicCodes.Count == 0))
            {
                // If the user has selected "Private" and if they haven't selected any SIC codes, then we should prompt them to do this
                return RedirectToAction("ManualSicCodes", "AddOrganisationManualSicCodes", viewModel);
            }
            else
            {
                return RedirectToAction("ManualConfirmGet", "AddOrganisationManualConfirm", viewModel);
            }
        }

    }
}
