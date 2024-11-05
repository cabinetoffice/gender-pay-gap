using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-employer")]
    public class AddOrganisationManualConfirmController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly OrganisationService organisationService;
        private readonly RegistrationRepository registrationRepository;

        public AddOrganisationManualConfirmController(
            IDataRepository dataRepository,
            OrganisationService organisationService,
            RegistrationRepository registrationRepository)
        {
            this.dataRepository = dataRepository;
            this.organisationService = organisationService;
            this.registrationRepository = registrationRepository;
        }


        [HttpGet("manual/confirm")]
        public IActionResult ManualConfirmGet(AddOrganisationManualViewModel viewModel)
        {
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            ValidateManuallyEnteredOrganisationDetails(viewModel);

            PopulateViewModel(viewModel);

            return View("ManualConfirm", viewModel);
        }

        [HttpPost("manual/confirm")]
        [ValidateAntiForgeryToken]
        public IActionResult ManualConfirmPost(AddOrganisationManualViewModel viewModel)
        {
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            ValidateManuallyEnteredOrganisationDetails(viewModel);

            if (!ModelState.IsValid)
            {
                return RedirectToAction("ManualConfirmGet", "AddOrganisationManualConfirm", viewModel);
            }

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            Organisation organisation = organisationService.CreateOrganisationFromManualDataEntry(
                viewModel.GetSectorType(),
                viewModel.OrganisationName,
                viewModel.PoBox,
                viewModel.Address1,
                viewModel.Address2,
                viewModel.Address3,
                viewModel.TownCity,
                viewModel.County,
                viewModel.Country,
                viewModel.PostCode,
                viewModel.GetIsUkAddressAsBoolean(),
                null, // TODO ASK USER FOR COMPANY NUMBER
                viewModel.SicCodes,
                user);

            UserOrganisation userOrganisation = registrationRepository.CreateRegistration(organisation, user, Url);

            return RedirectToConfirmationPage(userOrganisation);
        }

        private void ValidateManuallyEnteredOrganisationDetails(AddOrganisationManualViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.OrganisationName))
            {
                ModelState.AddModelError(nameof(viewModel.OrganisationName), "Enter the name of the organisation");
            }

            if (string.IsNullOrWhiteSpace(viewModel.Address1))
            {
                ModelState.AddModelError(nameof(viewModel.Address1), "Enter the registered address of the organisation");
            }

            if (!viewModel.IsUkAddress.HasValue)
            {
                ModelState.AddModelError(nameof(viewModel.IsUkAddress), "Select if this organisation's address is a UK address");
            }

            if (!viewModel.Sector.HasValue)
            {
                ModelState.AddModelError(nameof(viewModel.Sector), "Choose which type of organisation you would like to add");
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

        private IActionResult RedirectToConfirmationPage(UserOrganisation userOrganisation)
        {
            string confirmationId = $"{userOrganisation.UserId}:{userOrganisation.OrganisationId}";
            string encryptedConfirmationId = Encryption.EncryptQuerystring(confirmationId);
            return RedirectToAction("Confirmation", "AddOrganisationConfirmation", new { confirmationId = encryptedConfirmationId });
        }

    }
}
