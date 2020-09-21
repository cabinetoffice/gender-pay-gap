using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportEmployeesByPayQuartileViewModel : GovUkViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }

        public ReportPagesAction Action { get; set; }

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in upper quartile",
            NameWithinSentence = "male employees in upper quartile")]
        public decimal? MaleUpperPayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in upper quartile",
            NameWithinSentence = "female employees in upper quartile")]
        public decimal? FemaleUpperPayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in upper-middle quartile",
            NameWithinSentence = "male employees in upper-middle quartile")]
        public decimal? MaleUpperMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in upper-middle quartile",
            NameWithinSentence = "female employees in upper-middle quartile")]
        public decimal? FemaleUpperMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in lower-middle quartile",
            NameWithinSentence = "male employees in lower-middle quartile")]
        public decimal? MaleLowerMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in lower-middle quartile",
            NameWithinSentence = "female employees in lower-middle quartile")]
        public decimal? FemaleLowerMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in lower quartile",
            NameWithinSentence = "male employees in lower quartile")]
        public decimal? MaleLowerPayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in lower quartile",
            NameWithinSentence = "female employees in lower quartile")]
        public decimal? FemaleLowerPayBand { get; set; }

    }
}
