using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Search;

namespace GenderPayGap.WebUI.Classes.Presentation
{

    public interface IViewingService
    {
        
        SearchViewModel Search(EmployerSearchParameters searchParams, string orderBy);
        
    }

    public class ViewingService : IViewingService
    {

        private readonly IDataRepository dataRepo;
        private readonly ViewingSearchService viewingSearchService;

        public ViewingService(IDataRepository dataRepo, ViewingSearchService viewingSearchService)
        {
            this.dataRepo = dataRepo;
            this.viewingSearchService = viewingSearchService;
        }
        

        public SearchViewModel Search(EmployerSearchParameters searchParams, string orderBy)
        {
            bool orderByRelevance = orderBy == "relevance";
            
            // build the result view model
            return new SearchViewModel {
                SizeOptions = GetOrgSizeOptions(searchParams.FilterEmployerSizes),
                SectorOptions = GetSectorOptionsAsync(searchParams.FilterSicSectionIds),
                ReportingYearOptions = GetReportingYearOptions(searchParams.FilterReportedYears),
                ReportingStatusOptions = GetReportingStatusOptions(searchParams.FilterReportingStatus),
                Employers = viewingSearchService.Search(searchParams, orderByRelevance),
                search = searchParams.Keywords,
                p = searchParams.Page,
                s = searchParams.FilterSicSectionIds,
                es = searchParams.FilterEmployerSizes,
                y = searchParams.FilterReportedYears,
                st = searchParams.FilterReportingStatus,
                t = searchParams.SearchType.ToInt32().ToString(),
                OrderBy = orderBy
            };
        }

        private List<OptionSelect> GetOrgSizeOptions(IEnumerable<int> filterOrgSizes)
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

        private List<OptionSelect> GetSectorOptionsAsync(IEnumerable<char> filterSicSectionIds)
        {
            // setup the filters
            List<SearchViewModel.SicSection> allSectors = GetAllSicSectionsAsync();
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

        private List<SearchViewModel.SicSection> GetAllSicSectionsAsync()
        {
            var results = new List<SearchViewModel.SicSection>();
            List<SicSection> sortedSics = dataRepo.GetAll<SicSection>().OrderBy(sic => sic.Description).ToList();

            foreach (SicSection sector in sortedSics)
            {
                results.Add(
                    new SearchViewModel.SicSection {
                        SicSectionCode = sector.SicSectionId, Description = sector.Description = sector.Description.BeforeFirst(";")
                    });
            }

            return results;
        }

        private List<OptionSelect> GetReportingYearOptions(IEnumerable<int> filterSnapshotYears)
        {
            // setup the filters
            int firstYear = Global.FirstReportingYear;
            int currentYear = SectorTypes.Public.GetAccountingStartDate().Year;
            
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

        private List<OptionSelect> GetReportingStatusOptions(IEnumerable<int> filterReportingStatus)
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
