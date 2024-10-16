using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Search;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("viewing")]
    public class ViewingController : Controller
    {
        private readonly AutoCompleteSearchService autoCompleteSearchService;
        private readonly IDataRepository dataRepository;

        #region Constructors

        public ViewingController(
            IViewingService viewingService,
            ICompareViewService compareViewService,
            IOrganisationBusinessLogic organisationBusinessLogic,
            IDataRepository dataRepository,
            AutoCompleteSearchService autoCompleteSearchService)
        {
            ViewingService = viewingService;
            CompareViewService = compareViewService;
            OrganisationBusinessLogic = organisationBusinessLogic;
            this.autoCompleteSearchService = autoCompleteSearchService;
            this.dataRepository = dataRepository;
        }

        #endregion

        #region Dependencies

        public IViewingService ViewingService { get; }
        public ICompareViewService CompareViewService { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }

        #endregion

        #region Search

        /// <summary>
        /// </summary>
        /// <param name="searchQuery"></param>
        [HttpGet("~/search-results")]
        [HttpGet("search-results")]
        public async Task<IActionResult> SearchResults([FromQuery] SearchResultsQuery searchQuery, string orderBy = "relevance")
        {
            //If no compare employers in session then load employers from the cookie
            if (CompareViewService.BasketItemCount == 0)
            {
                CompareViewService.LoadComparedEmployersFromCookie();
            }

            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out HttpStatusViewResult result))
            {
                return result;
            }

            // generate result view model
            var searchParams = SearchResultsQueryToEmployerSearchParameters(searchQuery);
            SearchViewModel model = ViewingService.Search(searchParams, orderBy);
            ViewBag.ReturnUrl = Url.Action("SearchResults", "Viewing");

            ViewBag.BasketViewModel = new CompareBasketViewModel {
                CanAddEmployers = false, CanViewCompare = CompareViewService.BasketItemCount > 1, CanClearCompare = true
            };

            return View("Finder/SearchResults", model);
        }

        [HttpGet("~/search-results-js")]
        [HttpGet("search-results-js")]
        // used to generate suggestions for the search on the landing page 
        public async Task<IActionResult> SearchResultsJs([FromQuery] SearchResultsQuery searchQuery)
        {
            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out HttpStatusViewResult result))
            {
                return result;
            }

            // generate result view model
            var searchParams = SearchResultsQueryToEmployerSearchParameters(searchQuery);
            SearchViewModel model = ViewingService.Search(searchParams, "relevance");

            ViewBag.ReturnUrl = Url.Action("SearchResults", "Viewing");

            return PartialView("Finder/Parts/MainContent", model);
        }

        [HttpGet("suggest-employer-name-js")]
        public IActionResult SuggestEmployerNameJs(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return Json(new {ErrorCode = HttpStatusCode.BadRequest, ErrorMessage = "Cannot search for a null or empty value"});
            }

            List<SuggestOrganisationResult> matches = autoCompleteSearchService.Search(search);

            return Json(new {Matches = matches});
        }
        
        #endregion

        #region Reports

        [HttpGet("~/EmployerReport/{employerIdentifier}/{year}")]
        public IActionResult Report(string employerIdentifier, int year)
        {
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult("Missing employer identifier");
            }

            if (year < Global.FirstReportingYear || year > VirtualDateTime.Now.Year)
            {
                return new HttpBadRequestResult($"Invalid snapshot year {year}");
            }

            #region Load organisation

            Organisation foundOrganisation;

            long organisationId = ControllerHelper.DeObfuscateOrganisationIdOrThrow404(employerIdentifier);
            foundOrganisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(foundOrganisation);

            #endregion

            #region Load latest submission 

            try
            {
                Return foundReturn = foundOrganisation.GetReturn(year);

                if (foundReturn == null)
                {
                    throw new PageNotFoundException();
                }
                
                ViewBag.BasketViewModel = new CompareBasketViewModel {CanAddEmployers = true, CanViewCompare = true};

                return View("EmployerDetails/Report", foundReturn);
            }
            catch (Exception ex)
            {
                throw new PageNotFoundException();
            }

            #endregion
        }

        [HttpGet("add-search-results-to-compare")]
        public async Task<IActionResult> AddSearchResultsToCompare([FromQuery] SearchResultsQuery searchQuery, string orderBy = "relevance")
        {
            if (!searchQuery.TryValidateSearchParams(out HttpStatusViewResult result))
            {
                return result;
            }

            // generate compare list
            var searchParams = SearchResultsQueryToEmployerSearchParameters(searchQuery);

            // set maximum search size
            searchParams.Page = 1;
            searchParams.PageSize = CompareViewService.MaxCompareBasketCount;
            SearchViewModel searchResultsModel = ViewingService.Search(searchParams, orderBy);

            // add any new items to the compare list
            string[] resultIds = searchResultsModel.Employers.Results
                .Where(employer => CompareViewService.BasketContains(employer.OrganisationIdEncrypted) == false)
                .Take(CompareViewService.MaxCompareBasketCount - CompareViewService.BasketItemCount)
                .Select(employer => employer.OrganisationIdEncrypted)
                .ToArray();

            CompareViewService.AddRangeToBasket(resultIds);

            // save the results to the cookie
            CompareViewService.SaveComparedEmployersToCookie(Request);

            return RedirectToAction(nameof(SearchResults), "Viewing", searchQuery);
        }

        #endregion

        private EmployerSearchParameters SearchResultsQueryToEmployerSearchParameters(SearchResultsQuery searchQuery)
        {
            return new EmployerSearchParameters
            {
                Keywords = searchQuery.search,
                Page = searchQuery.p,
                FilterSicSectionIds = searchQuery.s ?? new List<char>(),
                FilterEmployerSizes = searchQuery.es ?? new List<int>(),
                FilterReportedYears = searchQuery.y ?? new List<int>(),
                FilterReportingStatus = searchQuery.st ?? new List<int>(),
                SearchType = searchQuery.t
            };
        }

    }
}
