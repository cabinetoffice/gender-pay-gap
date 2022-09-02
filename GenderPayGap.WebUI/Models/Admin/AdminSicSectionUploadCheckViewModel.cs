using GenderPayGap.Database;
using GenderPayGap.WebUI.Models.AdminReferenceData;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminSicSectionUploadCheckViewModel 
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public AddsEditsDeletesSet<SicSection> AddsEditsDeletesSet { get; set; }

        public string SerializedNewRecords { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        [GovUkValidateCharacterCount(MaxCharacters = 250)]
        public string Reason { get; set; }

    }

}
