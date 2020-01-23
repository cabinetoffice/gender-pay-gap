using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;

namespace GenderPayGap.WebUI.Models.Search
{
    [Serializable]
    public class SearchViewModel
    {

        private List<string> _reportingStatusFilterInfo;

        private List<string> _reportingYearFilterInfo;

        private List<string> _sectorFilterInfo;

        private List<string> _sizeFilterInfo;

        public SearchViewModel()
        {
            _sectorFilterInfo = new List<string>();
            _sizeFilterInfo = new List<string>();
            _reportingYearFilterInfo = new List<string>();
            _reportingStatusFilterInfo = new List<string>();
        }
        
        public List<OptionSelect> SectorOptions { get; set; }
        public List<OptionSelect> ReportingYearOptions { get; set; }
        public List<OptionSelect> ReportingStatusOptions { get; internal set; }
        public List<OptionSelect> SizeOptions { get; internal set; }

        public string search { get; set; }

        public IEnumerable<char> s { get; set; }
        public IEnumerable<int> es { get; set; }
        public IEnumerable<int> y { get; set; }
        public IEnumerable<int> st { get; set; }
        public int p { get; set; }
        public string t { get; set; }

        public PagedResult<EmployerSearchModel> Employers { get; set; }

        public int EmployerStartIndex
        {
            get
            {
                if (Employers == null || Employers.Results == null || Employers.Results.Count < 1)
                {
                    return 1;
                }

                return ((Employers.CurrentPage * Employers.PageSize) - Employers.PageSize) + 1;
            }
        }

        public int EmployerEndIndex
        {
            get
            {
                if (Employers == null || Employers.Results == null || Employers.Results.Count < 1)
                {
                    return 1;
                }

                return (EmployerStartIndex + Employers.Results.Count) - 1;
            }
        }

        public int PagerStartIndex
        {
            get
            {
                if (Employers == null || Employers.PageCount <= 5)
                {
                    return 1;
                }

                if (Employers.CurrentPage < 4)
                {
                    return 1;
                }

                if (Employers.CurrentPage + 2 > Employers.PageCount)
                {
                    return Employers.PageCount - 4;
                }

                return Employers.CurrentPage - 2;
            }
        }
        
        public List<string> SectorFilterInfo
        {
            get
            {
                if (_sectorFilterInfo == null)
                {
                    _sectorFilterInfo = OptionSelect.GetCheckedString(SectorOptions);
                }

                return _sectorFilterInfo;
            }
        }

        public List<string> SizeFilterInfo
        {
            get
            {
                if (_sizeFilterInfo == null)
                {
                    _sizeFilterInfo = OptionSelect.GetCheckedString(SizeOptions);
                }

                return _sizeFilterInfo;
            }
        }

        public List<string> ReportingYearFilterInfo
        {
            get
            {
                if (_reportingYearFilterInfo == null)
                {
                    _reportingYearFilterInfo = OptionSelect.GetCheckedString(ReportingYearOptions);
                }

                return _reportingYearFilterInfo;
            }
        }

        public List<string> ReportingStatusFilterInfo
        {
            get
            {
                if (_reportingStatusFilterInfo == null)
                {
                    _reportingStatusFilterInfo = OptionSelect.GetCheckedString(ReportingStatusOptions);
                }

                return _reportingStatusFilterInfo;
            }
        }

        public EmployerSearchModel GetEmployer(string employerIdentifier)
        {
            //Get the employer from the last search results
            return Employers?.Results?.FirstOrDefault(e => e.OrganisationIdEncrypted == employerIdentifier);
        }
        
        [Serializable]
        public class SicSection
        {

            public string SicSectionCode { get; set; }

            public string Description { get; set; }

        }

    }
}
