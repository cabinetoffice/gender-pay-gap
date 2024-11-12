using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
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
    public class AddOrganisationFoundController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly CompaniesHouseAPI companiesHouseApi;
        private readonly OrganisationService organisationService;
        private readonly RegistrationRepository registrationRepository;

        public AddOrganisationFoundController(
            IDataRepository dataRepository,
            CompaniesHouseAPI companiesHouseApi,
            OrganisationService organisationService,
            RegistrationRepository registrationRepository)
        {
            this.dataRepository = dataRepository;
            this.companiesHouseApi = companiesHouseApi;
            this.organisationService = organisationService;
            this.registrationRepository = registrationRepository;
        }


        #region GET
        [HttpGet("found")]
        public IActionResult FoundGet(AddOrganisationFoundViewModel viewModel)
        {
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            ThrowIfNeitherIdNorCompanyNumberIsSpecified(viewModel);
            ThrowIfBothIdAndCompanyNumberAreSpecified(viewModel);

            if (viewModel.Id.HasValue)
            {
                return FoundGetWithId(viewModel);
            }
            else
            {
                return FoundGetWithCompanyNumber(viewModel);
            }
        }

        private IActionResult FoundGetWithId(AddOrganisationFoundViewModel viewModel)
        {
            Organisation organisation = dataRepository.Get<Organisation>(viewModel.Id.Value);

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            UserOrganisation existingUserOrganisation = organisation.UserOrganisations
                .Where(uo => uo.UserId == user.UserId)
                .FirstOrDefault();

            if (existingUserOrganisation != null)
            {
                AddOrganisationAlreadyRegisteringViewModel alreadyRegisteringViewModel =
                    CreateAlreadyRegisteringViewModel(viewModel, existingUserOrganisation);
                return View("AlreadyRegistering", alreadyRegisteringViewModel);
            }

            PopulateViewModelBasedOnOrganisation(viewModel, organisation);
            return View("Found", viewModel);
        }

        private IActionResult FoundGetWithCompanyNumber(AddOrganisationFoundViewModel viewModel)
        {
            Organisation existingOrganisation = dataRepository.GetAll<Organisation>()
                .Where(o => o.CompanyNumber == viewModel.CompanyNumber)
                .Where(o => o.Status == OrganisationStatuses.Active) // Only redirect the user to an Active organisation (i.e. don't do this for Retired or Deleted organisations)
                .FirstOrDefault();

            // We expect an ID for organisations that are in our database
            // We expect a Company Number for organisations that are not in our database (but are in the Companies House API)
            // If we've been given a Company Number, but the organisation IS in our database,
            //   then redirect to this same page, but using the correct ID
            if (existingOrganisation != null)
            {
                return RedirectToAction(
                    "FoundGet",
                    "AddOrganisationFound",
                    new
                    {
                        id = existingOrganisation.OrganisationId,
                        query = viewModel.Query,
                        sector = viewModel.Sector
                    });
            }

            PopulateViewModelBasedOnCompanyNumber(viewModel);
            return View("Found", viewModel);
        }
        #endregion GET


        #region POST
        [HttpPost("found")]
        [ValidateAntiForgeryToken]
        public IActionResult FoundPost(AddOrganisationFoundViewModel viewModel)
        {
            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            ThrowIfNeitherIdNorCompanyNumberIsSpecified(viewModel);
            ThrowIfBothIdAndCompanyNumberAreSpecified(viewModel);

            if (viewModel.Id.HasValue)
            {
                return FoundPostWithId(viewModel);
            }
            else
            {
                return FoundPostWithCompanyNumber(viewModel);
            }
        }

        private IActionResult FoundPostWithId(AddOrganisationFoundViewModel viewModel)
        {
            Organisation organisation = dataRepository.Get<Organisation>(viewModel.Id.Value);
            
            if (viewModel.IsUkAddress is null)
            {
                return RedirectToFoundPageWithUkAddressError(viewModel);
            }

            organisationService.UpdateIsUkAddressIfItIsNotAlreadySet(organisation.OrganisationId, viewModel.GetIsUkAddressAsBoolean());

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            UserOrganisation existingUserOrganisation = organisation.UserOrganisations
                .Where(uo => uo.UserId == user.UserId)
                .FirstOrDefault();

            if (existingUserOrganisation != null)
            {
                AddOrganisationAlreadyRegisteringViewModel alreadyRegisteringViewModel =
                    CreateAlreadyRegisteringViewModel(viewModel, existingUserOrganisation);
                return View("AlreadyRegistering", alreadyRegisteringViewModel);
            }

            UserOrganisation userOrganisation = registrationRepository.CreateRegistration(organisation, user, Url);

            return RedirectToConfirmationPage(userOrganisation);
        }

        private IActionResult FoundPostWithCompanyNumber(AddOrganisationFoundViewModel viewModel)
        {
            if (viewModel.IsUkAddress is null)
            {
                return RedirectToFoundPageWithUkAddressError(viewModel);
            }

            Organisation existingOrganisation = dataRepository.GetAll<Organisation>()
                .Where(o => o.CompanyNumber == viewModel.CompanyNumber)
                .Where(o => o.Status == OrganisationStatuses.Active) // Only redirect the user to an Active organisation (i.e. don't do this for Retired or Deleted organisations)
                .FirstOrDefault();

            // We expect an ID for organisations that are in our database
            // We expect a Company Number for organisations that are not in our database (but are in the Companies House API)
            // If we've been given a Company Number, but the organisation IS in our database,
            //   then redirect to this same page, but using the correct ID
            if (existingOrganisation != null)
            {
                return RedirectToAction(
                    "FoundGet",
                    "AddOrganisationFound",
                    new
                    {
                        id = existingOrganisation.OrganisationId,
                        query = viewModel.Query,
                        sector = viewModel.Sector
                    });
            }

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            Organisation organisation = organisationService.ImportOrganisationFromCompaniesHouse(viewModel.CompanyNumber, user);

            organisationService.UpdateIsUkAddressIfItIsNotAlreadySet(organisation.OrganisationId, viewModel.GetIsUkAddressAsBoolean());

            UserOrganisation userOrganisation = registrationRepository.CreateRegistration(organisation, user, Url);

            return RedirectToConfirmationPage(userOrganisation);
        }

        private IActionResult RedirectToConfirmationPage(UserOrganisation userOrganisation)
        {
            string confirmationId = $"{userOrganisation.UserId}:{userOrganisation.OrganisationId}";
            string encryptedConfirmationId = Encryption.EncryptString(confirmationId);
            return RedirectToAction("Confirmation", "AddOrganisationConfirmation", new { confirmationId = encryptedConfirmationId });
        }

        private IActionResult RedirectToFoundPageWithUkAddressError(AddOrganisationFoundViewModel viewModel)
        {
            ModelState.AddModelError(nameof(viewModel.IsUkAddress), "Select if this employer's registered address is a UK address");
            PopulateViewModelBasedOnCompanyNumber(viewModel);
            return View("Found", viewModel);
        }
        #endregion POST


        #region Methods to validate the view-models (used by both GET and POST requests)
        private static void ThrowIfNeitherIdNorCompanyNumberIsSpecified(AddOrganisationFoundViewModel viewModel)
        {
            if (!viewModel.Id.HasValue && string.IsNullOrWhiteSpace(viewModel.CompanyNumber))
            {
                // One of `id` or `companyNumber` must be specified, otherwise we can't show a valid page
                // The user possibly typed in the link incorrectly
                throw new PageNotFoundException();
            }
        }

        private static void ThrowIfBothIdAndCompanyNumberAreSpecified(AddOrganisationFoundViewModel viewModel)
        {
            if (viewModel.Id.HasValue && !string.IsNullOrWhiteSpace(viewModel.CompanyNumber))
            {
                // It is only valid to specify ONE of `id` or `companyNumber`
                throw new PageNotFoundException();
            }
        }
        #endregion

        #region Methods to populate the view-models (used by both GET and POST requests)
        private static void PopulateViewModelBasedOnOrganisation(AddOrganisationFoundViewModel viewModel, Organisation organisation)
        {
            // Name
            viewModel.Name = organisation.OrganisationName;

            // Address
            OrganisationAddress organisationAddress = organisation.GetLatestAddress();
            string addressString = organisationAddress?.GetAddressString() ?? "";
            viewModel.AddressLines = addressString.Split(",").ToList();

            // IsUkAddress
            bool? isUkAddress = organisationAddress?.IsUkAddress;
            if (isUkAddress.HasValue)
            {
                viewModel.IsUkAddress = isUkAddress.Value
                    ? AddOrganisationIsUkAddress.Yes
                    : AddOrganisationIsUkAddress.No;
            }
            else
            {
                viewModel.IsUkAddress = PostcodesIoApi.IsValidPostcode(organisationAddress?.GetPostCodeInAllCaps())
                    ? AddOrganisationIsUkAddress.Yes
                    : (AddOrganisationIsUkAddress?) null;
            }

            // Company number
            if (!string.IsNullOrWhiteSpace(organisation.CompanyNumber))
            {
                viewModel.CompanyNumber = organisation.CompanyNumber;
            }
        }

        private void PopulateViewModelBasedOnCompanyNumber(AddOrganisationFoundViewModel viewModel)
        {
            CompaniesHouseCompany organisationFromCompaniesHouse = companiesHouseApi.GetCompany(viewModel.CompanyNumber);

            // Name
            viewModel.Name = organisationFromCompaniesHouse.CompanyName;

            // Address
            CompaniesHouseAddress coHoAddress = organisationFromCompaniesHouse.RegisteredOfficeAddress;
            OrganisationAddress organisationAddress = UpdateFromCompaniesHouseService.CreateOrganisationAddressFromCompaniesHouseAddress(coHoAddress);
            string addressString = organisationAddress?.GetAddressString() ?? "";
            viewModel.AddressLines = addressString.Split(",").ToList();

            // IsUkAddress
            string postCode = organisationFromCompaniesHouse.RegisteredOfficeAddress.PostalCode;
            viewModel.IsUkAddress = PostcodesIoApi.IsValidPostcode(postCode)
                ? AddOrganisationIsUkAddress.Yes
                : (AddOrganisationIsUkAddress?) null;
        }

        private AddOrganisationAlreadyRegisteringViewModel CreateAlreadyRegisteringViewModel(
            AddOrganisationFoundViewModel foundViewModel,
            UserOrganisation existingUserOrganisation)
        {
            var alreadyRegisteringViewModel = new AddOrganisationAlreadyRegisteringViewModel
            {
                Id = foundViewModel.Id.Value,
                Query = foundViewModel.Query
            };

            // UserOrganisation
            alreadyRegisteringViewModel.ExistingUserOrganisation = existingUserOrganisation;

            return alreadyRegisteringViewModel;
        }
        #endregion
    }
}
