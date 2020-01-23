using System;
using System.ComponentModel.DataAnnotations;

namespace GenderPayGap.Database.Models
{
    public class Feedback
    {

        public long FeedbackId { get; set; }

        #region DifficultyTypes

        public DifficultyTypes? Difficulty { get; set; }

        #endregion

        [MaxLength(2000)]
        public string Details { get; set; }

        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }

        public DateTime CreatedDate { get; set; }

        #region HowDidYouHearAboutGpg

        public bool? NewsArticle { get; set; }

        public bool? SocialMedia { get; set; }

        public bool? CompanyIntranet { get; set; }

        public bool? EmployerUnion { get; set; }

        public bool? InternetSearch { get; set; }

        public bool? Charity { get; set; }

        public bool? LobbyGroup { get; set; }

        public bool? Report { get; set; }

        public bool? OtherSource { get; set; }

        [MaxLength(2000)]
        public string OtherSourceText { get; set; }

        #endregion

        #region WhyVisitGpgSite

        public bool? FindOutAboutGpg { get; set; }

        public bool? ReportOrganisationGpgData { get; set; }

        public bool? CloseOrganisationGpg { get; set; }

        public bool? ViewSpecificOrganisationGpg { get; set; }

        public bool? ActionsToCloseGpg { get; set; }

        public bool? OtherReason { get; set; }

        [MaxLength(2000)]
        public string OtherReasonText { get; set; }

        #endregion

        #region WhoAreYou

        public bool? EmployeeInterestedInOrganisationData { get; set; }

        public bool? ManagerInvolvedInGpgReport { get; set; }

        public bool? ResponsibleForReportingGpg { get; set; }

        public bool? PersonInterestedInGeneralGpg { get; set; }

        public bool? PersonInterestedInSpecificOrganisationGpg { get; set; }

        public bool? OtherPerson { get; set; }

        [MaxLength(2000)]
        public string OtherPersonText { get; set; }

        #endregion

    }

    public enum DifficultyTypes
    {

        VeryEasy = 0,
        Easy = 1,
        Neutral = 2,
        Difficult = 3,
        VeryDifficult = 4

    }
}
