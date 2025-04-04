﻿using GenderPayGap.Database;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.ScopeNew
{
    [Serializable]
    public class ScopeViewModel 
    {
        [BindNever]
        public Organisation Organisation { get; set; }
        
        [BindNever]
        public DateTime ReportingYear { get; set; }

        [GovUkValidateRequired(
            ErrorMessageIfMissing = "Select a reason why this employer is out of scope."
        )]
        public WhyOutOfScope? WhyOutOfScope { get; set; }
        
        [GovUkValidateRequiredIf(
            IsRequiredPropertyName = nameof(WhyOutOfScopeDetailsRequired),
            ErrorMessageIfMissing = "Provide a reason that this employer is out of scope."
        )]
        [GovUkValidateCharacterCount(MaxCharacters = 250, NameAtStartOfSentence = "Reason", NameWithinSentence = "Reason")]
        public string WhyOutOfScopeDetails { get; set; }
        public bool WhyOutOfScopeDetailsRequired => WhyOutOfScope is ScopeNew.WhyOutOfScope.Other;

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select yes if you have read the guidance.")]
        public HaveReadGuidance? HaveReadGuidance { get; set; }

        // Reverse of whether an organisation is in scope to set view version
        public bool IsToSetInScope { get; set; }
    }
    
    public enum WhyOutOfScope
    {
        /*
         * There would normally be an annotation here, but we want the label to read as
         * "Why is your organisation not required to report their gender pay gap data for the [reporting year] reporting year?", so
         * this needs to change slightly depending on the year for which the scope is being changed.
         * The label is set in OutOfScopeQuestions.cshtml - an annotation may be needed here in future if this enum is used elsewhere
         * without a changeable label.
        */
        Under250 = 0,

        [GovUkRadioCheckboxLabelText(Text = "Other reason")]
        Other = 1
    }
    
    public enum HaveReadGuidance
    {
        [GovUkRadioCheckboxLabelText(Text = "Yes")]
        Yes = 0,

        [GovUkRadioCheckboxLabelText(Text = "No")]
        No = 1
    }

}
