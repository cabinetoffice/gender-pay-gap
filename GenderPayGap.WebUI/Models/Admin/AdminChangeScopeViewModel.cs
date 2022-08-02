using GenderPayGap.Core;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeScopeViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public long OrganisationId { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string OrganisationName { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public ScopeStatuses? CurrentScopeStatus { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

        [GovUkValidateRequiredIf(
            IsRequiredPropertyName = nameof(NewScopeStatusRequired), 
            ErrorMessageIfMissing = "Please select a new scope.")]
        public NewScopeStatus? NewScopeStatus { get; set; }

        public bool NewScopeStatusRequired => 
            CurrentScopeStatus != ScopeStatuses.InScope && 
            CurrentScopeStatus != ScopeStatuses.OutOfScope;

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }

    }

    public enum NewScopeStatus
    {

        InScope = 0,
        OutOfScope = 1

    }

}
