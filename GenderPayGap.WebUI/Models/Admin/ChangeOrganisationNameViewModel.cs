﻿using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class ChangeOrganisationNameViewModel : GovUkViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }

        public ManuallyChangeOrganisationNameViewModelActions Action { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a name")]
        public string Name { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing =
            "Select whether you want to use this name from Companies House or to enter a name manually")]
        public AcceptCompaniesHouseName? AcceptCompaniesHouseName { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Please enter a reason for this change")]
        public string Reason { get; set; }

    }

    public enum AcceptCompaniesHouseName
    {
        [GovUkRadioCheckboxLabelText(Text = "Yes, use this name from Companies House")]
        Accept,

        [GovUkRadioCheckboxLabelText(Text = "No, enter a name manually")]
        Reject
    }

    public enum ManuallyChangeOrganisationNameViewModelActions
    {
        Unknown = 0,
        OfferNewCompaniesHouseName = 1,
        ManualChange = 2,
        CheckChangesManual = 3,
        CheckChangesCoHo = 4
    }
}
