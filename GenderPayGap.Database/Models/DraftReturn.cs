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
                
                && OptedOutOfReportingPayQuarters == submittedReturn.OptedOutOfReportingPayQuarters
                ;
        }


        #region Obsolete properties - to be deleted once we're happy they're no longer used
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public long? ReturnId { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string EncryptedOrganisationId { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public SectorTypes? SectorType { get; set; }

        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public DateTime? AccountingDate { get; set; }

        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string ReturnUrl { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string OriginatingAction { get; set; }

        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string Address { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string LatestAddress { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string OrganisationName { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string LatestOrganisationName { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string Sector { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string LatestSector { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public bool? IsDifferentFromDatabase { get; set; }

        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public bool? IsVoluntarySubmission { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public bool? IsLateSubmission { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public bool? ShouldProvideLateReason { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public bool? IsInScopeForThisReportYear { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string LateReason { get; set; } = "";
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public string EHRCResponse { get; set; } = "";

        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public DateTime? LastWrittenDateTime { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public long? LastWrittenByUserId { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public bool? HasDraftBeenModifiedDuringThisSession { get; set; }

        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public DateTime? ReportingStartDate { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public DateTime? ReportModifiedDate { get; set; }
        [JsonProperty]
        [Obsolete("Only used by old Submit journey")]
        public ScopeStatuses? ReportingRequirement { get; set; }

        [NotMapped]
        [Obsolete("Only used by old Submit journey")]
        public bool NotRequiredToReport =>
            ReportingRequirement == ScopeStatuses.OutOfScope || ReportingRequirement == ScopeStatuses.PresumedOutOfScope;
        #endregion

    }
}
