using System.Collections.Generic;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Models.AddOrganisation;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminPendingRegistrationViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public UserOrganisation UserOrganisation { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<AddOrganisationSearchResult> SimilarOrganisationsFromOurDatabase { get; set; }
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public List<AddOrganisationSearchResult> SimilarOrganisationsFromCompaniesHouse { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select whether to approve or reject this registration")]
        public AdminPendingRegistrationApproveOrReject? ApproveOrReject { get; set; }

        [GovUkValidateRequiredIf(
            IsRequiredPropertyName = nameof(RejectionReasonRequired),
            ErrorMessageIfMissing = "Enter a reason for rejecting this registration")]
        [GovUkValidateCharacterCount(MaxCharacters = 250, NameAtStartOfSentence = "Reason", NameWithinSentence = "Reason")]
        public string RejectionReason { get; set; }
        public bool RejectionReasonRequired => ApproveOrReject == AdminPendingRegistrationApproveOrReject.Reject;
        
    }
    
    public enum AdminPendingRegistrationApproveOrReject
    {

        Approve,
        Reject

    }
}
