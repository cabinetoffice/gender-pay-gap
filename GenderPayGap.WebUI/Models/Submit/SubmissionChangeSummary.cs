namespace GenderPayGap.WebUI.Models.Submit
{

    public class SubmissionChangeSummary
    {

        public bool FiguresChanged { get; set; }
        public bool PersonResonsibleChanged { get; set; }
        public bool OrganisationSizeChanged { get; set; }
        public bool WebsiteUrlChanged { get; set; }
        public bool LateReasonChanged { get; set; }
        public bool IsPrevReportingStartYear { get; set; }


        public bool HasChanged =>
            FiguresChanged
            || PersonResonsibleChanged
            || OrganisationSizeChanged
            || WebsiteUrlChanged
            || ShouldProvideLateReason && LateReasonChanged;

        public bool ShouldProvideLateReason => (FiguresChanged || PersonResonsibleChanged) && IsPrevReportingStartYear;

        public string Modifications { get; set; }

    }

}
