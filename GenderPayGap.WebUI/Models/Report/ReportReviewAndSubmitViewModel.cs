using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportReviewAndSubmitViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Organisation Organisation { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public DraftReturn DraftReturn { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool WillBeLateSubmission { get; set; }

    }
}
