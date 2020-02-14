using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminDatabaseIntegrityChecksController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminDatabaseIntegrityChecksController(
            IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("database-integrity-checks")]
        public async Task<IActionResult> DatabaseIntegrityChecks()
        {
            return View("DatabaseIntegrityChecks");
        }

        [HttpGet("database-integrity-checks/organisations-with-multiple-active-addresses")]
        public async Task<IActionResult> OrganisationsWithMultipleActiveAddresses()
        {
            var organisationsWithMultipleActiveAddresses = new List<Organisation>();
            List<Organisation> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationAddresses).ToList();

            foreach (Organisation organisation in organisations)
            {
                int numberOfActiveAddresses = organisation.OrganisationAddresses
                    .Count(oa => oa.Status == AddressStatuses.Active);
                if (numberOfActiveAddresses > 2)
                {
                    organisationsWithMultipleActiveAddresses.Add(organisation);
                }
            }

            return PartialView("OrganisationsWithMultipleActiveAddresses", organisationsWithMultipleActiveAddresses);
        }

        [HttpGet("database-integrity-checks/active-organisations-with-the-same-name")]
        public async Task<IActionResult> ActiveOrganisationsWithTheSameName()
        {
            var activeOrganisationsWithTheSameName = new List<string>();
            List<Organisation> activeOrganisations = dataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active).ToList();

            var duplicateNames = activeOrganisations.GroupBy(x => x.OrganisationName)
                .Select(x => new {Name = x.Key, Count = x.Count()})
                .Where(x => x.Count > 1);

            foreach (var duplicate in duplicateNames)
            {
                activeOrganisationsWithTheSameName.Add(duplicate.Name);
            }

            return PartialView("ActiveOrganisationsWithTheSameName", activeOrganisationsWithTheSameName);
        }

        [HttpGet("database-integrity-checks/active-organisations-with-the-same-company-number")]
        public async Task<IActionResult> ActiveOrganisationsWithTheSameCompanyNumber()
        {
            var activeOrganisationsWithTheSameCompanyNumber = new List<string>();
            List<Organisation> activeOrganisations = dataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active).ToList();

            var duplicateCompanyNumbers = activeOrganisations.GroupBy(x => x.CompanyNumber)
                .Select(x => new {CompanyNumber = x.Key, Count = x.Count()})
                .Where(x => x.Count > 1);

            foreach (var duplicate in duplicateCompanyNumbers)
            {
                if (duplicate.CompanyNumber != null)
                {
                    activeOrganisationsWithTheSameCompanyNumber.Add(duplicate.CompanyNumber);
                }
            }

            return PartialView("ActiveOrganisationsWithTheSameCompanyNumber", activeOrganisationsWithTheSameCompanyNumber);
        }

        [HttpGet("database-integrity-checks/organisations-where-latest-address-is-not-active")]
        public async Task<IActionResult> OrganisationsWhereLatestAddressIsNotActive()
        {
            var organisationsWhereLatestAddressIsNotActive = new List<Organisation>();
            List<Organisation> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationAddresses).ToList();
            foreach (Organisation organisation in organisations)
            {
                OrganisationAddress latestAddress = organisation.LatestAddress;
                IEnumerable<OrganisationAddress> activeAddresses = organisation.OrganisationAddresses
                    .Where(oa => oa.Status == AddressStatuses.Active);
                if (!activeAddresses.Contains(latestAddress))
                {
                    organisationsWhereLatestAddressIsNotActive.Add(organisation);
                }
            }

            return PartialView("OrganisationsWhereLatestAddressIsNotActive", organisationsWhereLatestAddressIsNotActive);
        }

        [HttpGet("database-integrity-checks/organisations-with-multiple-active-scopes-for-a-single-year")]
        public async Task<IActionResult> OrganisationsWithMultipleActiveScopesForASingleYear()
        {
            var organisationsWithMultipleActiveScopesForASingleYear = new List<Organisation>();
            List<Organisation> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationScopes).ToList();
            foreach (Organisation organisation in organisations)
            {
                IEnumerable<OrganisationScope> activeScopes = organisation.OrganisationScopes
                    .Where(os => os.Status == ScopeRowStatuses.Active);

                bool multipleActiveScopesForYear = activeScopes
                    .GroupBy(scope => scope.SnapshotDate)
                    .Any(g => g.Count() > 1);

                if (multipleActiveScopesForYear)
                {
                    organisationsWithMultipleActiveScopesForASingleYear.Add(organisation);
                }
            }

            return PartialView("OrganisationsWithMultipleActiveScopesForASingleYear", organisationsWithMultipleActiveScopesForASingleYear);
        }

        [HttpGet("database-integrity-checks/organisations-with-no-active-scope-for-every-year")]
        public async Task<IActionResult> OrganisationsWithNoActiveScopeForEveryYear()
        {
            var organisationsWithNoActiveScopeForEveryYear = new List<Organisation>();
            List<Organisation> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationScopes).ToList();

            foreach (Organisation organisation in organisations)
            {
                DateTime organisationCreationDate = organisation.Created;
                DateTime accountingDateForCreationYear = organisation.SectorType.GetAccountingStartDate(organisationCreationDate.Year);

                int firstRequiredAccountingDateYear = organisationCreationDate < accountingDateForCreationYear
                    ? accountingDateForCreationYear.AddYears(-1).Year
                    : accountingDateForCreationYear.Year;

                int latestRequiredAccountingDateYear = organisation.SectorType.GetAccountingStartDate().Year;

                IEnumerable<int> requiredYears = Enumerable.Range(
                    firstRequiredAccountingDateYear,
                    (latestRequiredAccountingDateYear - firstRequiredAccountingDateYear) + 1);

                IEnumerable<int> yearsWithActiveScopes = organisation.OrganisationScopes
                    .Where(os => os.Status == ScopeRowStatuses.Active)
                    .GroupBy(scope => scope.SnapshotDate)
                    .Select(g => g.Key.Year);

                foreach (int year in requiredYears)
                {
                    if (!yearsWithActiveScopes.Contains(year) && !organisationsWithNoActiveScopeForEveryYear.Contains(organisation))
                    {
                        organisationsWithNoActiveScopeForEveryYear.Add(organisation);
                    }
                }
            }

            return PartialView("OrganisationsWithNoActiveScopeForEveryYear", organisationsWithNoActiveScopeForEveryYear);
        }

        [HttpGet("database-integrity-checks/organisations-with-multiple-active-returns-for-a-single-year")]
        public async Task<IActionResult> OrganisationsWithMultipleActiveReturnsForASingleYear()
        {
            var organisationsWithMultipleActiveReturnsForASingleYear = new List<Organisation>();
            List<Organisation> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.Returns).ToList();

            foreach (Organisation organisation in organisations)
            {
                IEnumerable<Return> activeReturns = organisation.Returns
                    .Where(r => r.Status == ReturnStatuses.Submitted);

                bool multipleActiveReturnsForYear = activeReturns
                    .GroupBy(r => r.AccountingDate.Year)
                    .Any(g => g.Count() > 1);

                if (multipleActiveReturnsForYear)
                {
                    organisationsWithMultipleActiveReturnsForASingleYear.Add(organisation);
                }
            }

            return PartialView(
                "OrganisationsWithMultipleActiveReturnsForASingleYear",
                organisationsWithMultipleActiveReturnsForASingleYear);
        }

        [HttpGet("database-integrity-checks/public-sector-organisations-without-a-public-sector-type")]
        public async Task<IActionResult> PublicSectorOrganisationsWithoutAPublicSectorType()
        {
            var publicSectorOrganisationsWithoutAPublicSectorType = new List<Organisation>();
            List<Organisation> activePublicOrganisations = dataRepository
                .GetAll<Organisation>()
                .Where(o => o.SectorType == SectorTypes.Public)
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Include(o => o.OrganisationScopes).ToList();

            foreach (Organisation organisation in activePublicOrganisations)
            {
                bool isInScope =
                    organisation.LatestScope.Status == ScopeRowStatuses.Active
                    && (organisation.LatestScope.ScopeStatus == ScopeStatuses.InScope
                        || organisation.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope);


                if (isInScope && organisation.LatestPublicSectorType == null)
                {
                    publicSectorOrganisationsWithoutAPublicSectorType.Add(organisation);
                }
            }

            return PartialView("PublicSectorOrganisationsWithoutAPublicSectorType", publicSectorOrganisationsWithoutAPublicSectorType);
        }

        [HttpGet("database-integrity-checks/private-sector-organisations-with-a-public-sector-type")]
        public async Task<IActionResult> PrivateSectorOrganisationsWithAPublicSectorType()
        {
            var privateSectorOrganisationsWithAPublicSectorType = new List<Organisation>();
            List<Organisation> activePrivateOrganisations = dataRepository
                .GetAll<Organisation>()
                .Where(o => o.SectorType == SectorTypes.Private)
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Include(o => o.OrganisationScopes).ToList();

            foreach (Organisation organisation in activePrivateOrganisations)
            {
                bool isInScope =
                    organisation.LatestScope.Status == ScopeRowStatuses.Active
                    && (organisation.LatestScope.ScopeStatus == ScopeStatuses.InScope
                        || organisation.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope);


                if (isInScope && organisation.LatestPublicSectorType != null)
                {
                    privateSectorOrganisationsWithAPublicSectorType.Add(organisation);
                }
            }

            return PartialView("PrivateSectorOrganisationsWithAPublicSectorType", privateSectorOrganisationsWithAPublicSectorType);
        }

    }
}
