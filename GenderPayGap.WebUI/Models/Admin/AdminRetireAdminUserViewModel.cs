using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminRetireAdminUserViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.User User { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [GovUkValidateCharacterCount(MaxCharacters = 250, NameAtStartOfSentence = "Reason", NameWithinSentence = "Reason")]
        public string Reason { get; set; }

    }
}
