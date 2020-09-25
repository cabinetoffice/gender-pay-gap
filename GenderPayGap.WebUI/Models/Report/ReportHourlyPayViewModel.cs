using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportHourlyPayViewModel : GovUkViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }


        public ReportPagesAction Action { get; set; }

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Difference in hourly pay (mean)",
            NameWithinSentence = "difference in hourly pay (mean)")]
        public decimal? DiffMeanHourlyPayPercent { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Difference in hourly pay (median)",
            NameWithinSentence = "difference in hourly pay (median)")]
        public decimal? DiffMedianHourlyPercent { get; set; }

    }
}
