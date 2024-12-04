using System.ComponentModel.DataAnnotations;
using GenderPayGap.Database;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportFiguresViewModel
    {

        public const double MinimumGapPercent = -999999999;
        private const string PositiveOneDecimalPlaceRegex = @"^\d+(\.?\d)?$";
        private const string PositiveAndNegativeOneDecimalPlaceRegex = @"^[-]?\d+(\.?\d)?$";
        private const string OneDecimalPlaceErrorMessage = "Value can't have more than 1 decimal place";

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Organisation Organisation { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }
        public bool IsEditingForTheFirstTime { get; set; }
        
        public DateTime SnapshotDate { get; set; }
        
        public bool OptedOutOfReportingPayQuarters { get; set; }
        public bool PayQuartilesRequired => !OptedOutOfReportingPayQuarters;

        #region Bonus Pay

        [GovUkValidateRequired(ErrorMessageIfMissing = "Females who received bonus pay is required")]
        [GovUkValidateDecimalRange("Females who received bonus pay", 0, 100)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? FemaleBonusPayPercent { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Males who received bonus pay is required")]
        [GovUkValidateDecimalRange("Males who received bonus pay", 0, 100)]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? MaleBonusPayPercent { get; set; }
        
        [GovUkValidateDecimalRange("Difference in bonus pay (mean)", MinimumGapPercent, 100, customErrorMessage: "Enter a percentage lower than or equal to 100")]
        [GovUkValidationRegularExpression(PositiveAndNegativeOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? DiffMeanBonusPercent { get; set; }
        
        [GovUkValidateDecimalRange("Difference in bonus pay (median)",MinimumGapPercent, 100, customErrorMessage: "Enter a percentage lower than or equal to 100")]
        [GovUkValidationRegularExpression(PositiveAndNegativeOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? DiffMedianBonusPercent { get; set; }

        #endregion

        #region Pay Quartile

        [GovUkValidateDecimalRange("Male employees in upper quarter", 0, 100)]
        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(PayQuartilesRequired), ErrorMessageIfMissing = "Male employees in upper quarter is required")]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? MaleUpperPayBand { get; set; }
        
        [GovUkValidateDecimalRange("Female employees in upper quarter", 0, 100)]
        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(PayQuartilesRequired), ErrorMessageIfMissing = "Female employees in upper quarter is required")]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? FemaleUpperPayBand { get; set; }
        
        [GovUkValidateDecimalRange("Male employees in upper-middle quarter", 0, 100)]
        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(PayQuartilesRequired), ErrorMessageIfMissing = "Male employees in upper-middle quarter is required")]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? MaleUpperMiddlePayBand { get; set; }
        
        [GovUkValidateDecimalRange("Female employees in upper-middle quarter", 0, 100)]
        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(PayQuartilesRequired), ErrorMessageIfMissing = "Female employees in upper-middle quarter is required")]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? FemaleUpperMiddlePayBand { get; set; }
        
        [GovUkValidateDecimalRange("Male employees in lower-middle quarter", 0, 100)]
        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(PayQuartilesRequired), ErrorMessageIfMissing = "Male employees in lower-middle quarter is required")]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? MaleLowerMiddlePayBand { get; set; }
        
        [GovUkValidateDecimalRange("Female employees in lower-middle quarter", 0, 100)]
        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(PayQuartilesRequired), ErrorMessageIfMissing = "Female employees in lower-middle quarter is required")]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? FemaleLowerMiddlePayBand { get; set; }
        
        [GovUkValidateDecimalRange("Male employees in lower quarter", 0, 100)]
        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(PayQuartilesRequired), ErrorMessageIfMissing = "Male employees in lower quarter is required")]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? MaleLowerPayBand { get; set; }
        
        [GovUkValidateDecimalRange("Female employees in lower quarter", 0, 100)]
        [GovUkValidateRequiredIf(IsRequiredPropertyName = nameof(PayQuartilesRequired), ErrorMessageIfMissing = "Female employees in lower quarter is required")]
        [GovUkValidationRegularExpression(PositiveOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? FemaleLowerPayBand { get; set; }

        #endregion

        #region Hourly Pay

        [GovUkValidateRequired(ErrorMessageIfMissing = "Difference in hourly pay (mean) is required")]
        [GovUkValidateDecimalRange("Difference in hourly pay (mean)", MinimumGapPercent, 100)]
        [GovUkValidationRegularExpression(PositiveAndNegativeOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? DiffMeanHourlyPayPercent { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Difference in hourly pay (median) is required")]
        [GovUkValidateDecimalRange("Difference in hourly pay (median)", MinimumGapPercent, 100)]
        [GovUkValidationRegularExpression(PositiveAndNegativeOneDecimalPlaceRegex, OneDecimalPlaceErrorMessage)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:0.#}")]
        public decimal? DiffMedianHourlyPercent { get; set; }

        #endregion
        
    }
}
