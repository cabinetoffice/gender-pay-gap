using GenderPayGap.Core.Models.CompaniesHouse;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeCompaniesHouseOptInOutViewModel : GovUkViewModel
    {

        public Database.Organisation Organisation { get; set; }

        public CompaniesHouseCompany CompaniesHouseCompany { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

    }
}
