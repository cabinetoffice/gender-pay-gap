namespace GenderPayGap.WebUI.Models
{

    public class ReportStatusBadgeViewModel
    {
        public ReportStatusBadgeType ReportStatus { get; set; }
        public string DateText { get; set; }
        public bool Desktop { get; set; }

    }

    public enum ReportStatusBadgeType
    {
        Due,
        OverDue,
        Reported,
        NotRequired,
        NotRequiredDueToCovid,
        VoluntarilyReported,
        SubmittedLate,
    }

}
