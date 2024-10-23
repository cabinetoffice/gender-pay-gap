﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Search;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class SearchController : Controller
    {

        private readonly ICompareViewService compareViewService;
        private readonly IDataRepository dataRepository;
        private readonly ViewingSearchService viewingSearchService;
        private readonly AutoCompleteSearchService autoCompleteSearchService;
        
        public SearchController(
            ICompareViewService compareViewService,
            IDataRepository dataRepository,
            ViewingSearchService viewingSearchService,
            AutoCompleteSearchService autoCompleteSearchService)
        {
            this.compareViewService = compareViewService;
            this.dataRepository = dataRepository;
            this.viewingSearchService = viewingSearchService;
            this.autoCompleteSearchService = autoCompleteSearchService;
        }

        [HttpGet("search")]
        public IActionResult SearchPage(SearchPageViewModel viewModel)
        {
            compareViewService.LoadComparedEmployersFromCookie();

            PopulateViewModel(viewModel);
            
            return View("SearchPage", viewModel);
        }

        private void PopulateViewModel(SearchPageViewModel viewModel)
        {
            viewModel.PossibleSectors = dataRepository.GetAll<SicSection>().ToList();
            viewModel.PossibleReportedLateYears =
                ReportingYearsHelper.GetReportingYears()
                    .Where(year => year != ReportingYearsHelper.GetCurrentReportingYear())
                    .OrderByDescending(year => year)
                    .ToList();
        }

        [HttpGet("search-api")]
        public IActionResult SearchApi(SearchPageViewModel viewModel)
        {
            return Json(viewingSearchService.Search(viewModel));
        }

        [HttpGet("search/suggest-employer-name-js")]
        // Used to generate suggestions for the search on the landing page 
        public IActionResult SuggestEmployerNameJs(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return Json(new {ErrorCode = HttpStatusCode.BadRequest, ErrorMessage = "Cannot search for a null or empty value"});
            }

            List<SuggestOrganisationResult> matches = autoCompleteSearchService.Search(search);

            return Json(new {Matches = matches});
        }

    }
}