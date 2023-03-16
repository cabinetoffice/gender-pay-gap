using GenderPayGap.Database.Models;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminFeedbackToCategoriseViewModel
    {
        
        public int NumberOfNewFeedbacks { get; set; }
        public Feedback FeedbackToCategorise { get; set; }

    }
}
