using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using GovUkDesignSystem;
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

            ValidateManuallyEnteredOrganisationDetails(viewModel);

            PopulateViewModel(viewModel);

            return View("ManualConfirm", viewModel);
        }

        private static void ValidateManuallyEnteredOrganisationDetails(AddOrganisationManualViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.OrganisationName))
            {
                viewModel.AddErrorFor(m => m.OrganisationName, "Enter the name of the organisation");
            }

            if (string.IsNullOrWhiteSpace(viewModel.Address1))
            {
                viewModel.AddErrorFor(m => m.Address1, "Enter the registered address of the organisation");
            }

            if (!viewModel.IsUkAddress.HasValue)
            {
                viewModel.AddErrorFor(m => m.IsUkAddress, "Select if this organisation's address is a UK address");
            }

            if (!viewModel.Sector.HasValue)
            {
                viewModel.AddErrorFor(m => m.Sector, "Choose which type of organisation you would like to add");
            }
        }

        private void PopulateViewModel(AddOrganisationManualViewModel viewModel)
        {
            if (viewModel.SicCodes == null)
            {
                viewModel.SicCodes = new List<int>();
            }

            viewModel.SelectedSicCodes = dataRepository.GetAll<SicCode>()
                .Where(sc => viewModel.SicCodes.Contains(sc.SicCodeId))
                .ToList();
        }

    }
}
