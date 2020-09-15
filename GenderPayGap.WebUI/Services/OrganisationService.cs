using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;

namespace GenderPayGap.WebUI.Services
{
    public class OrganisationService
    {

        private readonly IDataRepository dataRepository;
        private readonly ICompaniesHouseAPI companiesHouseApi;

        public OrganisationService(
            IDataRepository dataRepository,
            ICompaniesHouseAPI companiesHouseApi)
        {
            this.dataRepository = dataRepository;
            this.companiesHouseApi = companiesHouseApi;
        }

        public void UpdateIsUkAddressIfItIsNotAlreadySet(long organisationId, bool? isUkAddress)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            OrganisationAddress latestAddress = organisation.GetLatestAddress();

            if (!latestAddress.IsUkAddress.HasValue)
            {
                latestAddress.IsUkAddress = isUkAddress;
                dataRepository.SaveChangesAsync().Wait();
            }
        }

        public Organisation ImportOrganisationFromCompaniesHouse(string companyNumber, User requestingUser)
        {
            // If we already have an organisation with this company number in our database,
            //   then no need to import it, just load it from the database
            if (dataRepository.GetAll<Organisation>().Any(o => o.CompanyNumber == companyNumber))
            {
                return dataRepository.GetAll<Organisation>().First(o => o.CompanyNumber == companyNumber);
            }

            CompaniesHouseCompany companiesHouseCompany = companiesHouseApi.GetCompanyAsync(companyNumber).Result;

            Organisation organisation = new Organisation
            {
                SectorType = SectorTypes.Private, // All companies imported from CoHo are private-sector
                CompanyNumber = companyNumber,
                EmployerReference = GenerateUniqueEmployerReference()
            };

            SetInitialStatus(organisation, requestingUser, 
                OrganisationStatuses.Active /* Organisations imported from CoHo are trusted, so we set them to Active */);

            AddOrganisationName(organisation, companiesHouseCompany.CompanyName);

            AddOrganisationAddress(organisation, companiesHouseCompany.RegisteredOfficeAddress);

            AddOrganisationSicCodes(organisation, companiesHouseCompany.SicCodes);

            SetInitialScopes(organisation);

            dataRepository.Insert(organisation);
            dataRepository.SaveChangesAsync().Wait();

            return organisation;
        }

        public Organisation CreateOrganisationFromManualDataEntry(SectorTypes sector,
            string organisationName,
            string poBox,
            string address1,
            string address2,
            string address3,
            string townCity,
            string county,
            string country,
            string postCode,
            bool? isUkAddress,
            string companyNumber,
            List<int> sicCodes,
            User requestingUser)
        {
            Organisation organisation = new Organisation
            {
                SectorType = sector,
                CompanyNumber = companyNumber,
                EmployerReference = GenerateUniqueEmployerReference()
            };

            SetInitialStatus(organisation, requestingUser, 
                OrganisationStatuses.Pending /* Organisations that the user created manually  */);

            AddOrganisationName(organisation, organisationName);

            AddOrganisationAddress(
                organisation,
                requestingUser,
                poBox,
                address1,
                address2,
                address3,
                townCity,
                county,
                country,
                postCode,
                isUkAddress
            );

            AddOrganisationSicCodes(organisation, sicCodes);

            SetInitialScopes(organisation);

            dataRepository.Insert(organisation);
            dataRepository.SaveChangesAsync().Wait();

            return organisation;
        }

        private string GenerateUniqueEmployerReference()
        {
            string employerReference;
            bool isAlreadyTaken;

            do
            {
                employerReference = Crypto.GenerateEmployerReference();
                isAlreadyTaken = dataRepository.GetAll<Organisation>().Any(o => o.EmployerReference == employerReference);

            } while (isAlreadyTaken);

            return employerReference;
        }

        private void SetInitialStatus(Organisation organisation, User user, OrganisationStatuses status)
        {
            organisation.Status = status;
            organisation.StatusDate = DateTime.Now;
            organisation.StatusDetails = "Imported from CoHo";

            var organisationStatus = new OrganisationStatus
            {
                Organisation = organisation,
                Status = status,
                StatusDate = VirtualDateTime.Now,
                StatusDetails = "Imported from CoHo",
                ByUser = user
            };

            organisation.OrganisationStatuses.Add(organisationStatus);

            dataRepository.Insert(organisationStatus);
        }

        private void AddOrganisationName(Organisation organisation, string name)
        {
            organisation.OrganisationName = name;

            var organisationName = new OrganisationName
            {
                Organisation = organisation,
                Name = name,
                Source = "CoHo"
            };

            organisation.OrganisationNames.Add(organisationName);

            dataRepository.Insert(organisationName);
        }

