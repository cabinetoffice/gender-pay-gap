using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GenderPayGap.WebUI.Helpers.IntegrationChecksHelper;

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

        [HttpGet("database-integrity-checks/returns-with-figures-with-more-than-one-decimal-place")]
        public IActionResult ReturnsWithFiguresWithMoreThanOneDecimalPlace()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .AsEnumerable()
                    .Where(
                        r => HasMoreThanOneDecimalPlace(r.FemaleLowerPayBand)
                             || HasMoreThanOneDecimalPlace(r.MaleLowerPayBand)
                             || HasMoreThanOneDecimalPlace(r.FemaleMiddlePayBand)
                             || HasMoreThanOneDecimalPlace(r.MaleMiddlePayBand)
                             || HasMoreThanOneDecimalPlace(r.FemaleUpperPayBand)
                             || HasMoreThanOneDecimalPlace(r.MaleUpperPayBand)
                             || HasMoreThanOneDecimalPlace(r.FemaleUpperQuartilePayBand)
                             || HasMoreThanOneDecimalPlace(r.MaleUpperQuartilePayBand)
                             || HasMoreThanOneDecimalPlace(r.FemaleMedianBonusPayPercent)
                             || HasMoreThanOneDecimalPlace(r.MaleMedianBonusPayPercent)
                             || HasMoreThanOneDecimalPlace(r.DiffMeanBonusPercent)
                             || HasMoreThanOneDecimalPlace(r.DiffMedianBonusPercent)
                             || HasMoreThanOneDecimalPlace(r.DiffMedianHourlyPercent)
                             || HasMoreThanOneDecimalPlace(r.DiffMeanHourlyPayPercent))
                    .ToList();

            return PartialView("ReturnsWithFiguresWithMoreThanOneDecimalPlace", invalidReturns);
        }

        [HttpGet("database-integrity-checks/returns-with-quarters-figures-sum-different-than-100")]
        public IActionResult ReturnsWithQuartersFiguresSumDifferentThan100()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .Where(r => !r.OptedOutOfReportingPayQuarters)
                    .Where(
                        r => r.FemaleLowerPayBand + r.MaleLowerPayBand != 100
                             || r.FemaleMiddlePayBand + r.MaleMiddlePayBand != 100
                             || r.FemaleUpperPayBand + r.MaleUpperPayBand != 100
                             || r.FemaleUpperQuartilePayBand + r.MaleUpperQuartilePayBand != 100)
                    .ToList();

            return PartialView("ReturnsWithQuartersFiguresSumDifferentThan100", invalidReturns);
        }


        [HttpGet("database-integrity-checks/returns-with-invalid-quarters-figures")]
        public IActionResult ReturnsWithInvalidQuartersFigures()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .Where(HasInvalidPayQuarterFigures())
                    .ToList();

            return PartialView("ReturnsWithInvalidQuartersFigures", invalidReturns);
        }

        [HttpGet("database-integrity-checks/returns-with-invalid-mean-median-figures")]
        public IActionResult ReturnsWithInvalidMeanMedianFigures()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .Where(
                        r => r.DiffMedianHourlyPercent > 100 || r.DiffMedianHourlyPercent < -499.9m
                             || r.DiffMeanHourlyPayPercent > 100 || r.DiffMeanHourlyPayPercent < -499.9m)
                    .ToList();

            return PartialView("ReturnsWithInvalidMeanMedianFigures", invalidReturns);
        }

        [HttpGet("database-integrity-checks/returns-with-invalid-bonus-figures")]
        public IActionResult ReturnsWithInvalidBonusFigures()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .Where(
                        r => r.FemaleMedianBonusPayPercent > 100 || r.FemaleMedianBonusPayPercent < 0
                             || r.MaleMedianBonusPayPercent > 100 || r.MaleMedianBonusPayPercent < 0)
                    .ToList();

            return PartialView("ReturnsWithInvalidBonusFigures", invalidReturns);
        }

        [HttpGet("database-integrity-checks/returns-with-invalid-bonus-mean-median-figures")]
        public IActionResult ReturnsWithInvalidBonusMeanMedianFigures()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .Where(r => r.DiffMedianBonusPercent > 100 || r.DiffMeanBonusPercent > 100)
                    .ToList();

            return PartialView("ReturnsWithInvalidBonusMeanMedianFigures", invalidReturns);
        }

        [HttpGet("database-integrity-checks/returns-with-missing-figures")]
        public IActionResult ReturnsWithMissingFigures()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .Where(HasMissingFigures())
                    .ToList();

            return PartialView("ReturnsWithMissingFigures", invalidReturns);
        }

        [HttpGet("database-integrity-checks/private-employers-returns-without-responsible-person")]
        public IActionResult PrivateEmployersReturnsWithoutResponsiblePerson()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted && r.Organisation.SectorType == SectorTypes.Private)
                    .Where(r => String.IsNullOrEmpty(r.FirstName) || String.IsNullOrEmpty(r.LastName) || String.IsNullOrEmpty(r.JobTitle))
                    .ToList();

            return PartialView("PrivateEmployersReturnsWithoutResponsiblePerson", invalidReturns);
        }

        [HttpGet("database-integrity-checks/returns-with-invalid-bonus-figures-given-no-women-bonus")]
        public IActionResult ReturnsWithInvalidBonusFiguresGivenNoWomenBonus()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .Where(r => r.FemaleMedianBonusPayPercent == 0 && r.MaleMedianBonusPayPercent != 0)
                    .Where(r => r.DiffMeanBonusPercent != 100 && r.DiffMedianBonusPercent != 100)
                    .ToList();

            return PartialView("ReturnsWithInvalidBonusFiguresGivenNoWomenBonus", invalidReturns);
        }

        [HttpGet("database-integrity-checks/returns-with-invalid-text-fields-values")]
        public IActionResult ReturnsWithInvalidTextFieldsValues()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .Where(
                        r => r.CompanyLinkToGPGInfo != null && r.CompanyLinkToGPGInfo.Length > 255
                             || r.LateReason != null && r.LateReason.Length > 200)
                    .ToList();

            return PartialView("ReturnsWithInvalidTextFieldsValues", invalidReturns);
        }

        [HttpGet("database-integrity-checks/returns-with-invalid-company-link")]
        public IActionResult ReturnsWithInvalidCompanyLink()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .AsEnumerable()
                    .Where(r => !String.IsNullOrEmpty(r.CompanyLinkToGPGInfo)
                                && !(Uri.TryCreate(r.CompanyLinkToGPGInfo, UriKind.Absolute, out Uri uriResult)
                                     && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps)
                                    ))
                    .ToList();

            return PartialView("ReturnsWithInvalidCompanyLink", invalidReturns);
        }

        [HttpGet("database-integrity-checks/returns-with-invalid-opted-out-of-reporting-pay-quarters-value")]
        public IActionResult ReturnsWithInvalidOptedOutOfReportingPayQuartersValue()
        {
            List<Return> invalidReturns =
                dataRepository.GetAll<Return>()
                    .Where(r => r.OptedOutOfReportingPayQuarters)
                    .Where(IsNotReportingYearWithFurloughScheme())
                    .ToList();

            return PartialView("ReturnsWithInvalidOptedOutOfReportingPayQuartersValue", invalidReturns);
        }

        private Expression<Func<Return, bool>> HasInvalidPayQuarterFigures()
        {
            return r => (!r.OptedOutOfReportingPayQuarters
                         && (r.FemaleLowerPayBand > 100
                             || r.FemaleLowerPayBand < 0
                             || r.MaleLowerPayBand > 100
                             || r.MaleLowerPayBand < 0
                             || r.FemaleMiddlePayBand > 100
                             || r.FemaleMiddlePayBand < 0
                             || r.MaleMiddlePayBand > 100
                             || r.MaleMiddlePayBand < 0
                             || r.MaleUpperPayBand > 100
                             || r.MaleUpperPayBand < 0
                             || r.FemaleUpperPayBand > 100
                             || r.FemaleUpperPayBand < 0
                             || r.MaleUpperQuartilePayBand > 100
                             || r.MaleUpperQuartilePayBand < 0
                             || r.FemaleUpperQuartilePayBand > 100
                             || r.FemaleUpperQuartilePayBand < 0))
                        || (r.OptedOutOfReportingPayQuarters
                            && (r.FemaleLowerPayBand.HasValue
                                || r.MaleLowerPayBand.HasValue
                                || r.FemaleMiddlePayBand.HasValue
                                || r.MaleMiddlePayBand.HasValue
                                || r.FemaleUpperQuartilePayBand.HasValue
                                || r.MaleUpperQuartilePayBand.HasValue
                                || r.FemaleUpperPayBand.HasValue
                                || r.MaleUpperPayBand.HasValue));
        }

        private Expression<Func<Return, bool>> HasMissingFigures()
        {
            return r => (r.MaleMedianBonusPayPercent != 0
                         && (!r.DiffMeanBonusPercent.HasValue || !r.DiffMedianBonusPercent.HasValue))
                        || (!r.OptedOutOfReportingPayQuarters
                            && (!r.MaleLowerPayBand.HasValue
                                || !r.FemaleLowerPayBand.HasValue
                                || !r.MaleMiddlePayBand.HasValue
                                || !r.FemaleMiddlePayBand.HasValue
                                || !r.MaleUpperPayBand.HasValue
                                || !r.FemaleUpperPayBand.HasValue
                                || !r.MaleUpperQuartilePayBand.HasValue
                                || !r.FemaleUpperQuartilePayBand.HasValue));
        }

        private Expression<Func<Return, bool>> IsNotReportingYearWithFurloughScheme()
        {
            return r => !Global.ReportingStartYearsWithFurloughScheme.Contains(r.AccountingDate.Year);
        }

    }
}
