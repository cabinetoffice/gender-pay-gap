using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Models.Compare;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GenderPayGap.WebUI.Models
{
    public class CompareViewModel
    {

        public int Year { get; set; }

        public string YearFormatted => ReportingYearsHelper.FormatYearAsReportingPeriod(Year);

        public IEnumerable<CompareReportModel> CompareReports { get; set; }

        public IEnumerable<SelectListItem> GetSelectList
        {
            get
            {
                int endYear = SectorTypes.Private.GetAccountingStartDate().Year;
                int startYear = endYear - (Global.ShowReportYearCount - 1);
                if (startYear < Global.FirstReportingYear)
                {
                    startYear = Global.FirstReportingYear;
                }

                var list = new List<SelectListItem>();
                for (int year = startYear; year <=endYear; year++)
                {
                    SelectListItem item = new SelectListItem(
                        (ReportingYearsHelper.FormatYearAsReportingPeriod(year, "/")),
                        year.ToString(),
                        Year == year
                        );
                    list.Add(item);
                }

                return list;
            }
        }

        public string LastSearchUrl { get; set; }

        public int CompareBasketCount { get; set; }

        public string ShareEmailUrl { get; set; }

        public string ShareEmailSubject => "Comparing employer%27s gender pay gaps";

        public string ShareEmailBody =>
            $"Hi there,%0A%0AI compared these employers on GOV.UK. Thought you'd like to see the results...%0A%0A{ShareEmailUrl}%0A%0A";

        public bool SortAscending { get; set; }

        public string SortColumn { get; set; }

        public string FormatValue(decimal? value)
        {
            return (value ?? default(long)).ToString("0.0") + "%";
        }

        public string GetColumnSortCssClass(string columnName)
        {
            if (SortColumn == columnName)
            {
                return SortAscending ? "ascending" : "descending";
            }

            return "";
        }

        public string GetColumnSortIcon(string columnName)
        {
            if (SortColumn == columnName)
            {
                return SortAscending ? "/img/sort-glyph-up-white.png" : "/img/sort-glyph-down-white.png";
            }

            return "/img/sort-glyph-noSort.png";
        }

    }

}
