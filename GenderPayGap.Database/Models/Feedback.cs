using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core;
using Newtonsoft.Json;

namespace GenderPayGap.Database.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Feedback
    {

        [JsonProperty]
        public long FeedbackId { get; set; }

        #region DifficultyTypes

        [JsonProperty]
        public DifficultyTypes? Difficulty { get; set; }

        #endregion

        [MaxLength(2000)]
        [JsonProperty]
        public string Details { get; set; }

        [JsonProperty]
        public string EmailAddress { get; set; }
        [JsonProperty]
        public string PhoneNumber { get; set; }

        [JsonProperty]
        public DateTime CreatedDate { get; set; }

        [JsonProperty]
        public bool HasBeenAnonymised { get; set; }

        #region HowDidYouHearAboutGpg

        [JsonProperty]
        public bool? NewsArticle { get; set; }

        [JsonProperty]
        public bool? SocialMedia { get; set; }

        [JsonProperty]
        public bool? CompanyIntranet { get; set; }

        [JsonProperty]
        public bool? EmployerUnion { get; set; }

        [JsonProperty]
        public bool? InternetSearch { get; set; }

        [JsonProperty]
        public bool? Charity { get; set; }

        [JsonProperty]
        public bool? LobbyGroup { get; set; }

        [JsonProperty]
        public bool? Report { get; set; }

        [JsonProperty]
        public bool? OtherSource { get; set; }

        [MaxLength(2000)]
        [JsonProperty]
        public string OtherSourceText { get; set; }

        #endregion

        #region WhyVisitGpgSite

        [JsonProperty]
        public bool? FindOutAboutGpg { get; set; }

        [JsonProperty]
        public bool? ReportOrganisationGpgData { get; set; }

        [JsonProperty]
        public bool? CloseOrganisationGpg { get; set; }

        [JsonProperty]
        public bool? ViewSpecificOrganisationGpg { get; set; }

        [JsonProperty]
        public bool? ActionsToCloseGpg { get; set; }

        [JsonProperty]
        public bool? OtherReason { get; set; }

        [MaxLength(2000)]
        [JsonProperty]
        public string OtherReasonText { get; set; }

        #endregion

        #region WhoAreYou

        [JsonProperty]
        public bool? EmployeeInterestedInOrganisationData { get; set; }

        [JsonProperty]
        public bool? ManagerInvolvedInGpgReport { get; set; }

        [JsonProperty]
        public bool? ResponsibleForReportingGpg { get; set; }

        [JsonProperty]
        public bool? PersonInterestedInGeneralGpg { get; set; }

        [JsonProperty]
        public bool? PersonInterestedInSpecificOrganisationGpg { get; set; }

        [JsonProperty]
        public bool? OtherPerson { get; set; }

        [MaxLength(2000)]
        [JsonProperty]
        public string OtherPersonText { get; set; }

        #endregion

        [JsonProperty]
        public FeedbackStatus FeedbackStatus { get; set; }
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
