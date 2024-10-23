using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Compare;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class CompareEmployersController : Controller
    {

        private readonly ICompareViewService compareViewService;
        private readonly IDataRepository dataRepository;

        public CompareEmployersController(
            ICompareViewService compareViewService,
            IDataRepository dataRepository)
        {
            this.compareViewService = compareViewService;
            this.dataRepository = dataRepository;
        }

        [HttpGet("/compare-employers/add/{organisationId}")]
        public IActionResult AddEmployer(long organisationId, string returnUrl)
        {
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            
            compareViewService.LoadComparedEmployersFromCookie();
            compareViewService.AddToBasket(organisationId);
            compareViewService.SaveComparedEmployersToCookie();

            return LocalRedirect(returnUrl);
        }
        
        [HttpGet("/compare-employers/add-js/{organisationId}")]
        public IActionResult AddEmployerJs(long organisationId)
        {
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            
            compareViewService.LoadComparedEmployersFromCookie();
            compareViewService.AddToBasket(organisationId);
            compareViewService.SaveComparedEmployersToCookie();

            return Ok();
        }
        
        [HttpGet("/compare-employers/remove/{organisationId}")]
        public IActionResult RemoveEmployer(long organisationId, string returnUrl)
        {
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            
            compareViewService.LoadComparedEmployersFromCookie();
            compareViewService.RemoveFromBasket(organisationId);
            compareViewService.SaveComparedEmployersToCookie();

            return LocalRedirect(returnUrl);
        }
        
        [HttpGet("/compare-employers/remove-js/{organisationId}")]
        public IActionResult RemoveEmployerJs(long organisationId)
        {
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            
            compareViewService.LoadComparedEmployersFromCookie();
            compareViewService.RemoveFromBasket(organisationId);
            compareViewService.SaveComparedEmployersToCookie();

            return Ok();
        }
        
        [HttpGet("/compare-employers/clear")]
        public IActionResult ClearEmployers(string returnUrl)
        {
            compareViewService.LoadComparedEmployersFromCookie();
            compareViewService.ClearBasket();
            compareViewService.SaveComparedEmployersToCookie();

            return LocalRedirect(returnUrl);
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
            compareViewService.LoadComparedEmployersFromCookie();
            
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
                organisationIds = DecodeOrganisationIds(compareViewService.ComparedEmployers);
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
            compareViewService.LoadComparedEmployersFromCookie();
            
            ControllerHelper.ThrowIfReportingYearIsOutsideOfRangeForAnyOrganisation(year);

            List<long> organisationIds;
            if (!string.IsNullOrWhiteSpace(employers))
            {
                List<string> encodedEmployerIds = employers.Split("-", StringSplitOptions.RemoveEmptyEntries).ToList();
                organisationIds = DecodeOrganisationIds(encodedEmployerIds);
            }
            else
            {
                organisationIds = DecodeOrganisationIds(compareViewService.ComparedEmployers);
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
                            NumberOfEmployees = hasEmployerReportedForYear ? returnForYear.OrganisationSize.GetAttribute<DisplayAttribute>().Name : "",
                            
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
