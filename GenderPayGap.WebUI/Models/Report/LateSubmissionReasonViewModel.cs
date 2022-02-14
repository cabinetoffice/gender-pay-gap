using System;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Report
{
    public class LateSubmissionReasonViewModel : GovUkViewModel
    {

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public int ReportingYear { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public bool IsEditingSubmittedReturn { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public DateTime DeadlineDate { get; set; }

        [GovUkValidateRequired(ErrorMessageIfMissing = "Select yes if you have received a letter about a breach of the regulations from the Equality and Human Rights Commission.")]
        public ReportLateSubmissionReceivedLetterFromEhrc? ReceivedLetterFromEhrc { get; set; }
        [GovUkValidateRequired(ErrorMessageIfMissing = "Explain why your organisation is reporting or changing your gender pay gap figures after the deadline.")]
        public string Reason { get; set; }

    }

    public enum ReportLateSubmissionReceivedLetterFromEhrc
    {
        Yes,
        No
    }
}
