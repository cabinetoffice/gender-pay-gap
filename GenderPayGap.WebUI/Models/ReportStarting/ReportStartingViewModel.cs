using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.ReportStarting
{
    public class ReportStartingViewModel
    {
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }
        
        public bool IsEditingSubmittedReturn { get; set; }
        public bool IsEditingForTheFirstTime { get; set; }
        
        public DateTime SnapshotDate { get; set; }

    }
}
