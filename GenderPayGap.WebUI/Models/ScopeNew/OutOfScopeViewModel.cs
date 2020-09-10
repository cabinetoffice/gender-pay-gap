using System;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.ScopeNew
{
    [Serializable]
    public class OutOfScopeViewModel : GovUkViewModel
    {

        [BindNever]
        public Database.Organisation Organisation { get; set; }
        
        [BindNever]
        public DateTime ReportingYear { get; set; }

        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Select a reason why this organisation is out of scope."
        )]
        public WhyOutOfScope? WhyOutOfScope { get; set; }
        
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Details."
        )]
        public string WhyOutOfScopeDetails { get; set; }
        
        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Please ensure you have read the guidance before continuing."
        )]
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
