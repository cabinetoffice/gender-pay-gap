using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.ReportStarting
{
    public class ReportStartingViewModel
    {
        public Database.Organisation Organisation { get; set; }
        public int ReportingYear { get; set; }
        public DateTime SnapshotDate { get; set; }
    }
}
