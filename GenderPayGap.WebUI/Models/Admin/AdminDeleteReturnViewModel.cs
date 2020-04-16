using System.Collections.Generic;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminDeleteReturnViewModel : GovUkViewModel
    {
        public List<long> ReturnIds { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

        // Not mapped, only used for displaying information in the views
        public Database.Organisation Organisation { get; set; }
        public int Year { get; set; }

    }
}
