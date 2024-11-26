using System;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin + "," + LoginRoles.GpgAdminReadOnly)]
    [Route("admin")]
    public class AdminOrganisationAddressController : Controller
    {
        private readonly IDataRepository dataRepository;
        private readonly CompaniesHouseAPI companiesHouseApi;
        private readonly AuditLogger auditLogger;

        public AdminOrganisationAddressController(
            IDataRepository dataRepository,
            CompaniesHouseAPI companiesHouseApi,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.companiesHouseApi = companiesHouseApi;
            this.auditLogger = auditLogger;
        }

        [HttpGet("organisation/{id}/address")]
        public IActionResult ViewAddressHistory(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            return View("ViewOrganisationAddress", organisation);
        }

        [Authorize(Roles = LoginRoles.GpgAdmin)]
        [HttpGet("organisation/{id}/address/change")]
        public IActionResult ChangeAddressGet(long id)
        {
            Organisation organisation = dataRepository.Get<Organisation>(id);

            if (!string.IsNullOrWhiteSpace(organisation.CompanyNumber))
            {
                try
                {
                    CompaniesHouseCompany organisationFromCompaniesHouse =
                        companiesHouseApi.GetCompany(organisation.CompanyNumber);

                    OrganisationAddress addressFromCompaniesHouse =
                        UpdateFromCompaniesHouseService.CreateOrganisationAddressFromCompaniesHouseAddress(
                        organisationFromCompaniesHouse.RegisteredOfficeAddress);

                    OrganisationAddress latestAddress = organisation.GetLatestAddress();
                    if (latestAddress != null && !latestAddress.AddressMatches(addressFromCompaniesHouse))
                    {
                        return OfferNewCompaniesHouseAddress(organisation, addressFromCompaniesHouse);
                    }

                }
                catch (Exception ex)
                {
                    // Use Manual Change page instead
                    CustomLogger.Warning("Error from Companies House API", ex);
                }
            }

            // In all other cases...
            // * Organisation doesn't have a Companies House number
            // * CoHo API returns an error
            // * CoHo address matches Organisation address
            // ... send to the Manual Change page
            return SendToManualChangePage(organisation);
        }

        private IActionResult OfferNewCompaniesHouseAddress(Organisation organisation, OrganisationAddress addressFromCompaniesHouse)
        {
            var viewModel = new ChangeOrganisationAddressViewModel {
                Organisation = organisation,
                Action = ManuallyChangeOrganisationAddressViewModelActions.OfferNewCompaniesHouseAddress
            };

            viewModel.PopulateFromOrganisationAddress(addressFromCompaniesHouse);

            return View("OfferNewCompaniesHouseAddress", viewModel);
        }

        private IActionResult SendToManualChangePage(Organisation organisation)
        {
            OrganisationAddress address = organisation.OrganisationAddresses.OrderByDescending(a => a.Created).FirstOrDefault();

            var viewModel = new ChangeOrganisationAddressViewModel
            {
                Organisation = organisation,
                Action = ManuallyChangeOrganisationAddressViewModelActions.ManualChange
            };

            viewModel.PopulateFromOrganisationAddress(address);

            return View("ManuallyChangeOrganisationAddress", viewModel);
        }

        [ValidateAntiForgeryToken]
        [Authorize(Roles = LoginRoles.GpgAdmin)]
        [HttpPost("organisation/{id}/address/change")]
        public IActionResult ChangeAddressPost(long id, ChangeOrganisationAddressViewModel viewModel)
        {
            // We might need to change the value of Action before we go to the view
            // Apparently this is necessary
            // https://stackoverflow.com/questions/4837744/hiddenfor-not-getting-correct-value-from-view-model
            ModelState.Remove(nameof(viewModel.Action));

            Organisation organisation = dataRepository.Get<Organisation>(id);

            switch (viewModel.Action) {
                case ManuallyChangeOrganisationAddressViewModelActions.OfferNewCompaniesHouseAddress:
                    return OfferNewCompaniesHouseAction(viewModel, organisation);

                case ManuallyChangeOrganisationAddressViewModelActions.ManualChange:
                    return ManualChangeAction(viewModel, organisation);

                case ManuallyChangeOrganisationAddressViewModelActions.CheckChangesManual:
                case ManuallyChangeOrganisationAddressViewModelActions.CheckChangesCoHo:
                    return CheckChangesAction(viewModel, organisation);

                default:
                    throw new ArgumentException("Unknown action in AdminOrganisationAddressController.ChangeAddressPost");
            }
        }

        private IActionResult OfferNewCompaniesHouseAction(ChangeOrganisationAddressViewModel viewModel, Organisation organisation)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationAddressViewModelActions.OfferNewCompaniesHouseAddress;
                return View("OfferNewCompaniesHouseAddress", viewModel);
            }

            if (viewModel.AcceptCompaniesHouseAddress == AcceptCompaniesHouseAddress.Reject)
            {
                return SendToManualChangePage(organisation);
            }

            viewModel.Organisation = organisation;
            viewModel.Action = ManuallyChangeOrganisationAddressViewModelActions.CheckChangesCoHo;
            return View("ConfirmAddressChange", viewModel);
        }

        private IActionResult ManualChangeAction(ChangeOrganisationAddressViewModel viewModel, Organisation organisation)
        {
            if (!ModelState.IsValid)
            {
                viewModel.Organisation = organisation;
                viewModel.Action = ManuallyChangeOrganisationAddressViewModelActions.ManualChange;
                return View("ManuallyChangeOrganisationAddress", viewModel);
            }

            viewModel.Organisation = organisation;
            viewModel.Action = ManuallyChangeOrganisationAddressViewModelActions.CheckChangesManual;
            return View("ConfirmAddressChange", viewModel);
        }

        private IActionResult CheckChangesAction(ChangeOrganisationAddressViewModel viewModel, Organisation organisation)
        {
            if (viewModel.Action == ManuallyChangeOrganisationAddressViewModelActions.CheckChangesManual)
            {
                OptOrganisationOutOfCompaniesHouseUpdates(organisation);
            }

            SaveChangesAndAuditAction(viewModel, organisation);

            return View("SuccessfullyChangedOrganisationAddress", organisation);
        }

        private void SaveChangesAndAuditAction(ChangeOrganisationAddressViewModel viewModel, Organisation organisation)
        {
            string oldAddressString = organisation.GetLatestAddress()?.GetAddressString();

            if (oldAddressString != null)
            {
                RetireOldAddress(organisation);
            }

            OrganisationAddress newOrganisationAddress = CreateOrganisationAddressFromViewModel(viewModel);
            AddNewAddressToOrganisation(newOrganisationAddress, organisation);

            dataRepository.SaveChanges();

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.AdminChangeOrganisationAddress,
                organisation,
                new
                {
                    Action = viewModel.Action,
                    OldAddress = oldAddressString,
                    NewAddress = newOrganisationAddress.GetAddressString(),
                    NewAddressId = newOrganisationAddress.AddressId,
                    Reason = viewModel.Reason
                },
                User);
        }

        private static void RetireOldAddress(Organisation organisation)
        {
            OrganisationAddress oldOrganisationAddress = organisation.GetLatestAddress();
            oldOrganisationAddress.Status = AddressStatuses.Retired;
            oldOrganisationAddress.StatusDate = VirtualDateTime.Now;
        }

        private OrganisationAddress CreateOrganisationAddressFromViewModel(ChangeOrganisationAddressViewModel viewModel)
        {
            var organisationAddress = new OrganisationAddress {
                PoBox = viewModel.PoBox,
                Address1 = viewModel.Address1,
                Address2 = viewModel.Address2,
                Address3 = viewModel.Address3,
                TownCity = viewModel.TownCity,
                County = viewModel.County,
                Country = viewModel.Country,
                PostCode = viewModel.PostCode,
                Status = AddressStatuses.Active,
                StatusDate = VirtualDateTime.Now,
                StatusDetails = viewModel.Reason,
                Created = VirtualDateTime.Now,
                Source = "Service Desk",
            };

            if (viewModel.IsUkAddress.HasValue)
            {
                organisationAddress.IsUkAddress = (viewModel.IsUkAddress == ManuallyChangeOrganisationAddressIsUkAddress.Yes);
            }

            return organisationAddress;
        }

        private void AddNewAddressToOrganisation(OrganisationAddress organisationAddress, Organisation organisation)
        {
            organisationAddress.OrganisationId = organisation.OrganisationId;
            organisation.OrganisationAddresses.Add(organisationAddress);

            dataRepository.Insert(organisationAddress);
        }

        private void OptOrganisationOutOfCompaniesHouseUpdates(Organisation organisation)
        {
            organisation.OptedOutFromCompaniesHouseUpdate = true;
            dataRepository.SaveChanges();
        }

    }
}
