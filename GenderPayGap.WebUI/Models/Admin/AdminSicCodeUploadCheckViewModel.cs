using GenderPayGap.Database;
using GenderPayGap.WebUI.Models.AdminReferenceData;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminSicCodeUploadCheckViewModel : GovUkViewModel
    {

        // Only used to send data to the view - we don't expect the client to send this data back to us
        public AddsEditsDeletesSet<SicCode> AddsEditsDeletesSet { get; set; }

        public string SerializedNewRecords { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

    }

}
