using System;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminHomepageViewModel
    {

        public int FeedbackCount { get; set; }
        public DateTime? LatestFeedbackDate { get; set; }
        public int NewFeedbackCount { get; set; }

    }
}
