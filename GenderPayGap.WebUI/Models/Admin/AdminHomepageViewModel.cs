using System;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminHomepageViewModel
    {

        public bool IsSuperAdministrator { get; set; }
        public bool IsDatabaseAdministrator { get; set; }
        public bool IsDowngradedDueToIpRestrictions { get; set; }
        public int FeedbackCount { get; set; }
        public DateTime? LatestFeedbackDate { get; set; }
        public int NewFeedbackCount { get; set; }

    }
}
