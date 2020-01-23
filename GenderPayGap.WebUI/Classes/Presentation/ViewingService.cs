using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.Search;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Classes.Presentation
{

    public interface IViewingService
    {

        ISearchRepository<EmployerSearchModel> SearchRepository { get; }
        Task<SearchViewModel> SearchAsync(EmployerSearchParameters searchParams);
        Task<List<SearchViewModel.SicSection>> GetAllSicSectionsAsync();
        List<OptionSelect> GetOrgSizeOptions(IEnumerable<int> filterOrgSizes, Dictionary<object, long> facetReults);
        Task<List<OptionSelect>> GetSectorOptionsAsync(IEnumerable<char> filterSicSectionIds, Dictionary<object, long> facetReults);

        PagedResult<EmployerSearchModel> GetPagedResult(IEnumerable<EmployerSearchModel> searchResults,
            long totalRecords,
            int page,
            int pageSize);

        Task<List<SuggestEmployerResult>> SuggestEmployerNameAsync(string search);
        Task<List<SicCodeSearchResult>> GetListOfSicCodeSuggestionsAsync(string search);

    }

    public class ViewingService : IViewingService
    {

        private readonly ICommonBusinessLogic commonLogic;
        private readonly IDataRepository dataRepo;
        private readonly ISearchRepository<SicCodeSearchModel> SicCodeSearchServiceClient;

        public ViewingService(IDataRepository dataRepo, ISearchRepository<EmployerSearchModel> searchRepo, ICommonBusinessLogic commonLogic)
        {
            this.dataRepo = dataRepo;
            SearchRepository = searchRepo;
            this.commonLogic = commonLogic;
        }

        public ViewingService(IDataRepository dataRepo,
            ISearchRepository<EmployerSearchModel> searchRepo,
            ISearchRepository<SicCodeSearchModel> sicCodeSearchServiceClient,
            ICommonBusinessLogic commonLogic) : this(dataRepo, searchRepo, commonLogic)
        {
            SicCodeSearchServiceClient = sicCodeSearchServiceClient;
        }

        public ISearchRepository<EmployerSearchModel> SearchRepository { get; }

        public async Task<SearchViewModel> SearchAsync(EmployerSearchParameters searchParams)
        {
            var searchResults = new PagedResult<EmployerSearchModel>();

            var facets = new Dictionary<string, Dictionary<object, long>>();
            facets.Add("Size", null);
            facets.Add("SicSectionIds", null);
            facets.Add("ReportedYears", null);
            facets.Add("ReportedLateYears", null);
            facets.Add("ReportedExplanationYears", null);

            string searchTermEnteredOnScreen = searchParams.Keywords;

            if (searchParams.SearchType == SearchType.BySectorType)
            {
                IEnumerable<KeyValuePair<string, SicCodeSearchModel>> list =
                    await GetListOfSicCodeSuggestionsFromIndexAsync(searchParams.Keywords);
                searchParams.FilterCodeIds = list.Select(x => int.Parse(x.Value.SicCodeId));

                #region Log the search

                if (!string.IsNullOrEmpty(searchParams.Keywords))
                {
                    string detailedListOfReturnedSearchTerms = string.Join(", ", list.Take(5).Select(x => x.Value.ToLogFriendlyString()));

                    var telemetryProperties = new Dictionary<string, string> {
                        {"TimeStamp", VirtualDateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")},
                        {"QueryTerms", searchParams.Keywords},
                        {"ResultCount", list.Count().ToString()},
                        {"SearchType", searchParams.SearchType.ToString()},
                        {"SampleOfResultsReturned", detailedListOfReturnedSearchTerms}
                    };

                    //Global.AppInsightsClient?.TrackEvent("Gpg_SicCode_Suggest", telemetryProperties);

                    await Global.SearchLog.WriteAsync(telemetryProperties);
                }

                #endregion

                searchParams.SearchFields =
                    $"{nameof(EmployerSearchModel.SicCodeIds)};{nameof(EmployerSearchModel.SicCodeListOfSynonyms)}";
                searchParams.Keywords = "*"; // searchTermModified

                if (list.Any())
                {
                    searchResults = await DoSearchAsync(searchParams, facets);
                }
            }

            if (searchParams.SearchType == SearchType.ByEmployerName)
            {
                searchParams.Keywords = searchParams.Keywords?.Trim();
                searchParams.Keywords = searchParams.RemoveTheMostCommonTermsOnOurDatabaseFromTheKeywords();
                searchResults = await DoSearchAsync(searchParams, facets);
            }

            // build the result view model
            return new SearchViewModel {
                SizeOptions = GetOrgSizeOptions(searchParams.FilterEmployerSizes, facets["Size"]),
                SectorOptions = await GetSectorOptionsAsync(searchParams.FilterSicSectionIds, facets["SicSectionIds"]),
                ReportingYearOptions = GetReportingYearOptions(searchParams.FilterReportedYears),
                ReportingStatusOptions = GetReportingStatusOptions(searchParams.FilterReportingStatus),
                Employers = searchResults,
                search = searchTermEnteredOnScreen,
                p = searchParams.Page,
                s = searchParams.FilterSicSectionIds,
                es = searchParams.FilterEmployerSizes,
                y = searchParams.FilterReportedYears,
                st = searchParams.FilterReportingStatus,
                t = searchParams.SearchType.ToInt32().ToString()
            };
        }

        public async Task<List<SuggestEmployerResult>> SuggestEmployerNameAsync(string searchText)
        {
            IEnumerable<KeyValuePair<string, EmployerSearchModel>> results = await SearchRepository.SuggestAsync(
                searchText,
                $"{nameof(EmployerSearchModel.Name)};{nameof(EmployerSearchModel.PreviousName)};{nameof(EmployerSearchModel.Abbreviations)}");

            var matches = new List<SuggestEmployerResult>();
            foreach (KeyValuePair<string, EmployerSearchModel> result in results)
            {
                //Ensure all names in suggestions are unique
                if (matches.Any(m => m.Text == result.Value.Name))
                {
                    continue;
                }

                matches.Add(
                    new SuggestEmployerResult {
                        Id = result.Value.OrganisationIdEncrypted, Text = result.Value.Name, PreviousName = result.Value.PreviousName
                    });
            }

            return matches;
        }

        public async Task<List<SicCodeSearchResult>> GetListOfSicCodeSuggestionsAsync(string searchText)
        {
            IEnumerable<KeyValuePair<string, SicCodeSearchModel>> listOfSicCodeSuggestionsFromIndex =
                await GetListOfSicCodeSuggestionsFromIndexAsync(searchText);

            return SicCodeSearchResult.ConvertToScreenReadableListOfSuggestions(searchText, listOfSicCodeSuggestionsFromIndex);
        }

        public PagedResult<EmployerSearchModel> GetPagedResult(IEnumerable<EmployerSearchModel> searchResults,
            long totalRecords,
            int page,
            int pageSize)
        {
            var result = new PagedResult<EmployerSearchModel>();

            if (page == 0 || page < 0)
            {
                page = 1;
            }

            result.Results = searchResults.ToList();
            result.ActualRecordTotal = (int) totalRecords;
            result.VirtualRecordTotal = result.Results.Count;
            result.CurrentPage = page;
            result.PageSize = pageSize;

            return result;
        }

        public List<OptionSelect> GetOrgSizeOptions(IEnumerable<int> filterOrgSizes, Dictionary<object, long> facetResults)
        {
            Array allSizes = Enum.GetValues(typeof(OrganisationSizes));

            // setup the filters
            var results = new List<OptionSelect>();
            foreach (OrganisationSizes size in allSizes)
            {
                var id = (int) size;
                string label = size.GetAttribute<DisplayAttribute>().Name;
                bool isChecked = filterOrgSizes != null && filterOrgSizes.Contains(id);
                results.Add(
                    new OptionSelect {
                        Id = $"Size{id}", Label = label, Value = id.ToString(), Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return results;
        }

        public async Task<List<OptionSelect>> GetSectorOptionsAsync(IEnumerable<char> filterSicSectionIds,
            Dictionary<object, long> facetResults)
        {
            // setup the filters
            List<SearchViewModel.SicSection> allSectors = await GetAllSicSectionsAsync();
            var sources = new List<OptionSelect>();
            foreach (SearchViewModel.SicSection sector in allSectors)
            {
                bool isChecked = filterSicSectionIds != null && filterSicSectionIds.Any(x => x == sector.SicSectionCode[0]);
                sources.Add(
                    new OptionSelect {
                        Id = sector.SicSectionCode,
                        Label = sector.Description.TrimEnd('\r', '\n'),
                        Value = sector.SicSectionCode,
                        Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return sources;
        }

        public async Task<List<SearchViewModel.SicSection>> GetAllSicSectionsAsync()
        {
            var results = new List<SearchViewModel.SicSection>();
            List<SicSection> sortedSics = await dataRepo.GetAll<SicSection>().OrderBy(sic => sic.Description).ToListAsync();

            foreach (SicSection sector in sortedSics)
            {
                results.Add(
                    new SearchViewModel.SicSection {
                        SicSectionCode = sector.SicSectionId, Description = sector.Description = sector.Description.BeforeFirst(";")
                    });
            }

            return results;
        }

        private async Task<PagedResult<EmployerSearchModel>> DoSearchAsync(EmployerSearchParameters searchParams,
            Dictionary<string, Dictionary<object, long>> facets)
        {
            return await SearchRepository.SearchAsync(
                searchParams.Keywords, // .ToSearchQuery(),
                searchParams.Page,
                searchParams.SearchType,
                searchParams.PageSize,
                filter: searchParams.ToFilterQuery(),
                facets: facets,
                orderBy: string.IsNullOrWhiteSpace(searchParams.Keywords) ? nameof(EmployerSearchModel.Name) : null,
                searchFields: searchParams.SearchFields,
                searchMode: searchParams.SearchMode);
        }

        private async Task<IEnumerable<KeyValuePair<string, SicCodeSearchModel>>> GetListOfSicCodeSuggestionsFromIndexAsync(
            string searchText)
        {
            IEnumerable<KeyValuePair<string, SicCodeSearchModel>> listOfSicCodeSuggestionsFromIndex =
                await SicCodeSearchServiceClient.SuggestAsync(
                    searchText,
                    $"{nameof(SicCodeSearchModel.SicCodeDescription)},{nameof(SicCodeSearchModel.SicCodeListOfSynonyms)}",
                    null,
                    false,
                    100);
            return listOfSicCodeSuggestionsFromIndex;
        }

        public List<OptionSelect> GetReportingYearOptions(IEnumerable<int> filterSnapshotYears)
        {
            // setup the filters
            int firstYear = Global.FirstReportingYear;
            int currentYear = commonLogic.GetAccountingStartDate(SectorTypes.Public).Year;
            var allYears = new List<int>();
            for (int year = firstYear; year <= currentYear; year++)
            {
                allYears.Add(year);
            }

            var sources = new List<OptionSelect>();
            for (int year = currentYear; year >= firstYear; year--)
            {
                bool isChecked = filterSnapshotYears != null && filterSnapshotYears.Any(x => x == year);
                sources.Add(
                    new OptionSelect {
                        Id = year.ToString(), Label = $"{year} to {year + 1}", Value = year.ToString(), Checked = isChecked
                        // Disabled = facetResults.Count == 0 && !isChecked
                    });
            }

            return sources;
        }

        public List<OptionSelect> GetReportingStatusOptions(IEnumerable<int> filterReportingStatus)
        {
            Array allStatuses = Enum.GetValues(typeof(SearchReportingStatusFilter));

            // setup the filters
            var results = new List<OptionSelect>();
            foreach (SearchReportingStatusFilter enumEntry in allStatuses)
            {
                var id = (int) enumEntry;
                string label = enumEntry.GetAttribute<DisplayAttribute>().Name;
                bool isChecked = filterReportingStatus != null && filterReportingStatus.Contains(id);
                results.Add(new OptionSelect {Id = $"ReportingStatus{id}", Label = label, Value = id.ToString(), Checked = isChecked});
            }

            return results;
        }

    }
}
