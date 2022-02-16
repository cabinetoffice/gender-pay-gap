using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class ReportResponsiblePersonViewModel : GovUkViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }
        
        [GovUkValidateCharacterCount(MaxCharacters = 50)]
        public string ResponsiblePersonFirstName { get; set; }
        
        [GovUkValidateCharacterCount(MaxCharacters = 50)]
        public string ResponsiblePersonLastName { get; set; }
        
        [GovUkValidateCharacterCount(MaxCharacters = 100)]
        public string ResponsiblePersonJobTitle { get; set; }

    }
}
