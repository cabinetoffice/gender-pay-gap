using GenderPayGap.Core;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeScopeViewModel : GovUkViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public long OrganisationId { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string OrganisationName { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public ScopeStatuses? CurrentScopeStatus { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [GovUkValidateCharacterCount(MaxCharacters = 250)]
        public string Reason { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please select a new scope.")]
        public NewScopeStatus? NewScopeStatus { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }

    }

    public enum NewScopeStatus
    {

        InScope = 0,
        OutOfScope = 1

    }

}
