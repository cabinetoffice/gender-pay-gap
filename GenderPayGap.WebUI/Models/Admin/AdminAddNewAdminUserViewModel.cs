using GovUkDesignSystem.Attributes.ValidationAttributes;
using GovUkDesignSystem.ModelBinders;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminAddNewAdminUserViewModel 
    {

        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter an email address")]
        public string EmailAddress { get; set; }

        [ModelBinder(typeof(GovUkCheckboxBoolBinder))]
        public bool ReadOnly { get; set; }

    }
}
