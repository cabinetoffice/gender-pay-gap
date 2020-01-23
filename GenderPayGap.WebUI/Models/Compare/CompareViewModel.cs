using System.Collections.Generic;
using GenderPayGap.BusinessLogic.Models.Compare;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.Models
{
    public class CompareViewModel
    {

        public int Year { get; set; }

        public string YearFormatted => $"{Year}-{(Year + 1).ToTwoDigitYear()}";

        public IEnumerable<CompareReportModel> CompareReports { get; set; }

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
