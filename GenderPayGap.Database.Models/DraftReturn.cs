using System;
using System.ComponentModel.DataAnnotations.Schema;
using GenderPayGap.Core;

namespace GenderPayGap.Database.Models
{
    public class DraftReturn
    {

        public DraftReturn() { }

        public DraftReturn(DraftReturn clone)
        {
            DraftReturnStatus = clone.DraftReturnStatus;
            OrganisationId = clone.OrganisationId;
            SnapshotYear = clone.SnapshotYear;
            DiffMeanHourlyPayPercent = clone.DiffMeanHourlyPayPercent;
            DiffMedianHourlyPercent = clone.DiffMedianHourlyPercent;
            DiffMeanBonusPercent = clone.DiffMeanBonusPercent;
            DiffMedianBonusPercent = clone.DiffMedianBonusPercent;
            MaleMedianBonusPayPercent = clone.MaleMedianBonusPayPercent;
            FemaleMedianBonusPayPercent = clone.FemaleMedianBonusPayPercent;
            MaleLowerPayBand = clone.MaleLowerPayBand;
            FemaleLowerPayBand = clone.FemaleLowerPayBand;
            MaleMiddlePayBand = clone.MaleMiddlePayBand;
            FemaleMiddlePayBand = clone.FemaleMiddlePayBand;
            MaleUpperPayBand = clone.MaleUpperPayBand;
            FemaleUpperPayBand = clone.FemaleUpperPayBand;
            MaleUpperQuartilePayBand = clone.MaleUpperQuartilePayBand;
            FemaleUpperQuartilePayBand = clone.FemaleUpperQuartilePayBand;
            ReturnId = clone.ReturnId;
            EncryptedOrganisationId = clone.EncryptedOrganisationId;
            SectorType = clone.SectorType;
            AccountingDate = clone.AccountingDate;
            Modified = clone.Modified;
            JobTitle = clone.JobTitle;
            FirstName = clone.FirstName;
            LastName = clone.LastName;
            CompanyLinkToGPGInfo = clone.CompanyLinkToGPGInfo;
            ReturnUrl = clone.ReturnUrl;
            OriginatingAction = clone.OriginatingAction;
            Address = clone.Address;
            LatestAddress = clone.LatestAddress;
            OrganisationName = clone.OrganisationName;
            LatestOrganisationName = clone.LatestOrganisationName;
            OrganisationSize = clone.OrganisationSize;
            Sector = clone.Sector;
            LatestSector = clone.LatestSector;
            IsDifferentFromDatabase = clone.IsDifferentFromDatabase;
            IsVoluntarySubmission = clone.IsVoluntarySubmission;
            IsLateSubmission = clone.IsLateSubmission;
            ShouldProvideLateReason = clone.ShouldProvideLateReason;
            IsInScopeForThisReportYear = clone.IsInScopeForThisReportYear;
            LateReason = clone.LateReason;
            EHRCResponse = clone.EHRCResponse;
            LastWrittenDateTime = clone.LastWrittenDateTime;
            LastWrittenByUserId = clone.LastWrittenByUserId;
            HasDraftBeenModifiedDuringThisSession = clone.HasDraftBeenModifiedDuringThisSession;
            ReportingStartDate = clone.ReportingStartDate;
            ReportModifiedDate = clone.ReportModifiedDate;
            ReportingRequirement = clone.ReportingRequirement;
        }

        public long DraftReturnId { get; set; }

        public DraftReturnStatus DraftReturnStatus { get; set; }
        public long OrganisationId { get; set; }
        public int SnapshotYear { get; set; }

        public decimal? DiffMeanHourlyPayPercent { get; set; }
        public decimal? DiffMedianHourlyPercent { get; set; }
        public decimal? DiffMeanBonusPercent { get; set; }
        public decimal? DiffMedianBonusPercent { get; set; }
        public decimal? MaleMedianBonusPayPercent { get; set; }
        public decimal? FemaleMedianBonusPayPercent { get; set; }
        public decimal? MaleLowerPayBand { get; set; }
        public decimal? FemaleLowerPayBand { get; set; }
        public decimal? MaleMiddlePayBand { get; set; }
        public decimal? FemaleMiddlePayBand { get; set; }
        public decimal? MaleUpperPayBand { get; set; }
        public decimal? FemaleUpperPayBand { get; set; }
        public decimal? MaleUpperQuartilePayBand { get; set; }
        public decimal? FemaleUpperQuartilePayBand { get; set; }

        public long? ReturnId { get; set; }
        public string EncryptedOrganisationId { get; set; }
        public SectorTypes? SectorType { get; set; }

        public DateTime? AccountingDate { get; set; }
        public DateTime Modified { get; set; }

        public string JobTitle { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyLinkToGPGInfo { get; set; }

        public string ReturnUrl { get; set; }
        public string OriginatingAction { get; set; }

        public string Address { get; set; }
        public string LatestAddress { get; set; }
        public string OrganisationName { get; set; }
        public string LatestOrganisationName { get; set; }
        public OrganisationSizes? OrganisationSize { get; set; }
        public string Sector { get; set; }
        public string LatestSector { get; set; }
        public bool? IsDifferentFromDatabase { get; set; }

        public bool? IsVoluntarySubmission { get; set; }
        public bool? IsLateSubmission { get; set; }
        public bool? ShouldProvideLateReason { get; set; }
        public bool? IsInScopeForThisReportYear { get; set; }
        public string LateReason { get; set; } = "";
        public string EHRCResponse { get; set; } = "";

        public DateTime? LastWrittenDateTime { get; set; }
        public long? LastWrittenByUserId { get; set; }
        public bool? HasDraftBeenModifiedDuringThisSession { get; set; }

        public DateTime? ReportingStartDate { get; set; }
        public DateTime? ReportModifiedDate { get; set; }
        public ScopeStatuses? ReportingRequirement { get; set; }

        [NotMapped]
        public bool NotRequiredToReport =>
            ReportingRequirement == ScopeStatuses.OutOfScope || ReportingRequirement == ScopeStatuses.PresumedOutOfScope;

    }
}
