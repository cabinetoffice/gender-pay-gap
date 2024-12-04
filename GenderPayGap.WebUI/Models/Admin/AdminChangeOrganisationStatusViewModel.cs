using GenderPayGap.Core;
using GenderPayGap.Database;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeOrganisationStatusViewModel
    {

        public ChangeOrganisationStatusViewModelActions Action { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please select a new status")]
        public OrganisationStatuses? NewStatus { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [GovUkValidateCharacterCount(MaxCharacters = 250, NameAtStartOfSentence = "Reason", NameWithinSentence = "Reason")]
        public string Reason { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Organisation Organisation { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<InactiveUserOrganisation> InactiveUserOrganisations { get; set; }

        public List<AdminChangeOrganisationStatusReportingYearViewModel> Years { get; set; } =
            new List<AdminChangeOrganisationStatusReportingYearViewModel>();

        public bool AnyGuessedScopeChanges { get; set; }

    }
    
    public class AdminChangeOrganisationStatusReportingYearViewModel
    {

        public int ReportingYear { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public ScopeStatuses CurrentScope { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool HasReported { get; set; }

        public bool MarkAsOutOfScope { get; set; } = false;

    }

    public enum ChangeOrganisationStatusViewModelActions
    {

        Unknown,
        OfferNewStatusAndReason,
        ConfirmStatusChange

    }
}
