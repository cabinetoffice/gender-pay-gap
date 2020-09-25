using System;

namespace GenderPayGap.WebUI.Models.Organisation
{

    [Serializable]
    public class DisplayReportModel
    {
        public Database.Organisation Organisation { get; set; }
        public int ReportingYear { get; set; }
        public bool CanChangeScope { get; set; }
        public bool HasDraftReturnForYear { get; set; }
    }

}
