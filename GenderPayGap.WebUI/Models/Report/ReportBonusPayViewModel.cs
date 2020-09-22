using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportBonusPayViewModel : GovUkViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }

        public ReportPagesAction Action { get; set; }


        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Females who received bonus pay",
            NameWithinSentence = "females who received bonus pay")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? FemaleBonusPayPercent { get; set; }

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Males who received bonus pay",
            NameWithinSentence = "males who received bonus pay")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? MaleBonusPayPercent { get; set; }

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Different in bonus pay (mean)",
            NameWithinSentence = "difference in bonus pay (mean)")]
        public decimal? DiffMeanBonusPercent { get; set; }

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Different in bonus pay (median)",
            NameWithinSentence = "difference in bonus pay (median)")]
        public decimal? DiffMedianBonusPercent { get; set; }

    }
}
