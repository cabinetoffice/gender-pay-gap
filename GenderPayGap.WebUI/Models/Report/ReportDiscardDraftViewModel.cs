using GenderPayGap.Database;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportDiscardDraftViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Organisation Organisation { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }

    }
}
