using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Models.ViewReports
{
    public class ReportStatusBadgeViewModel
    {

        public ReportStatusTag ReportStatusTag { get; set; }
        public DateTime? ReportSubmittedDate { get; set; }
        public DateTime DeadlineDate { get; set; }

    }
}
