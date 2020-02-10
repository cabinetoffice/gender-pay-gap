using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        [HttpGet("database-integrity-checks/organisations-with-multiple-active-addresses-ajax")]
        public async Task<IActionResult> OrganisationsWithMultipleActiveAddressesAjax()
        {
            var organisationsWithMultipleActiveAddresses = new List<Organisation>();
            IIncludableQueryable<Organisation, ICollection<OrganisationAddress>> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationAddresses);

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

        [HttpGet("database-integrity-checks/active-organisations-with-the-same-name-ajax")]
        public async Task<IActionResult> ActiveOrganisationsWithTheSameNameAjax()
        {
            var activeOrganisationsWithTheSameName = new List<string>();
            IQueryable<Organisation> activeOrganisations = dataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active);

            var duplicateNames = activeOrganisations.GroupBy(x => x.OrganisationName)
                .Select(x => new {Name = x.Key, Count = x.Count()})
                .Where(x => x.Count > 1);

            foreach (var duplicate in duplicateNames)
            {
                activeOrganisationsWithTheSameName.Add(duplicate.Name);
            }

            return PartialView("ActiveOrganisationsWithTheSameName", activeOrganisationsWithTheSameName);
        }

        [HttpGet("database-integrity-checks/active-organisations-with-the-same-company-number-ajax")]
        public async Task<IActionResult> ActiveOrganisationsWithTheSameCompanyNumberAjax()
        {
            var activeOrganisationsWithTheSameCompanyNumber = new List<string>();
            IQueryable<Organisation> activeOrganisations = dataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active);

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

        [HttpGet("database-integrity-checks/organisations-where-latest-address-is-not-active-ajax")]
        public async Task<IActionResult> OrganisationsWhereLatestAddressIsNotActiveAjax()
        {
            var organisationsWhereLatestAddressIsNotActive = new List<Organisation>();
            IIncludableQueryable<Organisation, ICollection<OrganisationAddress>> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationAddresses);
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

        [HttpGet("database-integrity-checks/organisations-with-multiple-active-scopes-for-a-single-year-ajax")]
        public async Task<IActionResult> OrganisationsWithMultipleActiveScopesForASingleYearAjax()
        {
            var organisationsWithMultipleActiveScopesForASingleYear = new List<Organisation>();
            IIncludableQueryable<Organisation, ICollection<OrganisationScope>> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationScopes);
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

        [HttpGet("database-integrity-checks/organisations-with-no-active-scope-for-every-year-ajax")]
        public async Task<IActionResult> OrganisationsWithNoActiveScopeForEveryYearAjax()
        {
            var organisationsWithNoActiveScopeForEveryYear = new List<Organisation>();
            IIncludableQueryable<Organisation, ICollection<OrganisationScope>> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationScopes);

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

        [HttpGet("database-integrity-checks/organisations-with-multiple-active-returns-for-a-single-year-ajax")]
        public async Task<IActionResult> OrganisationsWithMultipleActiveReturnsForASingleYearAjax()
        {
            var organisationsWithMultipleActiveReturnsForASingleYear = new List<Organisation>();
            IIncludableQueryable<Organisation, ICollection<Return>> organisations = dataRepository.GetAll<Organisation>()
                .Include(o => o.Returns);

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

        [HttpGet("database-integrity-checks/public-sector-organisations-without-a-public-sector-type-ajax")]
        public async Task<IActionResult> PublicSectorOrganisationsWithoutAPublicSectorTypeAjax()
        {
            var publicSectorOrganisationsWithoutAPublicSectorType = new List<Organisation>();
            IIncludableQueryable<Organisation, ICollection<OrganisationScope>> activePublicOrganisations = dataRepository
                .GetAll<Organisation>()
                .Where(o => o.SectorType == SectorTypes.Public)
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Include(o => o.OrganisationScopes);

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

        [HttpGet("database-integrity-checks/private-sector-organisations-with-a-public-sector-type-ajax")]
        public async Task<IActionResult> PrivateSectorOrganisationsWithAPublicSectorTypeAjax()
        {
            var privateSectorOrganisationsWithAPublicSectorType = new List<Organisation>();
            IIncludableQueryable<Organisation, ICollection<OrganisationScope>> activePrivateOrganisations = dataRepository
                .GetAll<Organisation>()
                .Where(o => o.SectorType == SectorTypes.Private)
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Include(o => o.OrganisationScopes);

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
        
        [HttpGet("database-integrity-checks/broken-weblinks-ajax")]
        public async Task<IActionResult> BrokenWeblinksAjax()
        {
            var organisationsWithBrokenLinks = new List<Organisation>();
            IQueryable<Organisation> organisations = dataRepository
                .GetAll<Organisation>()
            
                .Where(o => o.SectorType == SectorTypes.Private)
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Include(o => o.LatestReturn);

            var i = 0;
            var numberOfOrganisations = organisations.Count();
            foreach (Organisation organisation in organisations)
            {
                Console.WriteLine($@"Organisation {i} of {numberOfOrganisations}");
                var latestReturn = organisation.LatestReturn;
                Console.WriteLine(latestReturn);
                
                if (latestReturn != null && latestReturn.CompanyLinkToGPGInfo != null)
                {
                    var uri = new Uri(latestReturn.CompanyLinkToGPGInfo);
                    Console.WriteLine(uri);
                    
                    HttpWebRequest HttpWReq = 
                        (HttpWebRequest)WebRequest.Create(uri);

                    try
                    {
                        HttpWebResponse HttpWResp = (HttpWebResponse) HttpWReq.GetResponse();
                        if (HttpWResp.StatusCode != HttpStatusCode.OK)
                        {
                            organisationsWithBrokenLinks.Add(organisation);
                        }

                        HttpWResp.Close();
                    }
                    catch (Exception) {
                        organisationsWithBrokenLinks.Add(organisation);
                    }
                    
                }
                Console.WriteLine($@"Organisations with broken links {organisationsWithBrokenLinks.Count()}");

                i++;
            }
            
            return PartialView("OrganisationsWithBrokenLinks", organisationsWithBrokenLinks);
        }

    }
}
