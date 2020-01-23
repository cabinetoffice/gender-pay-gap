using System;
using GenderPayGap.Core;

namespace GenderPayGap.BusinessLogic.Models
{
    [Serializable]
    public class LateSubmissionsFileModel
    {

        public long OrganisationId { get; internal set; }
        public string OrganisationName { get; internal set; }
        public SectorTypes OrganisationSectorType { get; internal set; }
        public long ReportId { get; internal set; }
        public DateTime ReportSnapshotDate { get; internal set; }
        public string ReportLateReason { get; internal set; }
        public DateTime ReportSubmittedDate { get; internal set; }
        public DateTime ReportModifiedDate { get; internal set; }
        public string ReportModifiedFields { get; internal set; }
        public string ReportPersonResonsible { get; internal set; }
        public bool ReportEHRCResponse { get; internal set; }

    }
}
