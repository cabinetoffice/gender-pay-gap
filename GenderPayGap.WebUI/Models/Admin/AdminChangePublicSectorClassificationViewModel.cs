using System.Collections.Generic;
using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangePublicSectorClassificationViewModel 
    {
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public long OrganisationId { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string OrganisationName { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<PublicSectorType> PublicSectorTypes { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please select a public sector classification")]
        public int? SelectedPublicSectorTypeId { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change")]
        [GovUkValidateCharacterCount(MaxCharacters = 250, NameAtStartOfSentence = "Reason", NameWithinSentence = "Reason")]
        public string Reason { get; set; }
    }
}
