using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Compare;
using GenderPayGap.WebUI.Services;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class CompareEmployersController : Controller
    {

        private readonly ComparisonBasketService comparisonBasketService;
        private readonly IDataRepository dataRepository;

        public CompareEmployersController(
            ComparisonBasketService comparisonBasketService,
            IDataRepository dataRepository)
        {
            this.comparisonBasketService = comparisonBasketService;
            this.dataRepository = dataRepository;
        }

        [HttpGet("/compare-employers/add/{organisationId}")]
        public IActionResult AddEmployer(long organisationId, string returnUrl)
        {
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            
            comparisonBasketService.LoadComparedEmployersFromCookie();
            comparisonBasketService.AddToBasket(organisationId);
            comparisonBasketService.SaveComparedEmployersToCookie();

            return !string.IsNullOrWhiteSpace(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction("CompareEmployersNoYear", "CompareEmployers");
        }
        
        [HttpGet("/compare-employers/add-js/{organisationId}")]
        public IActionResult AddEmployerJs(long organisationId)
        {
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            
            comparisonBasketService.LoadComparedEmployersFromCookie();
            comparisonBasketService.AddToBasket(organisationId);
            comparisonBasketService.SaveComparedEmployersToCookie();

            return Ok();
        }
        
        [HttpGet("/compare-employers/remove/{organisationId}")]
        public IActionResult RemoveEmployer(long organisationId, string returnUrl)
        {
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            
            comparisonBasketService.LoadComparedEmployersFromCookie();
            comparisonBasketService.RemoveFromBasket(organisationId);
            comparisonBasketService.SaveComparedEmployersToCookie();

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return comparisonBasketService.ComparedEmployers.Any()
                ? RedirectToAction("CompareEmployersNoYear", "CompareEmployers")
                : RedirectToAction("SearchPage", "Search");
        }
        
        [HttpGet("/compare-employers/remove-js/{organisationId}")]
        public IActionResult RemoveEmployerJs(long organisationId)
        {
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            
            comparisonBasketService.LoadComparedEmployersFromCookie();
            comparisonBasketService.RemoveFromBasket(organisationId);
            comparisonBasketService.SaveComparedEmployersToCookie();

            return Ok();
        }
        
        [HttpGet("/compare-employers/clear")]
        public IActionResult ClearEmployers(string returnUrl)
        {
            comparisonBasketService.LoadComparedEmployersFromCookie();
            comparisonBasketService.ClearBasket();
            comparisonBasketService.SaveComparedEmployersToCookie();

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return comparisonBasketService.ComparedEmployers.Any()
                ? RedirectToAction("CompareEmployersNoYear", "CompareEmployers")
                : RedirectToAction("SearchPage", "Search");
        }
        
        [HttpGet("/compare-employers")]
        public IActionResult CompareEmployersNoYear(string employers = null)
        {
            int year = ReportingYearsHelper.GetTheMostRecentCompletedReportingYear();
            RedirectToActionResult action = !string.IsNullOrWhiteSpace(employers)
                ? RedirectToAction("CompareEmployersForYear", new {year = year, employers = employers})
                : RedirectToAction("CompareEmployersForYear", new {year = year});
            return action;
        }

        [HttpGet("/compare-employers/{year}")]
        public IActionResult CompareEmployersForYear(int year, string employers = null)
        {
            comparisonBasketService.LoadComparedEmployersFromCookie();
            comparisonBasketService.SaveComparedEmployersToCookieIfAnyAreObfuscated();
            
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRangeForAnyOrganisation(year);
            var viewModel = new CompareEmployersForYearViewModel
            {
                ReportingYear = year
            };

            List<long> organisationIds;
            if (!string.IsNullOrWhiteSpace(employers))
            {
                List<string> encodedEmployerIds = employers.Split("-", StringSplitOptions.RemoveEmptyEntries).ToList();
                organisationIds = DecodeOrganisationIds(encodedEmployerIds);
                viewModel.CameFromShareLink = true;
            }
            else
            {
                organisationIds = comparisonBasketService.ComparedEmployers;
                viewModel.CameFromShareLink = false;
            }

            foreach (long organisationId in organisationIds)
            {
                try
                {
                    Organisation organisation = dataRepository.Get<Organisation>(organisationId);
                    viewModel.Organisations.Add(organisation);
                }
                catch (Exception e)
                {}
            }

            return View("CompareEmployersForYear", viewModel);
        }

        [HttpGet("/compare-employers/{year}/download-csv")]
        public IActionResult DownloadCSVOfCompareEmployersForYear(int year, string employers = null)
        {
            comparisonBasketService.LoadComparedEmployersFromCookie();
            comparisonBasketService.SaveComparedEmployersToCookieIfAnyAreObfuscated();
            
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRangeForAnyOrganisation(year);

            List<long> organisationIds;
            if (!string.IsNullOrWhiteSpace(employers))
            {
                List<string> encodedEmployerIds = employers.Split("-", StringSplitOptions.RemoveEmptyEntries).ToList();
                organisationIds = DecodeOrganisationIds(encodedEmployerIds);
            }
            else
            {
                organisationIds = comparisonBasketService.ComparedEmployers;
            }
            
            var organisationsToDownload = new List<Organisation>();
            foreach (long organisationId in organisationIds)
            {
                try
                {
                    Organisation organisation = dataRepository.Get<Organisation>(organisationId);
                    organisationsToDownload.Add(organisation);
                }
                catch (Exception e)
                {}
            }

            var records = organisationsToDownload.OrderBy(org => org.OrganisationName).Select(
                    organisation =>
                    {
                        Return returnForYear = organisation.GetReturn(year);
                        bool hasEmployerReportedForYear = returnForYear != null;
                        ReportStatusTag reportStatusTag = ReportStatusTagHelper.GetReportStatusTag(organisation, year);

                        return new
                        {
                            EmployerName = organisation.OrganisationName,
                            ReportStatus = reportStatusTag.ToString(),
                            NumberOfEmployees = hasEmployerReportedForYear ? returnForYear.OrganisationSize.GetDisplayName() : "",
                            
                            GenderPayGap_HourlyPay_Mean_Percent = hasEmployerReportedForYear ? returnForYear.DiffMeanHourlyPayPercent.ToString() : "",
                            GenderPayGap_HourlyPay_Median_Percent = hasEmployerReportedForYear ? returnForYear.DiffMedianHourlyPercent.ToString() : "",
                            
                            WomenInEachPayQuarter_Lower_Percent = hasEmployerReportedForYear ? returnForYear.FemaleLowerPayBand.ToString() : "",
                            WomenInEachPayQuarter_LowerMiddle_Percent = hasEmployerReportedForYear ? returnForYear.FemaleMiddlePayBand.ToString() : "",
                            WomenInEachPayQuarter_UpperMiddle_Percent = hasEmployerReportedForYear ? returnForYear.FemaleUpperPayBand.ToString() : "",
                            WomenInEachPayQuarter_Upper_Percent = hasEmployerReportedForYear ? returnForYear.FemaleUpperQuartilePayBand.ToString() : "",
                            
                            WhoReceivesBonusPay_Women_Percent = hasEmployerReportedForYear ? returnForYear.FemaleMedianBonusPayPercent.ToString( ): "",
                            WhoReceivesBonusPay_Men_Percent = hasEmployerReportedForYear ? returnForYear.MaleMedianBonusPayPercent.ToString( ): "",
                            
                            GenderPayGap_BonusPay_Mean_Percent = hasEmployerReportedForYear && returnForYear.DiffMeanBonusPercent.HasValue
                                                                 ? returnForYear.DiffMeanBonusPercent.Value.ToString()
                                                                 : "",
                            GenderPayGap_BonusPay_Median_Percent = hasEmployerReportedForYear && returnForYear.DiffMedianBonusPercent.HasValue
                                                                   ? returnForYear.DiffMedianBonusPercent.Value.ToString()
                                                                   : "",
                        };
                    })
                .ToList();
            
            string fileDownloadName = $"UK Gender Pay Gap Data for selected employers {ReportingYearsHelper.FormatYearAsReportingPeriod(year)}.csv";
            return DownloadHelper.CreateCsvDownload(records, fileDownloadName);
        }

        private List<long> DecodeOrganisationIds(List<string> encodedEmployerIds)
        {
            var organisationIds = new List<long>();
            
            foreach (string encodedEmployerId in encodedEmployerIds)
            {
                try
                {
                    if (long.TryParse(encodedEmployerId, out long parsedOrganisationId))
                    {
                        organisationIds.Add(parsedOrganisationId);
                    }
                    else
                    {
                        long deObfuscatedOrganisationId = Obfuscator.DeObfuscate(encodedEmployerId);
                        organisationIds.Add(deObfuscatedOrganisationId);
                    }
                }
                catch (Exception e)
                {}
            }
            return organisationIds;
        }

    }
}
