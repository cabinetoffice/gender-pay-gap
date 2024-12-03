using GenderPayGap.Core;
using GenderPayGap.Database;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeMultipleScopesViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Organisation Organisation { get; set; }

        public List<AdminChangeMultipleScopesReportingYearViewModel> Years { get; set; } =
            new List<AdminChangeMultipleScopesReportingYearViewModel>();

        public bool AnyGuessedScopeChanges { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [GovUkValidateCharacterCount(MaxCharacters = 250, NameAtStartOfSentence = "Reason", NameWithinSentence = "Reason")]
        public string Reason { get; set; }
        
    }

    public class AdminChangeMultipleScopesReportingYearViewModel
    {

        public int ReportingYear { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public ScopeStatuses CurrentScope { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool HasReported { get; set; }

        public ScopeStatuses? NewScope { get; set; }

    }

}
