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
        private enum SourceOfData
        {
            CompaniesHouse,
            Manual
        }

        private readonly AuditLogger auditLogger;
        private readonly IDataRepository dataRepository;
        private readonly ICompaniesHouseAPI companiesHouseApi;

        public OrganisationService(
            IDataRepository dataRepository,
            ICompaniesHouseAPI companiesHouseApi,
            AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.companiesHouseApi = companiesHouseApi;
            this.auditLogger = auditLogger;
        }

        public void UpdateIsUkAddressIfItIsNotAlreadySet(long organisationId, bool? isUkAddress)
        {
            Organisation organisation = dataRepository.Get<Organisation>(organisationId);

            OrganisationAddress latestAddress = organisation.GetLatestAddress();

            if (!latestAddress.IsUkAddress.HasValue)
            {
                latestAddress.IsUkAddress = isUkAddress;
                dataRepository.SaveChanges();
            }
        }

        public Organisation ImportOrganisationFromCompaniesHouse(string companyNumber, User requestingUser)
        {
            // If we already have an active organisation with this company number in our database,
            //   then no need to import it, just load it from the database
            Organisation existingOrganisation = dataRepository
                .GetAll<Organisation>()
                .FirstOrDefault(org => org.CompanyNumber == companyNumber);

            if (existingOrganisation != null && existingOrganisation.Status == OrganisationStatuses.Active)
            {
                return existingOrganisation;
            }

            // Reactivate the existing organisation that has been found before returning it.
            // Searchable organisations are either retired or active. This allows users to select
            // previously retired or deleted organisations with the company number rather than having to
            // contact the admin team to get the CompanyNumber removed from the organisation and adding a
            // new one.
            if (existingOrganisation != null &&
                (existingOrganisation.Status == OrganisationStatuses.Retired ||
                 existingOrganisation.Status == OrganisationStatuses.Deleted))
            {
                return SetDeletedRetiredOrganisationToActive(existingOrganisation, requestingUser);
            }

            return SaveNewOrganisation(companyNumber, requestingUser);
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
                CompanyNumber = companyNumber
            };

            SetInitialStatus(organisation, requestingUser, SourceOfData.Manual);

            AddOrganisationName(organisation, organisationName, SourceOfData.Manual);

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

            AddOrganisationSicCodes(organisation, sicCodes, SourceOfData.Manual);

            SetInitialScopes(organisation);

            dataRepository.Insert(organisation);
            dataRepository.SaveChanges();

            return organisation;
        }

        private Organisation SetDeletedRetiredOrganisationToActive(Organisation existingOrganisation, User requestingUser)
        {
            var oldStatus = existingOrganisation.Status;

            existingOrganisation.SetStatus(OrganisationStatuses.Active, requestingUser.UserId, "Automatic reactivation");

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.UserReactivatedOrganisationAutomatically,
                existingOrganisation,
                new { PreviousStatus = oldStatus, NewStatus = OrganisationStatuses.Active, Reason = "Automatic reactivation" },
                requestingUser);

            dataRepository.SaveChanges();

            return existingOrganisation;
        }

        private Organisation SaveNewOrganisation(string companyNumber, User requestingUser)
        {
            CompaniesHouseCompany companiesHouseCompany = companiesHouseApi.GetCompany(companyNumber);

            Organisation organisation = new Organisation
            {
                SectorType = SectorTypes.Private, // All companies imported from CoHo are private-sector
                CompanyNumber = companyNumber
            };

            SetInitialStatus(organisation, requestingUser, SourceOfData.CompaniesHouse);

            AddOrganisationName(organisation, companiesHouseCompany.CompanyName, SourceOfData.CompaniesHouse);

            AddOrganisationAddress(organisation, companiesHouseCompany.RegisteredOfficeAddress);

            AddOrganisationSicCodes(organisation, companiesHouseCompany.SicCodes, SourceOfData.CompaniesHouse);

            SetInitialScopes(organisation);

            dataRepository.Insert(organisation);
            dataRepository.SaveChanges();
            return organisation;
        }

        private void SetInitialStatus(Organisation organisation, User user, SourceOfData sourceOfData)
        {
            OrganisationStatuses status;
            string organisationStatusDetails;
            switch (sourceOfData)
            {
                case SourceOfData.CompaniesHouse:
                    status = OrganisationStatuses.Active;
                    organisationStatusDetails = "Imported from CoHo";
                    break;
                case SourceOfData.Manual:
                    status = OrganisationStatuses.Pending;
                    organisationStatusDetails = "Manually registered";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceOfData), sourceOfData, null);
            }

            organisation.Status = status;
            organisation.StatusDate = VirtualDateTime.Now;
            organisation.StatusDetails = organisationStatusDetails;

            var organisationStatus = new OrganisationStatus
            {
                Organisation = organisation,
                Status = status,
                StatusDate = VirtualDateTime.Now,
                StatusDetails = organisationStatusDetails,
                ByUser = user
            };

            organisation.OrganisationStatuses.Add(organisationStatus);

            dataRepository.Insert(organisationStatus);
        }

        private void AddOrganisationName(Organisation organisation, string name, SourceOfData sourceOfData)
        {
            organisation.OrganisationName = name;

            string source;
            switch (sourceOfData)
            {
                case SourceOfData.CompaniesHouse:
                    source = "CoHo";
                    break;
                case SourceOfData.Manual:
                    source = "User";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceOfData), sourceOfData, null);
            }

            var organisationName = new OrganisationName
            {
                Organisation = organisation,
                Name = name,
                Source = source
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

        private void AddOrganisationSicCodes(Organisation organisation, List<string> sicCodes, SourceOfData sourceOfData)
        {
            if (sicCodes != null)
            {
                foreach (string sicCodeString in sicCodes)
                {
                    AddOrganisationSicCode(organisation, sicCodeString, sourceOfData);
                }
            }
        }

        private void AddOrganisationSicCodes(Organisation organisation, List<int> sicCodes, SourceOfData sourceOfData)
        {
            if (sicCodes != null)
            {
                foreach (int sicCode in sicCodes)
                {
                    AddOrganisationSicCode(organisation, sicCode, sourceOfData);
                }
            }
        }

        private void AddOrganisationSicCode(Organisation organisation, string sicCodeString, SourceOfData sourceOfData)
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
                        Source = sourceOfData.ToString(),
                        Error = $"Could not parse ({sicCodeString}) as an integer"
                    });
                return;
            }

            AddOrganisationSicCode(organisation, sicCodeInt, sourceOfData);
        }

        private void AddOrganisationSicCode(Organisation organisation, int sicCodeInt, SourceOfData sourceOfData)
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
                        Source = sourceOfData.ToString(),
                        Error = $"SIC code ({sicCodeInt}) not found in our database"
                    });
                return;
            }

            string source;
            switch (sourceOfData)
            {
                case SourceOfData.CompaniesHouse:
                    source = "CoHo";
                    break;
                case SourceOfData.Manual:
                    source = "Manual";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sourceOfData), sourceOfData, null);
            }

            var organisationSicCode = new OrganisationSicCode
            {
                Organisation = organisation,
                SicCode = sicCode,
                Source = source
            };

            organisation.OrganisationSicCodes.Add(organisationSicCode);

            dataRepository.Insert(organisationSicCode);
        }

        private void SetInitialScopes(Organisation organisation)
        {
            DateTime currentYearSnapshotDate = organisation.SectorType.GetAccountingStartDate();
            SetInitialScopeForYear(organisation, currentYearSnapshotDate, ScopeStatuses.PresumedInScope);

            int firstYear = Global.FirstReportingYear;
            for (int snapshotYear = firstYear; snapshotYear < currentYearSnapshotDate.Year; snapshotYear++)
            {
                var snapshotDate = new DateTime(snapshotYear, currentYearSnapshotDate.Month, currentYearSnapshotDate.Day);
                SetInitialScopeForYear(organisation, snapshotDate, ScopeStatuses.PresumedOutOfScope);
            }
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
