using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.ReportStarting
{
    public class ReportStartingViewModel
    {
        public Organisation Organisation { get; set; }
        public int ReportingYear { get; set; }
        public DateTime SnapshotDate { get; set; }
    }
}
