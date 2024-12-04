using GenderPayGap.Database;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.ManageOrganisations
{
    public class DeclareScopeViewModel
    {
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Select if your employer was required to report for the reporting year.")]
        public DeclareScopeRequiredToReportOptions? DeclareScopeRequiredToReport { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int PreviousReportingYear { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Organisation Organisation { get; set; }

    }
    
    public enum DeclareScopeRequiredToReportOptions
    {
        Yes,
        No
    }
    
}