        private void AddOrganisationAddress(Organisation organisation, CompaniesHouseAddress companiesHouseAddress)
        {
            OrganisationAddress organisationAddress =
                UpdateFromCompaniesHouseService.CreateOrganisationAddressFromCompaniesHouseAddress(companiesHouseAddress);

            organisationAddress.StatusDetails = "Initial import from CoHo";
            
            organisationAddress.Organisation = organisation;
            organisation.OrganisationAddresses.Add(organisationAddress);

            dataRepository.Insert(organisationAddress);
        }

        private void AddOrganisationAddress(Organisation organisation,
            User user,
            string poBox,
            string address1,
            string address2,
            string address3,
            string townCity,
            string county,
            string country,
            string postCode,
            bool? isUkAddress)
        {
            var organisationAddress = new OrganisationAddress
            {
                PoBox = poBox,
                Address1 = address1,
                Address2 = address2,
                Address3 = address3,
                TownCity = townCity,
                County = county,
                Country = country,
                PostCode = postCode,
                IsUkAddress = isUkAddress,

                Created = VirtualDateTime.Now,
                Source = "User",
                Status = AddressStatuses.Pending,
                StatusDetails = "Manually registered",
                StatusDate = VirtualDateTime.Now,

                Organisation = organisation,
                CreatedByUserId = user.UserId
            };

            organisation.OrganisationAddresses.Add(organisationAddress);

            dataRepository.Insert(organisationAddress);
        }

        private void AddOrganisationSicCodes(Organisation organisation, List<string> sicCodes)
        {
            if (sicCodes != null)
            {
                foreach (string sicCodeString in sicCodes)
                {
                    AddOrganisationSicCode(organisation, sicCodeString);
                }
            }
        }

        private void AddOrganisationSicCodes(Organisation organisation, List<int> sicCodes)
        {
            if (sicCodes != null)
            {
                foreach (int sicCode in sicCodes)
                {
                    AddOrganisationSicCode(organisation, sicCode);
                }
            }
        }

        private void AddOrganisationSicCode(Organisation organisation, string sicCodeString)
        {
            if (!int.TryParse(sicCodeString, out int sicCodeInt))
            {
                CustomLogger.Warning(
                    "Bad SIC code",
                    new
                    {
                        OrganisationName = organisation.OrganisationName,
                        CompanyNumber = organisation.CompanyNumber,
                        SicCode = sicCodeString,
                        Source = "CoHo",
                        Error = $"Could not parse ({sicCodeString}) as an integer"
                    });
                return;
            }

            AddOrganisationSicCode(organisation, sicCodeInt);
        }

        private void AddOrganisationSicCode(Organisation organisation, int sicCodeInt)
        {
            SicCode sicCode = dataRepository.Get<SicCode>(sicCodeInt);

            if (sicCode == null)
            {
                CustomLogger.Warning(
                    "Bad SIC code",
                    new
                    {
                        OrganisationName = organisation.OrganisationName,
                        CompanyNumber = organisation.CompanyNumber,
                        SicCode = sicCodeInt,
                        Source = "CoHo",
                        Error = $"SIC code ({sicCodeInt}) not found in our database"
                    });
                return;
            }

            var organisationSicCode = new OrganisationSicCode
            {
                Organisation = organisation,
                SicCode = sicCode,
                Source = "CoHo"
            };

            organisation.OrganisationSicCodes.Add(organisationSicCode);

            dataRepository.Insert(organisationSicCode);
        }

        private void SetInitialScopes(Organisation organisation)
        {
            DateTime currentYearSnapshotDate = organisation.SectorType.GetAccountingStartDate();
            SetInitialScopeForYear(organisation, currentYearSnapshotDate, ScopeStatuses.PresumedInScope);

            DateTime previousYearSnapshotDate = currentYearSnapshotDate.AddYears(-1);
            SetInitialScopeForYear(organisation, previousYearSnapshotDate, ScopeStatuses.PresumedOutOfScope);
        }

        private void SetInitialScopeForYear(Organisation organisation, DateTime snapshotDate, ScopeStatuses scopeStatus)
        {
            var organisationScope = new OrganisationScope
            {
                Organisation = organisation,
                ScopeStatus = scopeStatus,
                ScopeStatusDate = VirtualDateTime.Now,
                SnapshotDate = snapshotDate,
                Status = ScopeRowStatuses.Active,
                StatusDetails = "Generated by the system"
            };

            organisation.OrganisationScopes.Add(organisationScope);

            dataRepository.Insert(organisationScope);
        }

    }
}
