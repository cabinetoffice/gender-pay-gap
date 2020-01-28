using GovUkDesignSystem;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminOrganisationCompanyNumberViewModel : GovUkViewModel
    {

        public Database.Organisation Organisation { get; set; }
        


    }

    public enum AdminOrganisationCompanyNumberChangeOrRemove
    {
        Change,
        Remove
    }
}
