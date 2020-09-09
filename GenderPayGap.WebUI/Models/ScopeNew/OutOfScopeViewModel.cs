using System;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;

namespace GenderPayGap.WebUI.Models.ScopeNew
{
    [Serializable]
    public class OutOfScopeViewModel : GovUkViewModel
    {

        public Database.Organisation Organisation { get; set; }
        
        public DateTime ReportingYear { get; set; }

        public WhyOutOfScope? WhyOutOfScope { get; set; }
        
        public string WhyOutOfScopeDetails { get; set; }
        
        public HaveReadGuidance? HaveReadGuidance { get; set; }
    }
    
    public enum WhyOutOfScope
    {
        [GovUkRadioCheckboxLabelText(Text = "My organisation had fewer than 250 employees")]
        Under250 = 0,

        [GovUkRadioCheckboxLabelText(Text = "Other reason")]
        Other = 1
    }
    
    public enum HaveReadGuidance
    {
        [GovUkRadioCheckboxLabelText(Text = "Yes")]
        HaveReadGuidance = 0,

        [GovUkRadioCheckboxLabelText(Text = "No")]
        HaveNotReadGuidance = 1
    }
}
