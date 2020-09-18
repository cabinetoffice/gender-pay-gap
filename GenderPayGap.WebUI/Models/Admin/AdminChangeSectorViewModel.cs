using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeSectorViewModel : GovUkViewModel
    {

        public ChangeOrganisationSectorViewModelActions Action { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select a new sector.")]
        public NewSectorTypes? NewSector { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Give a reason for this change.")]
        public string Reason { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }

    }

    public enum ChangeOrganisationSectorViewModelActions
    {

        Unknown,
        OfferNewSectorAndReason,
        ConfirmSectorChange

    }

    public enum NewSectorTypes
    {

        Public,
        Private

    }
}
