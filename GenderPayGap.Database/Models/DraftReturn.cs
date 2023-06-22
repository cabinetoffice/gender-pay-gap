using System;
using System.ComponentModel.DataAnnotations.Schema;
using GenderPayGap.Core;
using Newtonsoft.Json;

namespace GenderPayGap.Database.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DraftReturn
    {

        [JsonProperty]
        public long DraftReturnId { get; set; }

        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public int SnapshotYear { get; set; }


        [JsonProperty]
        public decimal? DiffMeanHourlyPayPercent { get; set; }
        [JsonProperty]
        public decimal? DiffMedianHourlyPercent { get; set; }

        [JsonProperty]
        public decimal? DiffMeanBonusPercent { get; set; }
        [JsonProperty]
        public decimal? DiffMedianBonusPercent { get; set; }
        [JsonProperty]
        public decimal? MaleMedianBonusPayPercent { get; set; }
        [JsonProperty]
        public decimal? FemaleMedianBonusPayPercent { get; set; }

        [JsonProperty]
        public decimal? MaleLowerPayBand { get; set; }
        [JsonProperty]
        public decimal? FemaleLowerPayBand { get; set; }
        [JsonProperty]
        public decimal? MaleMiddlePayBand { get; set; }
        [JsonProperty]
        public decimal? FemaleMiddlePayBand { get; set; }
        [JsonProperty]
        public decimal? MaleUpperPayBand { get; set; }
        [JsonProperty]
        public decimal? FemaleUpperPayBand { get; set; }
        [JsonProperty]
        public decimal? MaleUpperQuartilePayBand { get; set; }
        [JsonProperty]
        public decimal? FemaleUpperQuartilePayBand { get; set; }

        [JsonProperty]
        public string JobTitle { get; set; }
        [JsonProperty]
        public string FirstName { get; set; }
        [JsonProperty]
        public string LastName { get; set; }

        [JsonProperty]
        public OrganisationSizes? OrganisationSize { get; set; }

        [JsonProperty]
        public string CompanyLinkToGPGInfo { get; set; }

        [JsonProperty]
        public bool OptedOutOfReportingPayQuarters { get; set; }
        
        [JsonProperty]
        public DateTime Modified { get; set; }

        public bool IsEmpty()
        {
            return
                DiffMeanHourlyPayPercent == null
                && DiffMedianHourlyPercent == null

                && MaleMedianBonusPayPercent == null
                && FemaleMedianBonusPayPercent == null
                && DiffMeanBonusPercent == null
                && DiffMedianBonusPercent == null

                && MaleLowerPayBand == null
                && FemaleLowerPayBand == null
                && MaleMiddlePayBand == null
                && FemaleMiddlePayBand == null
                && MaleUpperPayBand == null
                && FemaleUpperPayBand == null
                && MaleUpperQuartilePayBand == null
                && FemaleUpperQuartilePayBand == null

                && string.IsNullOrWhiteSpace(FirstName)
                && string.IsNullOrWhiteSpace(LastName)
                && string.IsNullOrWhiteSpace(JobTitle)

                && OrganisationSize == null

                && string.IsNullOrWhiteSpace(CompanyLinkToGPGInfo)
                ;
        }

        public bool IsSameAsSubmittedReturn(Return submittedReturn)
        {
            return
                submittedReturn != null

                && DiffMeanHourlyPayPercent == submittedReturn.DiffMeanHourlyPayPercent
                && DiffMedianHourlyPercent == submittedReturn.DiffMedianHourlyPercent

                && DiffMeanBonusPercent == submittedReturn.DiffMeanBonusPercent
                && DiffMedianBonusPercent == submittedReturn.DiffMedianBonusPercent
                && MaleMedianBonusPayPercent == submittedReturn.MaleMedianBonusPayPercent
                && FemaleMedianBonusPayPercent == submittedReturn.FemaleMedianBonusPayPercent

                && MaleLowerPayBand == submittedReturn.MaleLowerPayBand
                && FemaleLowerPayBand == submittedReturn.FemaleLowerPayBand
                && MaleMiddlePayBand == submittedReturn.MaleMiddlePayBand
                && FemaleMiddlePayBand == submittedReturn.FemaleMiddlePayBand
                && MaleUpperPayBand == submittedReturn.MaleUpperPayBand
                && FemaleUpperPayBand == submittedReturn.FemaleUpperPayBand
                && MaleUpperQuartilePayBand == submittedReturn.MaleUpperQuartilePayBand
                && FemaleUpperQuartilePayBand == submittedReturn.FemaleUpperQuartilePayBand

                && FirstName == submittedReturn.FirstName
                && LastName == submittedReturn.LastName
                && JobTitle == submittedReturn.JobTitle

                && OrganisationSize == submittedReturn.OrganisationSize

                && CompanyLinkToGPGInfo == submittedReturn.CompanyLinkToGPGInfo

                && OptedOutOfReportingPayQuarters == submittedReturn.OptedOutOfReportingPayQuarters;
        }

    }
}
