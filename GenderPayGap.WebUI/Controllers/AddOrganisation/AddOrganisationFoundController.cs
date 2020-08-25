using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.AddOrganisation;
using GovUkDesignSystem.Parsers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.AddOrganisation
{
    [Authorize(Roles = LoginRoles.GpgEmployer)]
    [Route("add-organisation")]
    public class AddOrganisationFoundController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly ICompaniesHouseAPI companiesHouseApi;
        private readonly IObfuscator obfuscator;

        public AddOrganisationFoundController(
            IDataRepository dataRepository,
            ICompaniesHouseAPI companiesHouseApi,
            IObfuscator obfuscator)
        {
            this.dataRepository = dataRepository;
            this.companiesHouseApi = companiesHouseApi;
            this.obfuscator = obfuscator;
        }


        #region GET
        [HttpGet("found")]
        public IActionResult FoundGet(AddOrganisationFoundViewModel viewModel)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewAddOrganisationJourney);

            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            ThrowIfNeitherIdNorCompanyNumberIsSpecified(viewModel);
            ThrowIfBothIdAndCompanyNumberAreSpecified(viewModel);

            if (!string.IsNullOrWhiteSpace(viewModel.Id))
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
            long organisationId = obfuscator.DeObfuscate(viewModel.Id);
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            User user = ControllerHelper.GetGpgUserFromAspNetUser(User, dataRepository);

            UserOrganisation existingUserOrganisation = organisation.UserOrganisations
                .Where(uo => uo.UserId == user.UserId)
                .FirstOrDefault();

            if (existingUserOrganisation == null)
            {
                PopulateViewModelBasedOnOrganisation(viewModel, organisation);
                return View("Found", viewModel);
            }

            PopulateViewModelBasedOnExistingUserOrganisation(viewModel, existingUserOrganisation);
            return View("AlreadyRegistering", viewModel);
        }

        private IActionResult FoundGetWithCompanyNumber(AddOrganisationFoundViewModel viewModel)
        {
            Organisation existingOrganisation = dataRepository.GetAll<Organisation>()
                .Where(o => o.CompanyNumber == viewModel.CompanyNumber)
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
                        id = existingOrganisation.GetEncryptedId(),
                        query = viewModel.Query
                    });
            }

            PopulateViewModelBasedOnCompanyNumber(viewModel);
            return View("Found", viewModel);
        }
        #endregion GET


        #region POST
        [HttpPost("found")]
        public IActionResult FoundPost(AddOrganisationFoundViewModel viewModel)
        {
            ControllerHelper.Throw404IfFeatureDisabled(FeatureFlag.NewAddOrganisationJourney);

            ControllerHelper.ThrowIfUserAccountRetiredOrEmailNotVerified(User, dataRepository);

            ThrowIfNeitherIdNorCompanyNumberIsSpecified(viewModel);
            ThrowIfBothIdAndCompanyNumberAreSpecified(viewModel);

            if (!string.IsNullOrWhiteSpace(viewModel.Id))
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
            long organisationId = obfuscator.DeObfuscate(viewModel.Id);
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            // If the value hasn't been bound, Parse And Validate it
            if (!viewModel.IsUkAddress.HasValue)
            {
                viewModel.ParseAndValidateParameters(Request, m => m.IsUkAddress);
            }

            // If it still doesn't has a value on, then show an error
            if (!viewModel.IsUkAddress.HasValue)
            {
                PopulateViewModelBasedOnOrganisation(viewModel, organisation);
                return View("Found", viewModel);
            }

            throw new System.NotImplementedException();
        }

        private IActionResult FoundPostWithCompanyNumber(AddOrganisationFoundViewModel viewModel)
        {
            // If the value hasn't been bound, Parse And Validate it
            if (!viewModel.IsUkAddress.HasValue)
            {
                viewModel.ParseAndValidateParameters(Request, m => m.IsUkAddress);
            }

            // If it still doesn't has a value on, then show an error
            if (!viewModel.IsUkAddress.HasValue)
            {
                PopulateViewModelBasedOnCompanyNumber(viewModel);
                return View("Found", viewModel);
            }

            throw new System.NotImplementedException();
        }
        #endregion POST


        #region Methods to validate the view-models (used by both GET and POST requests)
        private static void ThrowIfNeitherIdNorCompanyNumberIsSpecified(AddOrganisationFoundViewModel viewModel)
        {
            if (string.IsNullOrWhiteSpace(viewModel.Id) && string.IsNullOrWhiteSpace(viewModel.CompanyNumber))
            {
                // One of `id` or `companyNumber` must be specified, otherwise we can't show a valid page
                // The user possibly typed in the link incorrectly
                throw new PageNotFoundException();
            }
        }

        private static void ThrowIfBothIdAndCompanyNumberAreSpecified(AddOrganisationFoundViewModel viewModel)
        {
            if (!string.IsNullOrWhiteSpace(viewModel.Id) && !string.IsNullOrWhiteSpace(viewModel.CompanyNumber))
            {
                // It is only valid to specify ONE of `id` or `companyNumber`
                throw new PageNotFoundException();
            }
        }
        #endregion

        #region Methods to populate the view-models (used by both GET and POST requests)
        private static void PopulateViewModelBasedOnOrganisation(AddOrganisationFoundViewModel viewModel, Organisation organisation)
        {
            // Sector
            viewModel.Sector = organisation.SectorType == SectorTypes.Public
                ? AddOrganisationSector.Public
                : AddOrganisationSector.Private;

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
                    ? AddOrganisationFoundViewModelIsUkAddress.Yes
                    : AddOrganisationFoundViewModelIsUkAddress.No;
            }
            else
            {
                viewModel.IsUkAddress = PostcodesIoApi.IsValidPostcode(organisationAddress?.PostCode).Result
                    ? AddOrganisationFoundViewModelIsUkAddress.Yes
                    : (AddOrganisationFoundViewModelIsUkAddress?) null;
            }

            // SicCodes
            viewModel.SicCodes = organisation.OrganisationSicCodes.Select(osc => osc.SicCode).ToList();
        }

        private void PopulateViewModelBasedOnCompanyNumber(AddOrganisationFoundViewModel viewModel)
        {
            CompaniesHouseCompany organisationFromCompaniesHouse = companiesHouseApi.GetCompanyAsync(viewModel.CompanyNumber).Result;

            // Sector
            viewModel.Sector = AddOrganisationSector.Private;

            // Name
            viewModel.Name = organisationFromCompaniesHouse.CompanyName;

            // Address
            CompaniesHouseAddress coHoAddress = organisationFromCompaniesHouse.RegisteredOfficeAddress;
            OrganisationAddress organisationAddress = UpdateFromCompaniesHouseService.CreateOrganisationAddressFromCompaniesHouseAddress(coHoAddress);
            string addressString = organisationAddress?.GetAddressString() ?? "";
            viewModel.AddressLines = addressString.Split(",").ToList();

            // IsUkAddress
            string postCode = organisationFromCompaniesHouse.RegisteredOfficeAddress.PostalCode;
            viewModel.IsUkAddress = PostcodesIoApi.IsValidPostcode(postCode).Result
                ? AddOrganisationFoundViewModelIsUkAddress.Yes
                : (AddOrganisationFoundViewModelIsUkAddress?) null;

            // SicCodes
            List<string> sicCodesFromCompaniesHouse = organisationFromCompaniesHouse.SicCodes ?? new List<string>();
            viewModel.SicCodes = dataRepository.GetAll<SicCode>()
                .Where(sicCode => sicCodesFromCompaniesHouse.Contains(sicCode.SicCodeId.ToString()))
                .ToList();
        }

        private void PopulateViewModelBasedOnExistingUserOrganisation(
            AddOrganisationFoundViewModel viewModel,
            UserOrganisation existingUserOrganisation)
        {
            // Sector
            viewModel.Sector = existingUserOrganisation.Organisation.SectorType == SectorTypes.Public
                ? AddOrganisationSector.Public
                : AddOrganisationSector.Private;

            // UserOrganisation
            viewModel.ExistingUserOrganisation = existingUserOrganisation;
        }
        #endregion
    }
}
