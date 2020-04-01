using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeStatusViewModel : GovUkViewModel
    {

        public ChangeOrganisationStatusViewModelActions Action { get; set; }

        public OrganisationStatuses? NewStatus { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

        // Not mapped, only used for displaying information in the views
        public Database.Organisation Organisation { get; set; }
        public List<InactiveUserOrganisation> InactiveUserOrganisations { get; set; }

    }

    public enum ChangeOrganisationStatusViewModelActions
    {

        Unknown,
        OfferNewStatusAndReason,
        ConfirmStatusChange

    }
}
