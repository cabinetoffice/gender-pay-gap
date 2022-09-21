using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models
{
    public class AdminChangeUserContactPreferencesViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public long UserId { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string FullName { get; set; }

        public bool AllowContact { get; set; }
        public bool SendUpdates { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [GovUkValidateCharacterCount(MaxCharacters = 250, NameAtStartOfSentence = "Reason", NameWithinSentence = "Reason")]
        public string Reason { get; set; }

    }
}
