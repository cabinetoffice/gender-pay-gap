using System.Collections.Generic;
using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangePublicSectorClassificationViewModel : GovUkViewModel
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }

        public List<PublicSectorType> PublicSectorTypes { get; set; }

        public int? SelectedPublicSectorTypeId { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change")]
        public string Reason { get; set; }
    }
}
