using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeUserStatusViewModel 
    {

        public ChangeUserStatusViewModelActions Action { get; set; }

        [GovUkValidateRequiredIf(
            IsRequiredPropertyName = nameof(NewStatusRequired), 
            ErrorMessageIfMissing = "Please select a new status")]
        public UserStatuses? NewStatus { get; set; }

        public bool NewStatusRequired => Action is ChangeUserStatusViewModelActions.OfferNewStatusAndReason;

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [GovUkValidateCharacterCount(MaxCharacters = 250, NameAtStartOfSentence = "Reason", NameWithinSentence = "Reason")]
        public string Reason { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.User User { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<InactiveUserOrganisation> InactiveUserOrganisations { get; set; }

    }

    public enum ChangeUserStatusViewModelActions
    {

        Unknown,
        OfferNewStatusAndReason,
        ConfirmStatusChange

    }
}
