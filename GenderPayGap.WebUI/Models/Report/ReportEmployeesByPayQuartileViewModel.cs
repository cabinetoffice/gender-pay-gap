using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
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
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? MaleUpperPayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in upper quartile",
            NameWithinSentence = "female employees in upper quartile")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? FemaleUpperPayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in upper-middle quartile",
            NameWithinSentence = "male employees in upper-middle quartile")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? MaleUpperMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in upper-middle quartile",
            NameWithinSentence = "female employees in upper-middle quartile")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? FemaleUpperMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in lower-middle quartile",
            NameWithinSentence = "male employees in lower-middle quartile")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? MaleLowerMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in lower-middle quartile",
            NameWithinSentence = "female employees in lower-middle quartile")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? FemaleLowerMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in lower quartile",
            NameWithinSentence = "male employees in lower quartile")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? MaleLowerPayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in lower quartile",
            NameWithinSentence = "female employees in lower quartile")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? FemaleLowerPayBand { get; set; }

    }
}
