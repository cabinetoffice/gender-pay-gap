using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Extensions;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using GovUkDesignSystem.GovUkDesignSystemComponents;
using GovUkDesignSystem.Helpers;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

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
        public SectorTypes SectorType { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }

        public DateTime SnapshotDate { get; set; }
        
        #region Quarter

        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? MaleUpperPayBand { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? FemaleUpperPayBand { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? MaleUpperMiddlePayBand { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? FemaleUpperMiddlePayBand { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? MaleLowerMiddlePayBand { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? FemaleLowerMiddlePayBand { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? MaleLowerPayBand { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? FemaleLowerPayBand { get; set; }

        #endregion

        #region Hourly

        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? DiffMeanHourlyPayPercent { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? DiffMedianHourlyPercent { get; set; }

        #endregion

        #region Bonus Percent

        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? FemaleBonusPayPercent { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? MaleBonusPayPercent { get; set; }

        #endregion

        #region Bonus Mean/Median

        [DisplayFormat(DataFormatString = "{0:0.#}")]
        public decimal? DiffMeanBonusPercent { get; set; }

        [DisplayFormat(DataFormatString = "{0:0.#}")]
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

        public IHtmlContent GetPayQuarterValue(
            IHtmlHelper<ReportOverviewViewModel> htmlHelper,
            Expression<Func<ReportOverviewViewModel, decimal?>> propertyLambdaExpression)
        {
            IHtmlContent notApplicableContent = htmlHelper.GovUkHtmlText(new HtmlTextViewModel { Text = "Not Applicable" });

            return OptedOutOfReportingPayQuarters
                ? notApplicableContent
                : GetPercentageValue(htmlHelper, propertyLambdaExpression);
        }

        public IHtmlContent GetMeanOrMedianValue(
            IHtmlHelper<ReportOverviewViewModel> htmlHelper,
            Expression<Func<ReportOverviewViewModel, decimal?>> propertyLambdaExpression)
        {
            IHtmlContent notApplicableContent = htmlHelper.GovUkHtmlText(new HtmlTextViewModel { Text = "Not Applicable" });
            
            return MaleBonusPayPercent == 0
                ? notApplicableContent
                : GetPercentageValue(htmlHelper, propertyLambdaExpression);
        }

        public IHtmlContent GetPercentageValue(
            IHtmlHelper<ReportOverviewViewModel> htmlHelper,
            Expression<Func<ReportOverviewViewModel, decimal?>> propertyLambdaExpression)
        {
            IHtmlContent notProvidedContent = htmlHelper.GovUkHtmlText(new HtmlTextViewModel { Text = "Not Provided" });
            IHtmlContent defaultHtmlContent = htmlHelper.GovUkHtmlTextFor(propertyLambdaExpression, "%");
            
            Func<ReportOverviewViewModel, decimal?> compiledExpression = propertyLambdaExpression.Compile();
            decimal? value = compiledExpression(this);

            return value == null
                ? notProvidedContent
                : defaultHtmlContent;
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
