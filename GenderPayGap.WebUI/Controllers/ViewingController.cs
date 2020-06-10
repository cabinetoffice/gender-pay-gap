using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using AutoMapper;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("viewing")]
    public class ViewingController : BaseController
    {
        private AutoCompleteSearchService autoCompleteSearchService;

        #region Constructors

        public ViewingController(
            IHttpCache cache,
            IHttpSession session,
            IViewingService viewingService,
            ISearchViewService searchViewService,
            ICompareViewService compareViewService,
            IOrganisationBusinessLogic organisationBusinessLogic,
            ISubmissionBusinessLogic submissionBusinessLogic,
            IObfuscator obfuscator,
            IDataRepository dataRepository,
            IWebTracker webTracker,
            AutoCompleteSearchService autoCompleteSearchService) : base(cache, session, dataRepository, webTracker)
        {
            ViewingService = viewingService;
            SearchViewService = searchViewService;
            CompareViewService = compareViewService;
            OrganisationBusinessLogic = organisationBusinessLogic;
            Obfuscator = obfuscator;
            SubmissionBusinessLogic = submissionBusinessLogic;
            this.autoCompleteSearchService = autoCompleteSearchService;
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

        #region Initialisation

        /// <summary>
        ///     This action is only used to warm up this controller on initialisation
        /// </summary>
        [HttpGet("Init")]
        public IActionResult Init()
        {
            return new EmptyResult();
        }

        [HttpGet("~/")]
        public IActionResult Index()
        {
            //Clear the default back url of the employer hub pages
            EmployerBackUrl = null;
            ReportBackUrl = null;
            
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
        public async Task<IActionResult> Redirect()
        {
            await WebTracker.TrackPageViewAsync(this);

            return RedirectToActionPermanent("Index");
        }

        [HttpGet("azure-uptime-pinger")]
        public async Task<IActionResult> AzureUptimePingerEndpoint()
        {
            return RedirectToActionPermanent("Index");
        }

        #endregion

        #region Search

        /// <summary>
        /// </summary>
        /// <param name="searchQuery"></param>
        [NoCache]
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

            //Clear the default back url of the employer hub pages
            EmployerBackUrl = null;
            ReportBackUrl = null;

            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out HttpStatusViewResult result))
            {
                return result;
            }

            // generate result view model
            var searchParams = Mapper.Map<EmployerSearchParameters>(searchQuery);
            SearchViewModel model = await ViewingService.SearchAsync(searchParams, orderBy);
            ViewBag.ReturnUrl = SearchViewService.GetLastSearchUrl();

            ViewBag.BasketViewModel = new CompareBasketViewModel {
                CanAddEmployers = false, CanViewCompare = CompareViewService.BasketItemCount > 1, CanClearCompare = true
            };

            return View("Finder/SearchResults", model);
        }

        [NoCache]
        [HttpGet("~/search-results-js")]
        [HttpGet("search-results-js")]
        // used to generate suggestions for the search on the landing page 
        public async Task<IActionResult> SearchResultsJs([FromQuery] SearchResultsQuery searchQuery)
        {
            //Clear the default back url of the employer hub pages
            EmployerBackUrl = null;
            ReportBackUrl = null;


            // ensure parameters are valid
            if (!searchQuery.TryValidateSearchParams(out HttpStatusViewResult result))
            {
                return result;
            }

            // generate result view model
            var searchParams = Mapper.Map<EmployerSearchParameters>(searchQuery);
            SearchViewModel model = await ViewingService.SearchAsync(searchParams, "relevance");

            ViewBag.ReturnUrl = SearchViewService.GetLastSearchUrl();

            return PartialView("Finder/Parts/MainContent", model);
        }

        [ResponseCache(CacheProfileName = "SuggestEmployerNameJs")]
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
        public async Task<IActionResult> Download()
        {
            var model = new DownloadViewModel {Downloads = new List<DownloadViewModel.Download>()};

            const string filePattern = "GPGData_????-????.csv";
            foreach (string file in await Global.FileRepository.GetFilesAsync(Global.DownloadsLocation, filePattern))
            {
                var download = new DownloadViewModel.Download {
                    Title = Path.GetFileNameWithoutExtension(file).AfterFirst("GPGData_"),
                    Count = await Global.FileRepository.GetMetaDataAsync(file, "RecordCount"),
                    Extension = Path.GetExtension(file).TrimI("."),
                    Size = Numeric.FormatFileSize(await Global.FileRepository.GetFileSizeAsync(file))
                };

                download.Url = Url.Action("DownloadData", new {year = download.Title.BeforeFirst("-")});
                model.Downloads.Add(download);
            }

            //Sort downloads by descending year
            model.Downloads = model.Downloads.OrderByDescending(d => d.Title).ToList();

            //Return the view with the model
            return View("Download", model);
        }

        [HttpGet("download-data")]
        [HttpGet("download-data/{year:int=0}")]
        public async Task<IActionResult> DownloadData(int year = 0)
        {
            if (year == 0)
            {
                year = SectorTypes.Private.GetAccountingStartDate().Year;
            }

            //Ensure we have a directory
            if (!await Global.FileRepository.GetDirectoryExistsAsync(Global.DownloadsLocation))
            {
                return new HttpNotFoundResult($"Directory '{Global.DownloadsLocation}' does not exist");
            }

            //Ensure we have a file
            string filePattern = $"GPGData_{year}-{year + 1}.csv";
            IEnumerable<string> files = await Global.FileRepository.GetFilesAsync(Global.DownloadsLocation, filePattern);
            string file = files.FirstOrDefault();
            if (file == null || !await Global.FileRepository.GetFileExistsAsync(file))
            {
                return new HttpNotFoundResult("Cannot find GPG data file for year: " + year);
            }
            //Get the public and private accounting dates for the specified year

            //TODO log download

            //Setup the HTTP response
            var contentDisposition = new ContentDisposition {
                FileName = $"UK Gender Pay Gap Data - {year} to {year + 1}.csv", Inline = false
            };
            HttpContext.SetResponseHeader("Content-Disposition", contentDisposition.ToString());

            //cache old files for 1 day
            DateTime lastWriteTime = await Global.FileRepository.GetLastWriteTimeAsync(file);
            if (lastWriteTime.AddMonths(12) < VirtualDateTime.Now)
            {
                Response.GetTypedHeaders().CacheControl = new CacheControlHeaderValue {MaxAge = TimeSpan.FromDays(1), Public = true};
            }

            /* No Longer required as AspNetCore has response buffering on by default
            Response.BufferOutput = true;
            */
            //Track the download 
            await WebTracker.TrackPageViewAsync(this, contentDisposition.FileName);

            //Return the data
            return Content(await Global.FileRepository.ReadAsync(file), "text/csv");
        }

        #endregion

        #region Employer details

        [HttpGet("employer-details")]
        [Obsolete("Please use method 'Employer' instead.")] //, true)]
        public async Task<IActionResult> EmployerDetails(string e = null, int y = 0, string id = null)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                Organisation organisation;

                try
                {
                    CustomResult<Organisation> organisationLoadingOutcome =
                        await OrganisationBusinessLogic.GetOrganisationByEncryptedReturnIdAsync(id);

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

        [NoCache]
        [HttpGet("~/Employer/{employerIdentifier}")]
        public IActionResult Employer(string employerIdentifier)
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

            //Clear the default back url of the report page
            ReportBackUrl = null;

            ViewBag.BasketViewModel = new CompareBasketViewModel {CanAddEmployers = true, CanViewCompare = true};

            return View(
                "EmployerDetails/Employer",
                new EmployerDetailsViewModel {
                    Organisation = organisationLoadingOutcome.Result,
                    LastSearchUrl = SearchViewService.GetLastSearchUrl(),
                    EmployerBackUrl = EmployerBackUrl,
                    ComparedEmployers = CompareViewService.ComparedEmployers.Value
                });
        }

        #endregion

        #region Reports

        [HttpGet("employer-{employerIdentifier}/report-{year}")]
        [NoCache]
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


        [HttpGet("~/Employer/{employerIdentifier}/{year}")]
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
            var searchParams = Mapper.Map<EmployerSearchParameters>(searchQuery);

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

    }
}
