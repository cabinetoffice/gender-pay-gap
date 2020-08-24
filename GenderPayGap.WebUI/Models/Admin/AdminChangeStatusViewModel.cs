using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeStatusViewModel : GovUkViewModel
    {

        public ChangeOrganisationStatusViewModelActions Action { get; set; }

        public OrganisationStatuses? NewStatus { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change.")]
        public string Reason { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<InactiveUserOrganisation> InactiveUserOrganisations { get; set; }

    }

    public enum ChangeOrganisationStatusViewModelActions
    {

        Unknown,
        OfferNewStatusAndReason,
        ConfirmStatusChange

    }
}
