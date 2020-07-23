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
        public long? ReturnId { get; set; }
        [JsonProperty]
        public string EncryptedOrganisationId { get; set; }
        [JsonProperty]
        public SectorTypes? SectorType { get; set; }

        [JsonProperty]
        public DateTime? AccountingDate { get; set; }
        [JsonProperty]
        public DateTime Modified { get; set; }

        [JsonProperty]
        public string JobTitle { get; set; }
        [JsonProperty]
        public string FirstName { get; set; }
        [JsonProperty]
        public string LastName { get; set; }
        [JsonProperty]
        public string CompanyLinkToGPGInfo { get; set; }

        [JsonProperty]
        public string ReturnUrl { get; set; }
        [JsonProperty]
        public string OriginatingAction { get; set; }

        [JsonProperty]
        public string Address { get; set; }
        [JsonProperty]
        public string LatestAddress { get; set; }
        [JsonProperty]
        public string OrganisationName { get; set; }
        [JsonProperty]
        public string LatestOrganisationName { get; set; }
        [JsonProperty]
        public OrganisationSizes? OrganisationSize { get; set; }
        [JsonProperty]
        public string Sector { get; set; }
        [JsonProperty]
        public string LatestSector { get; set; }
        [JsonProperty]
        public bool? IsDifferentFromDatabase { get; set; }

        [JsonProperty]
        public bool? IsVoluntarySubmission { get; set; }
        [JsonProperty]
        public bool? IsLateSubmission { get; set; }
        [JsonProperty]
        public bool? ShouldProvideLateReason { get; set; }
        [JsonProperty]
        public bool? IsInScopeForThisReportYear { get; set; }
        [JsonProperty]
        public string LateReason { get; set; } = "";
        [JsonProperty]
        public string EHRCResponse { get; set; } = "";

        [JsonProperty]
        public DateTime? LastWrittenDateTime { get; set; }
        [JsonProperty]
        public long? LastWrittenByUserId { get; set; }
        [JsonProperty]
        public bool? HasDraftBeenModifiedDuringThisSession { get; set; }

        [JsonProperty]
        public DateTime? ReportingStartDate { get; set; }
        [JsonProperty]
        public DateTime? ReportModifiedDate { get; set; }
        [JsonProperty]
        public ScopeStatuses? ReportingRequirement { get; set; }

        [NotMapped]
        public bool NotRequiredToReport =>
            ReportingRequirement == ScopeStatuses.OutOfScope || ReportingRequirement == ScopeStatuses.PresumedOutOfScope;

    }
}
