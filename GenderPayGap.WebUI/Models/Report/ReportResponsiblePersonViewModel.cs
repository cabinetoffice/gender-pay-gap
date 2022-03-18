using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
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
        public bool IsEditingForTheFirstTime { get; set; }
        
        [GovUkValidateCharacterCount(MaxCharacters = 50)]
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "The responsible person first name",
            NameWithinSentence = "the responsible person first name")]
        public string ResponsiblePersonFirstName { get; set; }
        
        [GovUkValidateCharacterCount(MaxCharacters = 50)]
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "The responsible person last name",
            NameWithinSentence = "the responsible person last name")]
        public string ResponsiblePersonLastName { get; set; }
        
        [GovUkValidateCharacterCount(MaxCharacters = 100)]
        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "The responsible person job title",
            NameWithinSentence = "the responsible person job title")]
        public string ResponsiblePersonJobTitle { get; set; }

    }
}
