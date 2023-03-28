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
using GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Models.Viewing.Download;
using GenderPayGap.WebUI.Search;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Route("viewing")]
    public class ViewingController : Controller
    {
        private readonly AutoCompleteSearchService autoCompleteSearchService;
        private readonly IFileRepository fileRepository;
        private readonly IDataRepository dataRepository;
        private readonly IWebTracker webTracker;

        #region Constructors

        public ViewingController(
            IViewingService viewingService,
            ISearchViewService searchViewService,
            ICompareViewService compareViewService,
            IOrganisationBusinessLogic organisationBusinessLogic,
            IDataRepository dataRepository,
            IWebTracker webTracker,
            AutoCompleteSearchService autoCompleteSearchService,
            IFileRepository fileRepository)
        {
            ViewingService = viewingService;
            SearchViewService = searchViewService;
            CompareViewService = compareViewService;
            OrganisationBusinessLogic = organisationBusinessLogic;
            this.autoCompleteSearchService = autoCompleteSearchService;
            this.fileRepository = fileRepository;
            this.dataRepository = dataRepository;
            this.webTracker = webTracker;
        }

        #endregion

        #region Dependencies

        public IViewingService ViewingService { get; }
        public ISearchViewService SearchViewService { get; }
        public ICompareViewService CompareViewService { get; }
        public IOrganisationBusinessLogic OrganisationBusinessLogic { get; set; }

        #endregion

        [HttpGet("~/")]
        public IActionResult Index()
        {
            return View("Launchpad/Index");
        }

        [HttpGet]
        public IActionResult Redirect()
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
            List<DownloadCsvFile> downloadCsvFiles = GetListOfCsvFilesToDownload();
            
            return View("Download/Download", downloadCsvFiles);
        }

        private List<DownloadCsvFile> GetListOfCsvFilesToDownload()
        {
            List<int> reportingYears = ReportingYearsHelper.GetReportingYears().OrderByDescending(y => y).ToList();

            var csvFiles = new List<DownloadCsvFile>();

            foreach (int year in reportingYears)
            {
                string filePath = UpdatePublicFacingDownloadFilesJob.GetDownloadFileLocationForYear(year);

                bool fileIsAvailable = fileRepository.FileExists(filePath);
                long? fileSize = fileIsAvailable ? fileRepository.GetFileSize(filePath) : null;

                csvFiles.Add(new DownloadCsvFile
                {
                    ReportingYear = year,
                    FileIsAvailable = fileIsAvailable,
                    FileSize = fileSize
                });
            }

            return csvFiles;
        }

        [HttpGet("download-data")]
        [HttpGet("download-data/{year:int=0}")]
        public IActionResult DownloadData(int year = 0)
        {
            if (year == 0)
            {
                year = SectorTypes.Private.GetAccountingStartDate().Year;
            }

            if (!ReportingYearsHelper.GetReportingYears().Contains(year))
            {
                throw new PageNotFoundException();
            }

            string filePath = UpdatePublicFacingDownloadFilesJob.GetDownloadFileLocationForYear(year);

            if (!fileRepository.FileExists(filePath))
            {
                throw new PageNotFoundException();
            }
            
            string fileContents = fileRepository.Read(filePath);

            string userFacingDownloadFileName = $"UK Gender Pay Gap Data - {year} to {year + 1}.csv";

            //Track the download 
            webTracker.TrackPageView(this, userFacingDownloadFileName);

            return DownloadHelper.CreateCsvDownload(fileContents, userFacingDownloadFileName);
        }

        #endregion

        #region Employer details

        [HttpGet("~/Employer/{employerIdentifier}")]
        public IActionResult Employer(string employerIdentifier, int? page = 1)
        {
            if (string.IsNullOrWhiteSpace(employerIdentifier))
            {
                return new HttpBadRequestResult("Missing employer identifier");
            }

            long organisationId = ControllerHelper.DeObfuscateOrganisationIdOrThrow404(employerIdentifier);
            Organisation organisation = ControllerHelper.LoadOrganisationOrThrow404(organisationId, dataRepository);
            ControllerHelper.Throw404IfOrganisationIsNotSearchable(organisation);

            ViewBag.BasketViewModel = new CompareBasketViewModel {CanAddEmployers = true, CanViewCompare = true};
            
            var totalEntries = organisation.GetRecentReports(Global.ShowReportYearCount).Count() + 1; // Years we report for + the year they joined
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
                    Organisation = organisation,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    EntriesPerPage = maxEntriesPerPage
                });
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

            ReturnViewModel model;

            try
            {
                Return foundReturn = foundOrganisation.GetReturn(year);

                if (foundReturn == null)
                {
                    throw new PageNotFoundException();
                }
                
                model = ConvertSubmissionReportToReturnViewModel(foundReturn);
            }
            catch (Exception ex)
            {
                throw new PageNotFoundException();
            }

            #endregion

            ViewBag.BasketViewModel = new CompareBasketViewModel {CanAddEmployers = true, CanViewCompare = true};

            return View("EmployerDetails/Report", model);
        }

        private ReturnViewModel ConvertSubmissionReportToReturnViewModel(Return reportToConvert)
        {
            var model = new ReturnViewModel {
                SectorType = reportToConvert.Organisation.SectorType,
                ReturnId = reportToConvert.ReturnId,
                OrganisationId = reportToConvert.OrganisationId,
                EncryptedOrganisationId = reportToConvert.Organisation.GetEncryptedId(),
                DiffMeanBonusPercent = reportToConvert.DiffMeanBonusPercent,
                DiffMeanHourlyPayPercent = reportToConvert.DiffMeanHourlyPayPercent,
                DiffMedianBonusPercent = reportToConvert.DiffMedianBonusPercent,
                DiffMedianHourlyPercent = reportToConvert.DiffMedianHourlyPercent,
                FemaleLowerPayBand = reportToConvert.FemaleLowerPayBand,
                FemaleMedianBonusPayPercent = reportToConvert.FemaleMedianBonusPayPercent,
                FemaleMiddlePayBand = reportToConvert.FemaleMiddlePayBand,
                FemaleUpperPayBand = reportToConvert.FemaleUpperPayBand,
                FemaleUpperQuartilePayBand = reportToConvert.FemaleUpperQuartilePayBand,
                MaleLowerPayBand = reportToConvert.MaleLowerPayBand,
                MaleMedianBonusPayPercent = reportToConvert.MaleMedianBonusPayPercent,
                MaleMiddlePayBand = reportToConvert.MaleMiddlePayBand,
                MaleUpperPayBand = reportToConvert.MaleUpperPayBand,
                MaleUpperQuartilePayBand = reportToConvert.MaleUpperQuartilePayBand,
                JobTitle = reportToConvert.JobTitle,
                FirstName = reportToConvert.FirstName,
                LastName = reportToConvert.LastName,
                CompanyLinkToGPGInfo = reportToConvert.CompanyLinkToGPGInfo,
                AccountingDate = reportToConvert.AccountingDate,
                Address = reportToConvert.Organisation.GetLatestAddress()?.GetAddressString(),
                LatestAddress = reportToConvert.Organisation.GetLatestAddress()?.GetAddressString(),
                EHRCResponse = reportToConvert.EHRCResponse.ToString(),
                IsVoluntarySubmission = reportToConvert.IsVoluntarySubmission(),
                IsLateSubmission = reportToConvert.IsLateSubmission,
                OptedOutOfReportingPayQuarters = reportToConvert.OptedOutOfReportingPayQuarters
            };

            if (model.Address.EqualsI(model.LatestAddress))
            {
                model.LatestAddress = null;
            }

            model.OrganisationName = reportToConvert.Organisation.GetName(reportToConvert.StatusDate)?.Name
                                     ?? reportToConvert.Organisation.OrganisationName;
            model.LatestOrganisationName = reportToConvert.Organisation.OrganisationName;

            model.Sector = reportToConvert.Organisation.GetSicSectorsString(reportToConvert.StatusDate);
            model.LatestSector = reportToConvert.Organisation.GetSicSectorsString();

            model.OrganisationSize = reportToConvert.OrganisationSize;
            model.Modified = reportToConvert.Modified;

            model.IsInScopeForThisReportYear =
                reportToConvert.Organisation.GetIsInscope(reportToConvert.AccountingDate.Year);

            return model;
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
