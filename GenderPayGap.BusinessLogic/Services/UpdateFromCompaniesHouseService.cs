using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using GenderPayGap.Core;
using GenderPayGap.Core.Api;
using GenderPayGap.Core.API;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.CompaniesHouse;
using GenderPayGap.Database;
using GenderPayGap.Extensions;

namespace GenderPayGap.BusinessLogic.Services
{
    public class UpdateFromCompaniesHouseService
    {

        private const string SourceOfChange = "CoHo";
        private const string DetailsOfChange = "Replaced by CoHo";

        private readonly ICompaniesHouseAPI _CompaniesHouseAPI;
        private readonly IDataRepository _DataRepository;

        public UpdateFromCompaniesHouseService(IDataRepository dataRepository, ICompaniesHouseAPI companiesHouseAPI)
        {
            _DataRepository = dataRepository;
            _CompaniesHouseAPI = companiesHouseAPI;
        }

        public void UpdateOrganisationDetails(long organisationId)
        {
            CustomLogger.Debug($"Loading organisation - OrganisationId({organisationId})");
            var organisation = _DataRepository.Get<Organisation>(organisationId);

            CustomLogger.Debug($"Updating LastCheckedAgainstCompaniesHouse - OrganisationId({organisationId})");
            organisation.LastCheckedAgainstCompaniesHouse = VirtualDateTime.Now;
            _DataRepository.SaveChangesAsync().Wait();

            try
            {
                CustomLogger.Debug($"Calling CoHo API - OrganisationId({organisationId})");
                CompaniesHouseCompany organisationFromCompaniesHouse =
                    _CompaniesHouseAPI.GetCompanyAsync(organisation.CompanyNumber).Result;

                CustomLogger.Debug($"Starting transaction - OrganisationId({organisationId})");
                _DataRepository.BeginTransactionAsync(
                        async () => {
                            try
                            {
                                CustomLogger.Debug($"Updating SIC codes - OrganisationId({organisationId})");
                                UpdateSicCode(organisation, organisationFromCompaniesHouse);

                                CustomLogger.Debug($"Updating Address - OrganisationId({organisationId})");
                                UpdateAddress(organisation, organisationFromCompaniesHouse);

                                CustomLogger.Debug($"Updating Name - OrganisationId({organisationId})");
                                UpdateName(organisation, organisationFromCompaniesHouse);

                                CustomLogger.Debug($"Saving - OrganisationId({organisationId})");
                                _DataRepository.SaveChangesAsync().Wait();
                                _DataRepository.CommitTransaction();

                                CustomLogger.Debug($"Saved - OrganisationId({organisationId})");
                            }
                            catch (Exception ex)
                            {
                                string message =
                                    $"Update from Companies House: Failed to update database, organisation id = {organisationId}";
                                CustomLogger.Error(message, ex);
                                _DataRepository.RollbackTransaction();
                            }
                        })
                    .Wait();
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
                if (_DataRepository.GetAll<SicCode>().Any(sicCode => sicCode.SicCodeId == sicCodeId))
                {
                    var sicCodeToBeAdded = new OrganisationSicCode {
                        Organisation = organisation, SicCodeId = sicCodeId, Source = SourceOfChange, Created = VirtualDateTime.Now
                    };
                    organisation.OrganisationSicCodes.Add(sicCodeToBeAdded);
                    _DataRepository.Insert(sicCodeToBeAdded);
                }
            }
        }

        public void UpdateAddress(Organisation organisation, CompaniesHouseCompany organisationFromCompaniesHouse)
        {
            CompaniesHouseAddress companiesHouseAddress = organisationFromCompaniesHouse.RegisteredOfficeAddress;
            OrganisationAddress newOrganisationAddressFromCompaniesHouse =
                CreateOrganisationAddressFromCompaniesHouseAddress(companiesHouseAddress);
            OrganisationAddress oldOrganisationAddress = organisation.GetLatestAddress();
            if (oldOrganisationAddress.AddressMatches(newOrganisationAddressFromCompaniesHouse)
                || IsNewOrganisationAddressNullOrEmpty(newOrganisationAddressFromCompaniesHouse))
            {
                return;
            }

            newOrganisationAddressFromCompaniesHouse.OrganisationId = organisation.OrganisationId;
            organisation.OrganisationAddresses.Add(newOrganisationAddressFromCompaniesHouse);

            oldOrganisationAddress.Status = AddressStatuses.Retired;
            oldOrganisationAddress.StatusDate = VirtualDateTime.Now;
            oldOrganisationAddress.Modified = VirtualDateTime.Now;

            _DataRepository.Insert(newOrganisationAddressFromCompaniesHouse);
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
                && string.IsNullOrEmpty(address.PostCode)
            )
            {
                return true;
            }

            return false;
        }

        public static OrganisationAddress CreateOrganisationAddressFromCompaniesHouseAddress(CompaniesHouseAddress companiesHouseAddress)
        {
            string premisesAndLine1 = GetAddressLineFromPremisesAndAddressLine1(companiesHouseAddress);
            bool? isUkAddress = null;
            if (PostcodesIoApi.IsValidPostcode(companiesHouseAddress?.PostalCode).Result)
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
                Modified = VirtualDateTime.Now,
                Created = VirtualDateTime.Now,
                Source = SourceOfChange,
                IsUkAddress = isUkAddress
            };
        }

        private static string GetAddressLineFromPremisesAndAddressLine1(CompaniesHouseAddress companiesHouseAddress)
        {
            return companiesHouseAddress?.Premises == null
                ? companiesHouseAddress?.AddressLine1
                : companiesHouseAddress?.Premises + "," + companiesHouseAddress?.AddressLine1;
        }

        public static bool AddressMatches(OrganisationAddress firstOrganisationAddress, OrganisationAddress secondOrganisationAddress)
        {
            return string.Equals(
                       firstOrganisationAddress.Address1,
                       secondOrganisationAddress.Address1,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.Address2,
                       secondOrganisationAddress.Address2,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.Address3,
                       secondOrganisationAddress.Address3,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.TownCity,
                       secondOrganisationAddress.TownCity,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.County,
                       secondOrganisationAddress.County,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.Country,
                       secondOrganisationAddress.Country,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.PostCode,
                       secondOrganisationAddress.PostCode,
                       StringComparison.Ordinal)
                   && string.Equals(
                       firstOrganisationAddress.PoBox,
                       secondOrganisationAddress.PoBox,
                       StringComparison.Ordinal);
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
            _DataRepository.Insert(nameToAdd);
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
            return new HashSet<int>(sicCodes.Select(sic => sic.SicCodeId)).SetEquals(companiesHouseSicCodes.Select(sic => int.Parse(sic)));
        }

    }
}
