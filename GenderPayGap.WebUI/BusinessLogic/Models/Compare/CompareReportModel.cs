using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.BusinessLogic.Models.Compare
{

    [Serializable]
    public sealed class CompareReportModel
    {

        public bool HasReported { get; set; }

        public string EncOrganisationId { get; set; }

        public string OrganisationName { get; set; }
        
        public string OrganisationSicCodes { get; set; }

        public ScopeStatuses ScopeStatus { get; set; }

        public decimal? DiffMeanHourlyPayPercent { get; set; }

        public decimal? DiffMedianHourlyPercent { get; set; }

        public decimal? DiffMeanBonusPercent { get; set; }

        public decimal? DiffMedianBonusPercent { get; set; }

        public decimal? MaleMedianBonusPayPercent { get; set; }

        public decimal? FemaleMedianBonusPayPercent { get; set; }

        public decimal? FemaleLowerPayBand { get; set; }

        public decimal? FemaleMiddlePayBand { get; set; }

        public decimal? FemaleUpperPayBand { get; set; }

        public decimal? FemaleUpperQuartilePayBand { get; set; }

        public OrganisationSizes? OrganisationSize { get; set; }

        public bool? HasBonusesPaid { get; set; }

        public bool RequiredToReport => ScopeStatus == ScopeStatuses.InScope || ScopeStatus == ScopeStatuses.PresumedInScope;

        public string OrganisationSizeName =>
            HasReported
                ? OrganisationSize.GetAttribute<DisplayAttribute>().Name
                : OrganisationSizes.NotProvided.GetAttribute<DisplayAttribute>().Name;

    }

}
