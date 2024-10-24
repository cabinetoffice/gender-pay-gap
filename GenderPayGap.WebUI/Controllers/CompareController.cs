﻿using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mime;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.BusinessLogic.Models.Compare;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("viewing")]
    public class CompareController : Controller
    {

        private readonly IDataRepository dataRepository;
        private readonly IWebTracker webTracker;
        
        public CompareController(ICompareViewService compareViewService,
            IDataRepository dataRepository,
            IOrganisationBusinessLogic organisationBusinessLogic,
            IWebTracker webTracker)
        {
            OrganisationBusinessLogic = organisationBusinessLogic;
            CompareViewService = compareViewService;
            this.dataRepository = dataRepository;
            this.webTracker = webTracker;
        }

        [HttpGet("add-employer/{employerIdentifier}")]
        public IActionResult AddEmployer(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");
            }

            //Get the employer from the encrypted identifier
            EmployerSearchModel employer = GetEmployer(employerIdentifier);
            
            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            //Add the employer to the compare list
            CompareViewService.AddToBasket(employer.OrganisationIdEncrypted);

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            //Redirect the user to the original page
            return LocalRedirect(returnUrl);
        }

        [HttpGet("add-employer-js/{employerIdentifier}")]
        public IActionResult AddEmployerJs(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");
            }

            //Get the employer from the encrypted identifier
            EmployerSearchModel employer = GetEmployer(employerIdentifier);

            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            //Add the employer to the compare list
            CompareViewService.AddToBasket(employer.OrganisationIdEncrypted);

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            //Setup compare basket
            bool fromSearchResults = returnUrl.Contains("/search-results");
            bool fromEmployer = returnUrl.StartsWithI("/employer");
            ViewBag.BasketViewModel = new CompareBasketViewModel {
                CanAddEmployers = false,
                CanClearCompare = true,
                CanViewCompare = fromSearchResults && CompareViewService.BasketItemCount > 1
                                 || fromEmployer && CompareViewService.BasketItemCount > 0,
                IsSearchPage = fromSearchResults,
                IsEmployerPage = fromEmployer
            };

            ViewBag.ReturnUrl = returnUrl;

            var model = new AddRemoveButtonViewModel {
                OrganisationIdEncrypted = employer.OrganisationIdEncrypted, OrganisationName = employer.Name
            };

            return PartialView("Basket_Button", model);
        }

        [HttpGet("remove-employer/{employerIdentifier}")]
        public IActionResult RemoveEmployer(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");
            }

            //Get the employer from the encrypted identifier
            EmployerSearchModel employer = GetEmployer(employerIdentifier);

            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            //Remove the employer from the list
            CompareViewService.RemoveFromBasket(employer.OrganisationIdEncrypted);

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            return LocalRedirect(returnUrl);
        }

        [HttpGet("remove-employer-js/{employerIdentifier}")]
        public IActionResult RemoveEmployerJs(string employerIdentifier, string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult($"Missing {nameof(employerIdentifier)}");
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");
            }

            //Get the employer from the encrypted identifier
            EmployerSearchModel employer = GetEmployer(employerIdentifier);

            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            //Remove the employer from the list
            CompareViewService.RemoveFromBasket(employer.OrganisationIdEncrypted);

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            //Setup compare basket
            bool fromSearchResults = returnUrl.Contains("/search-results");
            bool fromEmployer = returnUrl.StartsWithI("/employer");
            ViewBag.BasketViewModel = new CompareBasketViewModel {
                CanAddEmployers = false,
                CanClearCompare = true,
                CanViewCompare = fromSearchResults && CompareViewService.BasketItemCount > 1
                                 || fromEmployer && CompareViewService.BasketItemCount > 0,
                IsSearchPage = fromSearchResults,
                IsEmployerPage = fromEmployer
            };

            ViewBag.ReturnUrl = returnUrl;

            var model = new AddRemoveButtonViewModel {
                OrganisationIdEncrypted = employer.OrganisationIdEncrypted, OrganisationName = employer.Name
            };

            return PartialView("Basket_Button", model);
        }

        [HttpGet("clear-employers")]
        public IActionResult ClearEmployers(string returnUrl)
        {
            //Check the parameters are populated
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return new HttpBadRequestResult($"Missing {nameof(returnUrl)}");
            }

            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            CompareViewService.ClearBasket();

            //Save the compared employers to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            return LocalRedirect(returnUrl);
        }

        [HttpGet("~/compare-employers/{year:int=0}")]
        public IActionResult CompareEmployers(int year, string employers = null, string sortColumn = null, bool sortAscending = true)
        {
            if (year == 0)
            {
                year = ReportingYearsHelper.GetTheMostRecentCompletedReportingYear();
            }

            // Load the current compared employers from the cookie
            CompareViewService.LoadComparedEmployersFromCookie();

            //Load employers from querystring (via shared email)
            if (!string.IsNullOrWhiteSpace(employers))
            {
                string[] comparedEmployers = employers.SplitI("-");
                if (comparedEmployers.Any())
                {
                    CompareViewService.ClearBasket();
                    CompareViewService.AddRangeToBasket(comparedEmployers);
                    return RedirectToAction("CompareEmployers", new {year});
                }
            }

            ViewBag.ReturnUrl = Url.Action("CompareEmployers", new {year});

            //Get the compare basket organisations
            IEnumerable<CompareReportModel> compareReports = OrganisationBusinessLogic.GetCompareData(
                CompareViewService.ComparedEmployers,
                year,
                sortColumn,
                sortAscending);

            //Generate the shared links
            string shareEmailUrl = Url.Action(
                nameof(CompareEmployers),
                "Compare",
                new {year, employers = CompareViewService.ComparedEmployers.ToDelimitedString("-")},
                Request.Scheme);

            ViewBag.BasketViewModel = new CompareBasketViewModel {CanAddEmployers = true, CanViewCompare = false, CanClearCompare = true};

            return View(
                "CompareEmployers",
                new CompareViewModel {
                    CompareReports = compareReports,
                    CompareBasketCount = CompareViewService.BasketItemCount,
                    ShareEmailUrl =
                        CompareViewService.BasketItemCount <= CompareViewService.MaxCompareBasketShareCount ? shareEmailUrl : null,
                    Year = year,
                    SortAscending = sortAscending,
                    SortColumn = sortColumn
                });
        }

        [ValidateAntiForgeryToken]
        [HttpPost("~/compare-employers/{year:int=0}")]
        public IActionResult CompareEmployers(string command, int year = 0)
        {
            if (year == 0)
            {
                year = ReportingYearsHelper.GetTheMostRecentCompletedReportingYear();
            }

            string args = command.AfterFirst(":");
            command = command.BeforeFirst(":");

            switch (command.ToLower())
            {
                case "employer":
                    return RedirectToAction("ViewEmployerPage", "Redirect", new {employerIdentifier = args});
                case "report":
                    return RedirectToAction(nameof(ViewingController.Report), "Viewing", new {employerIdentifier = args, year});
            }

            return new HttpBadRequestResult($"Invalid command '{command}'");
        }

        [HttpGet("download-compare-data")]
        public IActionResult DownloadCompareData(int year = 0)
        {
            if (year == 0)
            {
                year = ReportingYearsHelper.GetTheMostRecentCompletedReportingYear();
            }

            var result = CompareEmployers(year) as ViewResult;
            var viewModel = result.Model as CompareViewModel;
            IEnumerable<CompareReportModel> data = viewModel?.CompareReports;

            //Ensure we some data
            if (data == null || !data.Any())
            {
                throw new PageNotFoundException();
            }

            DataTable model = OrganisationBusinessLogic.GetCompareDatatable(data);

            //Setup the HTTP response
            var contentDisposition = new ContentDisposition {
                FileName = $"Compared GPG Data {ReportingYearsHelper.FormatYearAsReportingPeriod(year)}.csv", Inline = false
            };
            HttpContext.SetResponseHeader("Content-Disposition", contentDisposition.ToString());

            /* No Longer required as AspNetCore has response buffering on by default
            Response.BufferOutput = true;
            */
            //Track the download 
            webTracker.TrackPageView(this, contentDisposition.FileName);

            //Return the data
            return Content(model.ToCSV(), "text/csv");
        }

        #region Helpers

        private EmployerSearchModel GetEmployer(string employerIdentifier, bool activeOnly = true)
        {
            long organisationId = ControllerHelper.DeObfuscateOrganisationIdOrThrow404(employerIdentifier);
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);

            if (activeOnly)
            {
                ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);
            }

            EmployerSearchModel employer = organisation.ToEmployerSearchResult();

            return employer;
        }

        #endregion

        #region Dependencies

        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }

        public ICompareViewService CompareViewService { get; }

        #endregion

    }

}
