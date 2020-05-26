using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class DeleteOrganisationNameViewModel : GovUkViewModel
    {

        // Not mapped, only used for displaying information in the views
        public Database.Organisation Organisation { get; set; }
        public OrganisationName OrganisationNameToBeDeleted { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

    }
}
