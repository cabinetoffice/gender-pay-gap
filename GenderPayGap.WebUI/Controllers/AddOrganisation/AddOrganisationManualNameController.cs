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
    public class AddOrganisationManualNameController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AddOrganisationManualNameController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet("manual/name")]
        public IActionResult ManualName(AddOrganisationManualViewModel viewModel)
        {
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            if (viewModel.Validate == true)
            {
                viewModel.ParseAndValidateParameters(Request, m => m.OrganisationName);

                if (viewModel.HasAnyErrors())
                {
                    return View("ManualName", viewModel);
                }

                return ProceedToNextPage(viewModel);
            }

            return View("ManualName", viewModel);
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
                // In the normal "non-Editing" flow, we go to each page in turn - the next page here is the Address page
                return RedirectToAction("ManualAddress", "AddOrganisationManualAddress", viewModel);
            }
        }

    }
}
