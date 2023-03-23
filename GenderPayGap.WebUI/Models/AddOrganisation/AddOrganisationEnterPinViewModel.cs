using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class AddOrganisationEnterPinViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.UserOrganisation UserOrganisation { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter the PIN for this organisation")]
        public string Pin { get; set; }
        
    }
}
