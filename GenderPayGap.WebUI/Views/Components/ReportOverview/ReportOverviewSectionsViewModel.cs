using System;
using System.Collections.Generic;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Html;

namespace GenderPayGap.WebUI.Views.Components.ReportOverview
{
    public class ReportOverviewSectionsViewModel : GovUkViewModel
    {
        public string Title { get; set; }
        
        public List<ReportOverviewSectionViewModel> Sections { get; set; }
    }
    
    public class ReportOverviewSectionViewModel
    {
        public string Title { get; set; }

        public string EditLink { get; set; }

        public string LeftTitle { get; set; }

        public string RightTitle { get; set; }

        public List<ReportOverviewSectionRowViewModel> Rows { get; set; }
    }

    public class ReportOverviewSectionRowViewModel
    {
        public Func<object, object> Title { get; set; }
        
        public IHtmlContent LeftValue { get; set; }
        
        public IHtmlContent RightValue { get; set; }
    }
}
