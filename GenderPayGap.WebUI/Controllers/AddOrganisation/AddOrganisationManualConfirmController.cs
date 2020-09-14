using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-organisation")]
    public class AddOrganisationManualConfirmController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AddOrganisationManualConfirmController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        [HttpGet("manual/confirm")]
        public IActionResult ManualConfirmGet(AddOrganisationManualViewModel viewModel)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewAddOrganisationJourney);

            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            if (viewModel.Validate == true)
            {
                viewModel.Validate = null; // Required to prevent the next page immediately trying to validate the form
                return RedirectToAction("ManualSicCodes", "AddOrganisationManualSicCodes", viewModel);
            }

            if (viewModel.SicCodes == null)
            {
                viewModel.SicCodes = new List<int>();
            }
            viewModel.SelectedSicCodes = dataRepository.GetAll<SicCode>()
                .Where(sc => viewModel.SicCodes.Contains(sc.SicCodeId))
                .ToList();

            viewModel.Editing = true;
            return View("ManualConfirm", viewModel);
        }

    }
}
