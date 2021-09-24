using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;

namespace GenderPayGap.WebUI.BusinessLogic.Services
{
    public class UpdateFromCompaniesHouseService
    {

        private const string SourceOfChange = "CoHo";
        private const string DetailsOfChange = "Replaced by CoHo";

        private readonly ICompaniesHouseAPI companiesHouseApi;
        private readonly IDataRepository dataRepository;

        public UpdateFromCompaniesHouseService(IDataRepository dataRepository, ICompaniesHouseAPI companiesHouseApi)
        {
            this.dataRepository = dataRepository;
            this.companiesHouseApi = companiesHouseApi;
        }

        public void UpdateOrganisationDetails(long organisationId)
        {
            CustomLogger.Debug($"Loading organisation - OrganisationId({organisationId})");
            var organisation = dataRepository.Get<Organisation>(organisationId);

            CustomLogger.Debug($"Updating LastCheckedAgainstCompaniesHouse - OrganisationId({organisationId})");
            organisation.LastCheckedAgainstCompaniesHouse = VirtualDateTime.Now;
            dataRepository.SaveChanges();

            try
            {
                CustomLogger.Debug($"Calling CoHo API - OrganisationId({organisationId})");
                CompaniesHouseCompany organisationFromCompaniesHouse =
                    companiesHouseApi.GetCompany(organisation.CompanyNumber);

                try
                {
                    CustomLogger.Debug($"Updating SIC codes - OrganisationId({organisationId})");
                    UpdateSicCode(organisation, organisationFromCompaniesHouse);

                    CustomLogger.Debug($"Updating Address - OrganisationId({organisationId})");
                    UpdateAddress(organisation, organisationFromCompaniesHouse);

                    CustomLogger.Debug($"Updating Name - OrganisationId({organisationId})");
                    UpdateName(organisation, organisationFromCompaniesHouse);

                    CustomLogger.Debug($"Saving - OrganisationId({organisationId})");
                    dataRepository.SaveChanges();

                    CustomLogger.Debug($"Saved - OrganisationId({organisationId})");
                }
                catch (Exception ex)
                {
                    string message =
                        $"Update from Companies House: Failed to update database, organisation id = {organisationId}";
                    CustomLogger.Error(message, ex);
                }

            }
            catch (Exception ex)
            {
                string message =
                    $"Update from Companies House: Failed to get company data from companies house, organisation id = {organisationId}";
                CustomLogger.Error(message, ex);
            }
        }

        public void UpdateSicCode(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            List<string> companySicCodes = organisationFromCompaniesHouse.SicCodes ?? new List<string>();
            RetireExtraSicCodes(organisation, companySicCodes);
            AddNewSicCodes(organisation, companySicCodes);
        }

        private void RetireExtraSicCodes(Organisation organisation, List<string> companySicCodes)
        {
            IEnumerable<int> sicCodeIds = organisation.GetSicCodes().Select(sicCode => sicCode.SicCodeId);
            IEnumerable<int> newSicCodeIds =
                companySicCodes.Where(sicCode => !sicCode.IsNullOrEmpty()).Select(sicCode => int.Parse(sicCode));

            IEnumerable<int> idsToBeRetired = sicCodeIds.Except(newSicCodeIds);
            IEnumerable<OrganisationSicCode> sicCodesToBeRetired =
                organisation.OrganisationSicCodes.Where(s => idsToBeRetired.Contains(s.SicCodeId));
            foreach (OrganisationSicCode sicCodeToBeRetired in sicCodesToBeRetired)
            {
                sicCodeToBeRetired.Retired = VirtualDateTime.Now;
            }
        }

        private void AddNewSicCodes(Organisation organisation, List<string> companySicCodes)
        {
            IEnumerable<int> sicCodeIds = organisation.GetSicCodes().Select(sicCode => sicCode.SicCodeId);
            IEnumerable<int> newSicCodeIds =
                companySicCodes.Where(sicCode => !sicCode.IsNullOrEmpty()).Select(sicCode => int.Parse(sicCode));

            IEnumerable<int> idsToBeAdded = newSicCodeIds.Except(sicCodeIds);
            foreach (int sicCodeId in idsToBeAdded)
            {
                if (dataRepository.GetAll<SicCode>().Any(sicCode => sicCode.SicCodeId == sicCodeId))
                {
                    var sicCodeToBeAdded = new OrganisationSicCode {
                        Organisation = organisation, SicCodeId = sicCodeId, Source = SourceOfChange, Created = VirtualDateTime.Now
                    };
                    organisation.OrganisationSicCodes.Add(sicCodeToBeAdded);
                    dataRepository.Insert(sicCodeToBeAdded);
                }
            }
        }

