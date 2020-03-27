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

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please select a new status.")]
        public NewStatusesFromActive? NewStatusFromActive { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please select a new status.")]
        public NewStatusesFromRetired? NewStatusFromRetired { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please select a new status.")]
        public NewStatusesFromDeleted? NewStatusFromDeleted { get; set; }

    }

    public enum NewStatusesFromActive
    {

        Retired = 1,
        Deleted = 2

    }

    public enum NewStatusesFromRetired
    {

        Active = 0,
        Deleted = 2

    }

    public enum NewStatusesFromDeleted
    {

        Active = 0,
        Retired = 1

    }
}
