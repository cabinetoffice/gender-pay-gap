using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminBulkMarkFeedbackAsSpamViewModel
    {
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter the feedback IDs you want to mark as spam")]
        public string FeedbackIdsToMarkAsSpam { get; set; }

    }
}
