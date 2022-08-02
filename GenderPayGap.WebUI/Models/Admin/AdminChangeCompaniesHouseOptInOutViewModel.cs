using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeCompaniesHouseOptInOutViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public CompaniesHouseCompany CompaniesHouseCompany { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

    }
}
