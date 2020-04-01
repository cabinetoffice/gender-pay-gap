using GenderPayGap.Core;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeStatusViewModel : GovUkViewModel
    {

        public long OrganisationId { get; set; }

        public string OrganisationName { get; set; }
        public OrganisationStatuses CurrentStatus { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

        public OrganisationStatuses? NewStatus { get; set; }

    }
}