        public void UpdateAddress(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            CompaniesHouseAddress companiesHouseAddress = organisationFromCompaniesHouse.RegisteredOfficeAddress;
            OrganisationAddress newOrganisationAddressFromCompaniesHouse =
                CreateOrganisationAddressFromCompaniesHouseAddress(companiesHouseAddress);
            OrganisationAddress oldOrganisationAddress = organisation.GetLatestAddress();
            if (oldOrganisationAddress != null)
            {
                if (oldOrganisationAddress.AddressMatches(newOrganisationAddressFromCompaniesHouse)
                    || IsNewOrganisationAddressNullOrEmpty(newOrganisationAddressFromCompaniesHouse))
                {
                    return;
                }
                oldOrganisationAddress.Status = AddressStatuses.Retired;
                oldOrganisationAddress.StatusDate = VirtualDateTime.Now;
            }

            newOrganisationAddressFromCompaniesHouse.OrganisationId = organisation.OrganisationId;
            organisation.OrganisationAddresses.Add(newOrganisationAddressFromCompaniesHouse);

            dataRepository.Insert(newOrganisationAddressFromCompaniesHouse);
        }

        private static bool IsNewOrganisationAddressNullOrEmpty(OrganisationAddress address)
        {
            // Some organisations are not required to provide information to Companies House, and so we might get an empty
            // address. See https://wck2.companieshouse.gov.uk/goWCK/help/en/stdwc/excl_ch.html for more details. In other cases
            // organisations may have deleted their information when closing an organisation or merging with another.
            if (
                string.IsNullOrEmpty(address.Address1)
                && string.IsNullOrEmpty(address.Address2)
                && string.IsNullOrEmpty(address.Address3)
                && string.IsNullOrEmpty(address.TownCity)
                && string.IsNullOrEmpty(address.County)
                && string.IsNullOrEmpty(address.Country)
                && string.IsNullOrEmpty(address.PoBox)
                && string.IsNullOrEmpty(address.GetPostCodeInAllCaps())
            )
            {
                return true;
            }

            return false;
        }

        public static OrganisationAddress CreateOrganisationAddressFromCompaniesHouseAddress(CompaniesHouseAddress companiesHouseAddress)
        {
            string premisesAndLine1 = companiesHouseAddress.GetAddressLineFromPremisesAndAddressLine1();
            bool? isUkAddress = null;
            if (PostcodesIoApi.IsValidPostcode(companiesHouseAddress?.PostalCode))
            {
                isUkAddress = true;
            }

            return new OrganisationAddress {
                Address1 = FirstHundredChars(companiesHouseAddress?.CareOf ?? premisesAndLine1),
                Address2 =
                    FirstHundredChars(companiesHouseAddress?.CareOf != null ? premisesAndLine1 : companiesHouseAddress?.AddressLine2),
                Address3 = FirstHundredChars(companiesHouseAddress?.CareOf != null ? companiesHouseAddress?.AddressLine2 : null),
                TownCity = FirstHundredChars(companiesHouseAddress?.Locality),
                County = FirstHundredChars(companiesHouseAddress?.Region),
                Country = companiesHouseAddress?.Country,
                PostCode = companiesHouseAddress?.PostalCode,
                PoBox = companiesHouseAddress?.PoBox,
                Status = AddressStatuses.Active,
                StatusDate = VirtualDateTime.Now,
                StatusDetails = DetailsOfChange,
                Created = VirtualDateTime.Now,
                Source = SourceOfChange,
                IsUkAddress = isUkAddress
            };
        }

        public void UpdateName(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            string companyNameFromCompaniesHouse = organisationFromCompaniesHouse.CompanyName;
            companyNameFromCompaniesHouse = FirstHundredChars(companyNameFromCompaniesHouse);

            if (IsCompanyNameEqual(organisation.GetName(), companyNameFromCompaniesHouse))
            {
                return;
            }

            var nameToAdd = new OrganisationName {
                Organisation = organisation, Name = companyNameFromCompaniesHouse, Source = SourceOfChange, Created = VirtualDateTime.Now
            };
            organisation.OrganisationNames.Add(nameToAdd);
            organisation.OrganisationName = companyNameFromCompaniesHouse;
            dataRepository.Insert(nameToAdd);
        }

        public static bool IsCompanyNameEqual(OrganisationName organisationName, string companyName)
        {
            return string.Equals(
                organisationName.Name,
                companyName,
                StringComparison.Ordinal);
        }

        private static string FirstHundredChars(string str)
        {
            if (str == null)
            {
                return null;
            }

            return str.Substring(0, Math.Min(str.Length, 100));
        }

        public static bool SicCodesEqual(IEnumerable<OrganisationSicCode> sicCodes, IEnumerable<string> companiesHouseSicCodes)
        {
            List<int> sicCodeIdsFromDatabase = sicCodes
                .Select(sic => sic.SicCodeId)
                .ToList();

            if (companiesHouseSicCodes == null)
            {
                companiesHouseSicCodes = new List<string>();
            }
            List<int> sicCodeIdsFromCompaniesHouse = companiesHouseSicCodes
                .Select(sic => int.Parse(sic))
                .ToList();

            return new HashSet<int>(sicCodeIdsFromDatabase).SetEquals(sicCodeIdsFromCompaniesHouse);
        }

    }
}
