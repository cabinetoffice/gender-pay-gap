using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Search;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("viewing")]
    public class ViewingController : BaseController
    {
        private readonly AutoCompleteSearchService autoCompleteSearchService;
        private readonly IFileRepository fileRepository;

        #region Constructors

        public ViewingController(
            IHttpSession session,
            IViewingService viewingService,
            ISearchViewService searchViewService,
            ICompareViewService compareViewService,
            IOrganisationBusinessLogic organisationBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic,
            IObfuscator obfuscator,
            IDataRepository dataRepository,
            IWebTracker webTracker,
            AutoCompleteSearchService autoCompleteSearchService,
            IFileRepository fileRepository) : base(session, dataRepository, webTracker)
        {
            ViewingService = viewingService;
            SearchViewService = searchViewService;
            CompareViewService = compareViewService;
            OrganisationBusinessLogic = organisationBusinessLogic;
            Obfuscator = obfuscator;
            SubmissionBusinessLogic = submissionBusinessLogic;
            this.autoCompleteSearchService = autoCompleteSearchService;
            this.fileRepository = fileRepository;
        }

        #endregion

        private string DecodeEmployerIdentifier(string employerIdentifier, out ActionResult actionResult)
        {
            string result = string.Empty;
            actionResult = new EmptyResult();

            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                actionResult = new HttpBadRequestResult("Missing employer identifier");
            }

            try
            {
                result = HttpUtility.UrlDecode(Encryption.DecryptQuerystring(employerIdentifier));
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"Cannot decrypt return id from '{employerIdentifier}'", ex);
                actionResult = View("CustomError", new ErrorViewModel(400));
            }

            return result;
        }

        #region Dependencies

        public IViewingService ViewingService { get; }
        public ISearchViewService SearchViewService { get; }
        public ICompareViewService CompareViewService { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }
        public ISubmissionBusinessLogic SubmissionBusinessLogic { get; set; }
        public IObfuscator Obfuscator { get; }

        #endregion

        [HttpGet("~/")]
        public IActionResult Index()
        {
            if (FeatureFlagHelper.IsFeatureEnabled(FeatureFlag.ReportingStepByStep))
            {
                return View("Launchpad/PrototypeIndex");
            }
            else
            {
                return View("Launchpad/Index");
            }
        }

        [HttpGet]
        public IActionResult Redirect()
        {
            return RedirectToActionPermanent("Index");
        }

        [HttpGet("azure-uptime-pinger")]
        public IActionResult AzureUptimePingerEndpoint()
        {
            return RedirectToActionPermanent("Index");
        }

        #region Search

        /// <summary>
        /// </summary>
        /// <param name="searchQuery"></param>
        [HttpGet("~/search-results")]
        [HttpGet("search-results")]
        public async Task<IActionResult> SearchResults([FromQuery] SearchResultsQuery searchQuery, string orderBy = "relevance")
        {
            //When never searched in this session
            if (string.IsNullOrWhiteSpace(SearchViewService.LastSearchParameters))
            {
                //If no compare employers in session then load employers from the cookie
                if (CompareViewService.BasketItemCount == 0)
                {
                    CompareViewService.LoadComparedEmployersFromCookie();
                }
            }

            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out HttpStatusViewResult result))
            {
                return result;
            }

            // generate result view model
            var searchParams = SearchResultsQueryToEmployerSearchParameters(searchQuery);
            SearchViewModel model = await ViewingService.SearchAsync(searchParams, orderBy);
            ViewBag.ReturnUrl = SearchViewService.GetLastSearchUrl();

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
            SearchViewModel model = await ViewingService.SearchAsync(searchParams, "relevance");

            ViewBag.ReturnUrl = SearchViewService.GetLastSearchUrl();

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

        #region Downloads

        [HttpGet("download")]
        public IActionResult Download()
        {
            return View("Download/Download");
        }

        [HttpGet("download-data")]
        [HttpGet("download-data/{year:int=0}")]
        public IActionResult DownloadData(int year = 0)
        {
            if (year == 0)
            {
                year = SectorTypes.Private.GetAccountingStartDate().Year;
            }

            string filePath = Path.Combine(Global.DownloadsLocation, $"GPGData_{year}-{year + 1}.csv");
            string fileContents = fileRepository.Read(filePath);

            string userFacingDownloadFileName = $"UK Gender Pay Gap Data - {year} to {year + 1}.csv";

            //Track the download 
            WebTracker.TrackPageView(this, userFacingDownloadFileName);

            return DownloadHelper.CreateCsvDownload(fileContents, userFacingDownloadFileName);
        }

        #endregion

        #region Employer details

        [HttpGet("employer-details")]
        [Obsolete("Please use method 'Employer' instead.")] //, true)]
        public IActionResult EmployerDetails(string e = null, int y = 0, string id = null)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                Organisation organisation;

                try
                {
                    CustomResult<Organisation> organisationLoadingOutcome =
                        OrganisationBusinessLogic.GetOrganisationByEncryptedReturnId(id);

                    if (organisationLoadingOutcome.Failed)
                    {
                        return organisationLoadingOutcome.ErrorMessage.ToHttpStatusViewResult();
                    }

                    organisation = organisationLoadingOutcome.Result;
                }
                catch (Exception ex)
                {
                    CustomLogger.Error("Cannot decrypt return id from query string", ex);
                    return View("CustomError", new ErrorViewModel(400));
                }

                string organisationIdEncrypted = organisation.GetEncryptedId();
                return RedirectToActionPermanent(nameof(Employer), new {employerIdentifier = organisationIdEncrypted});
            }

            if (string.IsNullOrWhiteSpace(e))
            {
                return new HttpBadRequestResult("EmployerDetails: \'e\' query parameter was null or white space");
            }

            return RedirectToActionPermanent(nameof(Employer), new {employerIdentifier = e});
        }

        [HttpGet("employer-{employerIdentifier}")]
        [Obsolete("Please use method 'Employer' instead.")] // , true)]
        public IActionResult EmployerDeprecated(string employerIdentifier)
        {
            string decodedEmployerIdentifier = DecodeEmployerIdentifier(employerIdentifier, out ActionResult actionResult);

            if (string.IsNullOrEmpty(decodedEmployerIdentifier))
            {
                return actionResult;
            }

            int employerIdentifierId = employerIdentifier.ToInt32();
            string shortUrlObfuscatedEmployerIdentifier = Obfuscator.Obfuscate(employerIdentifierId);

            return RedirectToActionPermanent(nameof(Employer), new {employerIdentifier = shortUrlObfuscatedEmployerIdentifier});
        }

        [HttpGet("~/Employer/{employerIdentifier}")]
        public IActionResult Employer(string employerIdentifier, int? page = 1)
        {
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult("Missing employer identifier");
            }

            CustomResult<Organisation> organisationLoadingOutcome;

            try
            {
                organisationLoadingOutcome = OrganisationBusinessLogic.LoadInfoFromActiveEmployerIdentifier(employerIdentifier);

                if (organisationLoadingOutcome.Failed)
                {
                    return organisationLoadingOutcome.ErrorMessage.ToHttpStatusViewResult();
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"Cannot decrypt return employerIdentifier from '{employerIdentifier}'", ex);
                return View("CustomError", new ErrorViewModel(400));
            }

            ViewBag.BasketViewModel = new CompareBasketViewModel {CanAddEmployers = true, CanViewCompare = true};
            
            var totalEntries = organisationLoadingOutcome.Result.GetRecentReports(Global.ShowReportYearCount).Count() + 1; // Years we report for + the year they joined
            var maxEntriesPerPage = 10;
            var totalPages = (int)Math.Ceiling((double)totalEntries / maxEntriesPerPage);

            if (page < 1)
            {
                page = 1;
            }

            if (page > totalPages)
            {
                page = totalPages;
            }

            return View(
                "EmployerDetails/Employer",
                new EmployerDetailsViewModel {
                    Organisation = organisationLoadingOutcome.Result,
                    ComparedEmployers = CompareViewService.ComparedEmployers.Value,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    EntriesPerPage = maxEntriesPerPage
                });
        }

        #endregion

        #region Reports

        [HttpGet("employer-{employerIdentifier}/report-{year}")]
        [Obsolete("ReportDeprecated is (unsurprisingly) deprecated, please use method 'Report' instead.")] // , true)]
        public IActionResult ReportDeprecated(string employerIdentifier, int year)
        {
            if (year < Global.FirstReportingYear || year > VirtualDateTime.Now.Year)
            {
                return new HttpBadRequestResult($"Invalid snapshot year {year}");
            }

            string decodedEmployerIdentifier = DecodeEmployerIdentifier(employerIdentifier, out ActionResult actionResult);

            if (string.IsNullOrEmpty(decodedEmployerIdentifier))
            {
                return actionResult;
            }

            int employerIdentifierId = decodedEmployerIdentifier.ToInt32();
            string shortUrlObfuscatedEmployerIdentifier = Obfuscator.Obfuscate(employerIdentifierId);

            return RedirectToActionPermanent(nameof(Report), new {employerIdentifier = shortUrlObfuscatedEmployerIdentifier, year});
        }


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

            CustomResult<Organisation> organisationLoadingOutcome;

            try
            {
                organisationLoadingOutcome = OrganisationBusinessLogic.LoadInfoFromActiveEmployerIdentifier(employerIdentifier);

                if (organisationLoadingOutcome.Failed)
                {
                    return organisationLoadingOutcome.ErrorMessage.ToHttpStatusViewResult();
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"Cannot decrypt return employerIdentifier from '{employerIdentifier}'", ex);
                return View("CustomError", new ErrorViewModel(400));
            }

            Organisation foundOrganisation = organisationLoadingOutcome.Result;

            #endregion

            #region Load latest submission 

            ReturnViewModel model;

            try
            {
                CustomResult<Return> getLatestSubmissionLoadingOutcome =
                    SubmissionBusinessLogic.GetSubmissionByOrganisationAndYear(foundOrganisation, year);

                if (getLatestSubmissionLoadingOutcome.Failed)
                {
                    return getLatestSubmissionLoadingOutcome.ErrorMessage.ToHttpStatusViewResult();
                }

                model = SubmissionBusinessLogic.ConvertSubmissionReportToReturnViewModel(getLatestSubmissionLoadingOutcome.Result);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"Exception processing the return information for Organisation '{foundOrganisation.OrganisationId}:{foundOrganisation.OrganisationName}'", ex);
                return View("CustomError", new ErrorViewModel(400));
            }

            #endregion

            ViewBag.BasketViewModel = new CompareBasketViewModel {CanAddEmployers = true, CanViewCompare = true};

            return View("EmployerDetails/Report", model);
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
            SearchViewModel searchResultsModel = await ViewingService.SearchAsync(searchParams, orderBy);

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
