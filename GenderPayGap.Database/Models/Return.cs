using System;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Return
    {

        [JsonProperty]
        public long ReturnId { get; set; }
        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public DateTime AccountingDate { get; set; }
        [JsonProperty]
        public decimal DiffMeanHourlyPayPercent { get; set; }
        [JsonProperty]
        public decimal DiffMedianHourlyPercent { get; set; }
        [JsonProperty]
        public decimal? DiffMeanBonusPercent { get; set; }
        [JsonProperty]
        public decimal? DiffMedianBonusPercent { get; set; }
        [JsonProperty]
        public decimal MaleMedianBonusPayPercent { get; set; }
        [JsonProperty]
        public decimal FemaleMedianBonusPayPercent { get; set; }
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
        public string CompanyLinkToGPGInfo { get; set; }
        [JsonProperty]
        public ReturnStatuses Status { get; set; }
        [JsonProperty]
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public string StatusDetails { get; set; }
        [JsonProperty]
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public string JobTitle { get; set; }
        [JsonProperty]
        public string FirstName { get; set; }
        [JsonProperty]
        public string LastName { get; set; }
        [JsonProperty]
        public int MinEmployees { get; set; }
        [JsonProperty]
        public int MaxEmployees { get; set; }
        [JsonProperty]
        public bool IsLateSubmission { get; set; }
        [JsonProperty]
        public string LateReason { get; set; }
        [JsonProperty]
        public string Modifications { get; set; }
        [JsonProperty]
        public bool EHRCResponse { get; set; }
        [JsonProperty]
        public bool OptedOutOfReportingPayQuarters { get; set; }

        public virtual Organisation Organisation { get; set; }
    }
}
