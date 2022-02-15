using System;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Extensions;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportOverviewViewModel: GovUkViewModel
    {

        public Database.Organisation Organisation { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool DraftReturnExists { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }

        public DateTime SnapshotDate { get; set; }
        
        #region Quarter

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in upper quarter",
            NameWithinSentence = "male employees in upper quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? MaleUpperPayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in upper quarter",
            NameWithinSentence = "female employees in upper quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? FemaleUpperPayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in upper-middle quarter",
            NameWithinSentence = "male employees in upper-middle quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? MaleUpperMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in upper-middle quarter",
            NameWithinSentence = "female employees in upper-middle quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? FemaleUpperMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in lower-middle quarter",
            NameWithinSentence = "male employees in lower-middle quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? MaleLowerMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in lower-middle quarter",
            NameWithinSentence = "female employees in lower-middle quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? FemaleLowerMiddlePayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Male employees in lower quarter",
            NameWithinSentence = "male employees in lower quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? MaleLowerPayBand { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Female employees in lower quarter",
            NameWithinSentence = "female employees in lower quarter")]
        [GovUkValidateDecimalRange(Minimum = 0, Maximum = 100)]
        public decimal? FemaleLowerPayBand { get; set; }

        #endregion

        #region Hourly

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Difference in hourly pay (mean)",
            NameWithinSentence = "difference in hourly pay (mean)")]
        public decimal? DiffMeanHourlyPayPercent { get; set; }
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Difference in hourly pay (median)",
            NameWithinSentence = "difference in hourly pay (median)")]
        public decimal? DiffMedianHourlyPercent { get; set; }

        #endregion

        #region Bonus Percent

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

        #endregion

        #region Bonus Mean/Median

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Difference in bonus pay (mean)",
            NameWithinSentence = "difference in bonus pay (mean)")]
        public decimal? DiffMeanBonusPercent { get; set; }

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Difference in bonus pay (median)",
            NameWithinSentence = "difference in bonus pay (median)")]
        public decimal? DiffMedianBonusPercent { get; set; }

        #endregion

        #region Person responsible

        public string ResponsiblePersonFirstName { get; set; }
        public string ResponsiblePersonLastName { get; set; }
        public string ResponsiblePersonJobTitle { get; set; }

        #endregion

        #region Org size

        public OrganisationSizes? SizeOfOrganisation { get; set; }

        #endregion
        
        #region Link to GPG Info

        public string LinkToOrganisationWebsite { get; set; }

        #endregion
        
        public bool OptedOutOfReportingPayQuarters { get; set; }
        
        public string GetPayQuarterValue(decimal? payQuarterValue)
        {
            return OptedOutOfReportingPayQuarters ? "Not Applicable" : GetPercentageValue(payQuarterValue);
        }

        public string GetMeanOrMedianValue(decimal? meanOrMedianValue)
        {
            return MaleBonusPayPercent == 0 ? "Not Applicable" : GetPercentageValue(meanOrMedianValue);
        }

        public string GetPercentageValue(decimal? value)
        {
            return value == null ? "Not Completed" : $"{value}%";
        }
        public bool PersonResponsibleNotProvided()
        {
            return ResponsiblePersonFirstName == null || ResponsiblePersonLastName == null || ResponsiblePersonJobTitle == null;
        }

        public bool AllRequiredFieldsAreFilled()
        {
            bool optOutOfReporting = ReportingYearsHelper.IsReportingYearWithFurloughScheme(SnapshotDate) && OptedOutOfReportingPayQuarters;

            bool hasPayQuartersData = MaleLowerPayBand.HasValue
                                      && FemaleLowerPayBand.HasValue
                                      && MaleLowerMiddlePayBand.HasValue
                                      && FemaleLowerMiddlePayBand.HasValue
                                      && MaleUpperPayBand.HasValue
                                      && FemaleUpperPayBand.HasValue
                                      && MaleUpperMiddlePayBand.HasValue
                                      && FemaleUpperMiddlePayBand.HasValue;

            bool hasHourlyData = DiffMeanHourlyPayPercent.HasValue
                                 && DiffMedianHourlyPercent.HasValue;

            bool hasBonusData = MaleBonusPayPercent.HasValue
                                && FemaleBonusPayPercent.HasValue
                                && DiffMeanBonusPercent.HasValue
                                && DiffMedianBonusPercent.HasValue;


            bool hasEnterCalculationsData = hasHourlyData && hasBonusData;

            bool hasValidBonusFigures = MaleBonusPayPercent == 0 || DiffMeanBonusPercent.HasValue && DiffMedianBonusPercent.HasValue;
            
            bool hasValidGpgFigures = (optOutOfReporting || hasPayQuartersData) && hasEnterCalculationsData && hasValidBonusFigures;
            
            if (Organisation.SectorType == SectorTypes.Public)
            {
                return hasValidGpgFigures;
            }

            bool hasPersonResponsibleData = !string.IsNullOrWhiteSpace(ResponsiblePersonJobTitle)
                                            && !string.IsNullOrWhiteSpace(ResponsiblePersonFirstName)
                                            && !string.IsNullOrWhiteSpace(ResponsiblePersonLastName);

            return hasValidGpgFigures && hasPersonResponsibleData;
        }
    }
}
