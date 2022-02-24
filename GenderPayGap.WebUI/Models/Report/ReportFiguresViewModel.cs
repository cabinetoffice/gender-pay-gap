using System;
using System.ComponentModel.DataAnnotations;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportFiguresViewModel : GovUkViewModel
    {

        private const string PositiveOneDecimalPlaceRegex = @"^\d+(\.?\d)?$";
        private const string PositiveAndNegativeOneDecimalPlaceRegex = @"^[-]?\d+(\.?\d)?$";
        private const string OneDecimalPlaceErrorMessage = "Value can't have more than 1 decimal place";

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }
        
        public DateTime SnapshotDate { get; set; }
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Employer opted out of reporting pay quarter",
            NameWithinSentence = "employer opted out of reporting pay quarter")]
        public bool OptedOutOfReportingPayQuarters { get; set; }

        #region Bonus Pay

        [GovUkValidateRequired]
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Females who received bonus pay",
            NameWithinSentence = "females who received bonus pay")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? FemaleBonusPayPercent { get; set; }

        [GovUkValidateRequired]
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Males who received bonus pay",
            NameWithinSentence = "males who received bonus pay")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? MaleBonusPayPercent { get; set; }
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Difference in bonus pay (mean)",
            NameWithinSentence = "difference in bonus pay (mean)")]
        [GovUkValidateDecimalRange(Minimum = (double) decimal.MinValue, Maximum = 100)]
        [GovUkValidationRegularExpression(PositiveAndNegativeOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? DiffMeanBonusPercent { get; set; }
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Difference in bonus pay (median)",
            NameWithinSentence = "difference in bonus pay (median)")]
        [GovUkValidateDecimalRange(Minimum = (double) decimal.MinValue, Maximum = 100)]
        [GovUkValidationRegularExpression(PositiveAndNegativeOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? DiffMedianBonusPercent { get; set; }

        #endregion

        #region Pay Quartile

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in upper quarter",
            NameWithinSentence = "male employees in upper quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 200.9)]
        [GovUkValidationRequiredIf("OptedOutOfReportingPayQuarters", false)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? MaleUpperPayBand { get; set; }
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in upper quarter",
            NameWithinSentence = "female employees in upper quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 200.9)]
        [GovUkValidationRequiredIf("OptedOutOfReportingPayQuarters", false)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? FemaleUpperPayBand { get; set; }
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in upper-middle quarter",
            NameWithinSentence = "male employees in upper-middle quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 200.9)]
        [GovUkValidationRequiredIf("OptedOutOfReportingPayQuarters", false)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? MaleUpperMiddlePayBand { get; set; }
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in upper-middle quarter",
            NameWithinSentence = "female employees in upper-middle quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 200.9)]
        [GovUkValidationRequiredIf("OptedOutOfReportingPayQuarters", false)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? FemaleUpperMiddlePayBand { get; set; }
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in lower-middle quarter",
            NameWithinSentence = "male employees in lower-middle quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 200.9)]
        [GovUkValidationRequiredIf("OptedOutOfReportingPayQuarters", false)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? MaleLowerMiddlePayBand { get; set; }
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in lower-middle quarter",
            NameWithinSentence = "female employees in lower-middle quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 200.9)]
        [GovUkValidationRequiredIf("OptedOutOfReportingPayQuarters", false)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? FemaleLowerMiddlePayBand { get; set; }
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in lower quarter",
            NameWithinSentence = "male employees in lower quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 200.9)]
        [GovUkValidationRequiredIf("OptedOutOfReportingPayQuarters", false)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? MaleLowerPayBand { get; set; }
        
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in lower quarter",
            NameWithinSentence = "female employees in lower quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 200.9)]
        [GovUkValidationRequiredIf("OptedOutOfReportingPayQuarters", false)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? FemaleLowerPayBand { get; set; }

        #endregion

        #region Hourly Pay

        [GovUkValidateRequired]
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Difference in hourly pay (mean)",
            NameWithinSentence = "difference in hourly pay (mean)")]
        [GovUkValidateDecimalRange(Minimum = -499.99, Maximum = 100)]
        [GovUkValidationRegularExpression(PositiveAndNegativeOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? DiffMeanHourlyPayPercent { get; set; }
        
        [GovUkValidateRequired]
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Difference in hourly pay (median)",
            NameWithinSentence = "difference in hourly pay (median)")]
        [GovUkValidateDecimalRange(Minimum = -499.99, Maximum = 100)]
        [GovUkValidationRegularExpression(PositiveAndNegativeOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? DiffMedianHourlyPercent { get; set; }

        #endregion
        
    }
}
