using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models
{
    public class AdminChangeUserContactPreferencesViewModel : GovUkViewModel
    {

        public long UserId { get; set; }
        public string FullName { get; set; }

        public bool AllowContact { get; set; }
        public bool SendUpdates { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

    }
}
