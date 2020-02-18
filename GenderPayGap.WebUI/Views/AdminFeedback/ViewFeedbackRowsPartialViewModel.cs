using System.Collections.Generic;
using GenderPayGap.Database.Models;

namespace GenderPayGap.WebUI.Views.AdminFeedback
{
    public class ViewFeedbackRowsPartialViewModel
    {
        public List<Feedback> FeedbackRows { get; set; }
        public string ButtonClasses { get; set; }
        public string FeedbackTitle { get; set; }
        public bool Open { get; set; }
        public bool ShowMarkAsSpamButton { get; set; }
        public bool ShowMarkAsNotSpamButton { get; set; }

    }
}
