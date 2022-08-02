using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminRemoveUserViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public long OrganisationId { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string OrganisationName { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public long UserId { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public string UserFullName { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool FromViewUserPage { get; set; }

    }
}
