using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminRemoveUserViewModel : GovUkViewModel
    {

        public long OrganisationId { get; set; }

        public string OrganisationName { get; set; }

        public long UserId { get; set; }

        public string UserFullName { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

        public bool FromViewUserPage { get; set; }

    }
}
