using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpGet("organisations-with-multiple-active-addresses-ajax")]
        public async Task<IActionResult> OrganisationsWithMultipleActiveAddressesAjax()
        {
            var model = new DatabaseIntegrityChecksViewModel();
            List<Organisation> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationAddresses)
                .ToList();

            foreach (Organisation organisation in organisations)
            {
                IEnumerable<OrganisationAddress> activeAddresses = organisation.OrganisationAddresses
                    .Where(oa => oa.Status == AddressStatuses.Active);
                if (activeAddresses.Count() > 2)
                {
                    model.OrganisationsWithMultipleActiveAddresses.Add(organisation);
                }
            }

            return PartialView("OrganisationsWithMultipleActiveAddresses", model);
        }

        [HttpGet("organisations-where-latest-address-is-not-active-ajax")]
        public async Task<IActionResult> OrganisationsWhereLatestAddressIsNotActiveAjax()
        {
            var model = new DatabaseIntegrityChecksViewModel();
            List<Organisation> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationAddresses)
                .ToList();
            foreach (Organisation organisation in organisations)
            {
                OrganisationAddress latestAddress = organisation.LatestAddress;
                List<OrganisationAddress> activeAddresses = organisation.OrganisationAddresses
                    .Where(oa => oa.Status == AddressStatuses.Active)
                    .ToList();
                if (!activeAddresses.Contains(latestAddress))
                {
                    model.OrganisationsWhereLatestAddressIsNotActive.Add(organisation);
                }
            }

            return PartialView("OrganisationsWhereLatestAddressIsNotActive", model);
        }

        [HttpGet("organisations-with-multiple-active-scopes-for-a-single-year-ajax")]
        public async Task<IActionResult> OrganisationsWithMultipleActiveScopesForASingleYearAjax()
        {
            var model = new DatabaseIntegrityChecksViewModel();
            List<Organisation> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationScopes)
                .ToList();
            foreach (Organisation organisation in organisations)
            {
                List<OrganisationScope> activeScopes = organisation.OrganisationScopes
                    .Where(os => os.Status == ScopeRowStatuses.Active)
                    .ToList();

                bool multipleActiveScopesForYear = activeScopes
                    .GroupBy(scope => scope.SnapshotDate)
                    .Any(g => g.Count() > 1);

                if (multipleActiveScopesForYear)
                {
                    model.OrganisationsWithMultipleActiveScopesForASingleYear.Add(organisation);
                }
            }

            return PartialView("OrganisationsWithMultipleActiveScopesForASingleYear", model);
        }

        [HttpGet("organisations-with-no-active-scope-for-every-year-ajax")]
        public async Task<IActionResult> OrganisationsWithNoActiveScopeForEveryYearAjax()
        {
            var model = new DatabaseIntegrityChecksViewModel();
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

                List<int> requiredYears = Enumerable.Range(
                        firstRequiredAccountingDateYear,
                        (latestRequiredAccountingDateYear - firstRequiredAccountingDateYear) + 1)
                    .ToList();

                List<int> yearsWithActiveScopes = organisation.OrganisationScopes
                    .Where(os => os.Status == ScopeRowStatuses.Active)
                    .GroupBy(scope => scope.SnapshotDate)
                    .Select(g => g.Key.Year)
                    .ToList();

                foreach (int year in requiredYears)
                {
                    if (!yearsWithActiveScopes.Contains(year))
                    {
                        model.OrganisationsWithNoActiveScopeForEveryYear.Add(organisation);
                    }
                }
            }

            return PartialView("OrganisationsWithNoActiveScopeForEveryYear", model);
        }

        [HttpGet("organisations-with-multiple-active-returns-for-a-single-year-ajax")]
        public async Task<IActionResult> OrganisationsWithMultipleActiveReturnsForASingleYearAjax()
        {
            var model = new DatabaseIntegrityChecksViewModel();
            List<Organisation> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.Returns)
                .ToList();

            foreach (Organisation organisation in organisations)
            {
                List<Return> activeReturns = organisation.Returns
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .ToList();

                bool multipleActiveReturnsForYear = activeReturns
                    .GroupBy(r => r.AccountingDate)
                    .Any(g => g.Count() > 1);

                if (multipleActiveReturnsForYear)
                {
                    model.OrganisationsWithMultipleActiveReturnsForASingleYear.Add(organisation);
                }
            }

            return PartialView("OrganisationsWithMultipleActiveReturnsForASingleYear", model);
        }

        [HttpGet("public-sector-organisations-without-a-public-sector-type-ajax")]
        public async Task<IActionResult> PublicSectorOrganisationsWithoutAPublicSectorTypeAjax()
        {
            var model = new DatabaseIntegrityChecksViewModel();
            List<Organisation> activePublicOrganisations = dataRepository.GetAll<Organisation>()
                .Where(o => o.SectorType == SectorTypes.Public)
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Include(o => o.OrganisationScopes)
                .ToList();

            foreach (Organisation organisation in activePublicOrganisations)
            {
                bool isInScope =
                    organisation.LatestScope.Status == ScopeRowStatuses.Active
                    && (organisation.LatestScope.ScopeStatus == ScopeStatuses.InScope
                        || organisation.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope);
                

                if (isInScope && organisation.LatestPublicSectorType == null)
                {
                    model.PublicSectorOrganisationsWithoutAPublicSectorType.Add(organisation);
                }
            }

            return PartialView("PublicSectorOrganisationsWithoutAPublicSectorType", model);
        }

    }
}
