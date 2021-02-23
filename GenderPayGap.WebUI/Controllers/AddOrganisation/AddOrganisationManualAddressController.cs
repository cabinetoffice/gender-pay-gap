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
    [Route("add-employer")]
    public class AddOrganisationManualAddressController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AddOrganisationManualAddressController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet("manual/address")]
        public IActionResult ManualAddress(AddOrganisationManualViewModel viewModel)
        {
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            if (viewModel.Validate == true)
            {
                viewModel.ParseAndValidateParameters(Request, m => m.PoBox);
                viewModel.ParseAndValidateParameters(Request, m => m.Address1);
                viewModel.ParseAndValidateParameters(Request, m => m.Address2);
                viewModel.ParseAndValidateParameters(Request, m => m.Address3);
                viewModel.ParseAndValidateParameters(Request, m => m.TownCity);
                viewModel.ParseAndValidateParameters(Request, m => m.County);
                viewModel.ParseAndValidateParameters(Request, m => m.Country);
                viewModel.ParseAndValidateParameters(Request, m => m.PostCode);
                viewModel.ParseAndValidateParameters(Request, m => m.IsUkAddress);

                if (viewModel.HasAnyErrors())
                {
                    return View("ManualAddress", viewModel);
                }

                return ProceedToNextPage(viewModel);
            }

            return View("ManualAddress", viewModel);
        }

        private IActionResult ProceedToNextPage(AddOrganisationManualViewModel viewModel)
        {
            viewModel.Validate = null; // Required to prevent the next page immediately trying to validate the (empty) address
            if (viewModel.Editing == true)
            {
                // In the "Editing" flow, we go to each page, then back to the "Review" page
                viewModel.Editing = null; // To make the url look a bit nicer (the Review page implies we're editing so there's no need for "Editing" in the url)
                return RedirectToAction("ManualConfirmGet", "AddOrganisationManualConfirm", viewModel);
            }
            else
            {
                // In the normal "non-Editing" flow, we go to each page in turn
                if (viewModel.Sector.Value == AddOrganisationSector.Private)
                {
                    // For private-sector organisations, the next page is "SIC codes"
                    return RedirectToAction("ManualSicCodes", "AddOrganisationManualSicCodes", viewModel);
                }
                else
                {
                    // For public-sector organisations, the next page is "Review"
                    return RedirectToAction("ManualConfirmGet", "AddOrganisationManualConfirm", viewModel);
                }
            }
        }

    }
}
