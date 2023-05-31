using GenderPayGap.WebUI.ExternalServices.CompaniesHouse;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminAddNewAdminUserViewModel 
    {

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter an email address")]
        public string EmailAddress { get; set; }

    }
}
