using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin)]
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
        public IActionResult DatabaseIntegrityChecks()
        {
            return View("DatabaseIntegrityChecks");
        }
        
        [HttpGet("database-integrity-checks/active-organisations-with-the-same-name")]
        public IActionResult ActiveOrganisationsWithTheSameName()
        {
            List<string> duplicateOrganisationNames =
                dataRepository.GetAll<Organisation>()
                    .Where(o => o.Status == OrganisationStatuses.Active /* Choose just the active organisations */)
                    .Select(o => o.OrganisationName /* Just get their names (makes the query run faster) */)
                    .GroupBy(on => on /* Group by the names */)
                    .Select(grouping => new {Name = grouping.Key, Count = grouping.Count()})
                    .Where(nameAndCount => nameAndCount.Count > 1)
                    .Select(nameAndCount => nameAndCount.Name)
                    .ToList();

            return PartialView("ActiveOrganisationsWithTheSameName", duplicateOrganisationNames);
        }
        
        [HttpGet("database-integrity-checks/active-organisations-with-the-same-company-number")]
        public IActionResult ActiveOrganisationsWithTheSameCompanyNumber()
        {
            List<string> duplicateOrganisationCompanyNumbers =
                dataRepository.GetAll<Organisation>()
                    .Where(o => o.Status == OrganisationStatuses.Active /* Choose just the active organisations */)
                    .Select(o => o.CompanyNumber /* Just get their company numbers (makes the query run faster) */)
                    .GroupBy(cn => cn /* Group by the company number */)
                    .Select(grouping => new {CompanyNumber = grouping.Key, Count = grouping.Count()})
                    .Where(nameAndCompanyNumber => nameAndCompanyNumber.Count > 1)
                    .Where(nameAndCompanyNumber => nameAndCompanyNumber.CompanyNumber != null)
                    .Select(nameAndCompanyNumber => nameAndCompanyNumber.CompanyNumber)
                    .ToList();

            return PartialView("ActiveOrganisationsWithTheSameCompanyNumber", duplicateOrganisationCompanyNumbers);
        }

        [HttpGet("database-integrity-checks/organisations-with-multiple-active-addresses")]
        public IActionResult OrganisationsWithMultipleActiveAddresses()
        {
            List<Organisation> organisationsWithMultipleActiveAddresses =
                dataRepository.GetAll<Organisation>()
                    .Where(o => o.OrganisationAddresses.Count(oa => oa.Status == AddressStatuses.Active) > 2)
                    .ToList();

            return PartialView("OrganisationsWithMultipleActiveAddresses", organisationsWithMultipleActiveAddresses);
        }
        
        [HttpGet("database-integrity-checks/organisations-without-an-active-address")]
        public IActionResult OrganisationsWithoutAnActiveAddress()
        {
            List<Organisation> organisationsWithoutAnActiveAddress =
                dataRepository.GetAll<Organisation>()
                    .Where(o => !o.OrganisationAddresses.Any(oa => oa.Status == AddressStatuses.Active))
                    .ToList();

            return PartialView("OrganisationsWithoutAnActiveAddress", organisationsWithoutAnActiveAddress);
        }
        
        [HttpGet("database-integrity-checks/organisations-where-latest-address-is-not-active")]
        public IActionResult OrganisationsWhereLatestAddressIsNotActive()
        {
            List<Organisation> organisationsWhereLatestAddressIsNotActive =
                dataRepository.GetAll<Organisation>()
                    .Include(o => o.OrganisationAddresses)
                    .AsEnumerable( /* Needed to prevent "The LINQ expression could not be translated" - o.GetLatestAddress() cannot be translated */)
                    .Where(o => o.GetLatestAddress() != null)
                    .Where(o => o.GetLatestAddress().Status != AddressStatuses.Active)
                    .ToList();

            return PartialView("OrganisationsWhereLatestAddressIsNotActive", organisationsWhereLatestAddressIsNotActive);
        }
        
        [HttpGet("database-integrity-checks/organisations-with-multiple-active-scopes-for-a-single-year")]
        public IActionResult OrganisationsWithMultipleActiveScopesForASingleYear()
        {
            List<Organisation> organisationsWithMultipleActiveScopesForASingleYear =
                dataRepository.GetAll<Organisation>()
                    .Where(
                        o => o.OrganisationScopes
                            .Where(scope => scope.Status == ScopeRowStatuses.Active)
                            .GroupBy(scope => scope.SnapshotDate)
                            .Any(grouping => grouping.Count() > 1))
                    .ToList();

            return PartialView("OrganisationsWithMultipleActiveScopesForASingleYear", organisationsWithMultipleActiveScopesForASingleYear);
        }

        [HttpGet("database-integrity-checks/organisations-with-no-active-scope-for-every-year")]
        public IActionResult OrganisationsWithNoActiveScopeForEveryYear()
        {
            var organisationsWithNoActiveScopeForEveryYear = new List<Organisation>();
            List<Organisation> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationScopes)
                .ToList();

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
        public IActionResult OrganisationsWithMultipleActiveReturnsForASingleYear()
        {
            List<Organisation> organisationsWithMultipleActiveReturnsForASingleYear =
                dataRepository.GetAll<Organisation>()
                    .Where(
                        o => o.Returns
                            .Where(r => r.Status == ReturnStatuses.Submitted)
                            .GroupBy(r => r.AccountingDate.Year)
                            .Any(g => g.Count() > 1))
                    .ToList();

            return PartialView("OrganisationsWithMultipleActiveReturnsForASingleYear", organisationsWithMultipleActiveReturnsForASingleYear);
        }

        [HttpGet("database-integrity-checks/public-sector-organisations-without-a-public-sector-type")]
        public IActionResult PublicSectorOrganisationsWithoutAPublicSectorType()
        {
            List<Organisation> publicSectorOrganisationsWithoutAPublicSectorType =
                dataRepository.GetAll<Organisation>()
                    .Where(o => o.Status == OrganisationStatuses.Active)
                    .Where(o => o.SectorType == SectorTypes.Public)
                    .Where(o => o.LatestPublicSectorType == null)
                    .Include(o => o.OrganisationScopes)
                    .AsEnumerable( /* Needed to prevent "The LINQ expression could not be translated" - o.GetCurrentScope() cannot be translated */)
                    .Where(o => o.GetCurrentScope() != null)
                    .Where(o => o.GetCurrentScope().ScopeStatus == ScopeStatuses.InScope || o.GetCurrentScope().ScopeStatus == ScopeStatuses.PresumedInScope)
                    .ToList();

            return PartialView("PublicSectorOrganisationsWithoutAPublicSectorType", publicSectorOrganisationsWithoutAPublicSectorType);
        }

        [HttpGet("database-integrity-checks/private-sector-organisations-with-a-public-sector-type")]
        public IActionResult PrivateSectorOrganisationsWithAPublicSectorType()
        {
            List<Organisation> privateSectorOrganisationsWithAPublicSectorType =
                dataRepository.GetAll<Organisation>()
                    .Where(o => o.Status == OrganisationStatuses.Active)
                    .Where(o => o.SectorType == SectorTypes.Private)
                    .Where(organisation => organisation.LatestPublicSectorType != null)
                    .ToList();

            return PartialView("PrivateSectorOrganisationsWithAPublicSectorType", privateSectorOrganisationsWithAPublicSectorType);
        }

    }
}
