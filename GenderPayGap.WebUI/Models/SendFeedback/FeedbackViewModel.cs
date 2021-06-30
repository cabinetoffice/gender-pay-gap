using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GovUkDesignSystem;
using GovUkDesignSystem.Attributes;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace GenderPayGap.WebUI.Controllers.SendFeedback
{

    public class FeedbackViewModel : GovUkViewModel
    {

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "How easy is this service to use",
            NameWithinSentence = "how easy this service is to use")]
        [GovUkValidateRequired(ErrorMessageIfMissing = "Select how easy this service is to use.")]
        public HowEasyIsThisServiceToUse? HowEasyIsThisServiceToUse { get; set; }

        public List<HowDidYouHearAboutGpg> HowDidYouHearAboutGpg { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "Other source", NameWithinSentence = "other source")]
        [GovUkValidateCharacterCount(MaxCharacters = 2000)]
        public string OtherSourceText { get; set; }

        public List<WhyVisitGpgSite> WhyVisitGpgSite { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "Other reason", NameWithinSentence = "other reason")]
        [GovUkValidateCharacterCount(MaxCharacters = 2000)]
        public string OtherReasonText { get; set; }

        public List<WhoAreYou> WhoAreYou { get; set; }

        [GovUkDisplayNameForErrors(NameAtStartOfSentence = "Other person", NameWithinSentence = "other person")]
        [GovUkValidateCharacterCount(MaxCharacters = 2000)]
        public string OtherPersonText { get; set; }

        [GovUkDisplayNameForErrors(
            NameAtStartOfSentence = "Details",
            NameWithinSentence = "details")]
        [GovUkValidateCharacterCount(MaxCharacters = 2000)]
        public string Details { get; set; }

        public string EmailAddress { get; set; }

        public string PhoneNumber { get; set; }

    }

    public enum HowEasyIsThisServiceToUse
    {

        [GovUkRadioCheckboxLabelText(Text = "Very easy")]
        VeryEasy = 0,

        [GovUkRadioCheckboxLabelText(Text = "Easy")]
        Easy = 1,

        [GovUkRadioCheckboxLabelText(Text = "Neither easy nor difficult")]
        Neutral = 2,

        [GovUkRadioCheckboxLabelText(Text = "Difficult")]
        Difficult = 3,

        [GovUkRadioCheckboxLabelText(Text = "Very difficult")]
        VeryDifficult = 4

    }

    public enum HowDidYouHearAboutGpg
    {

        [GovUkRadioCheckboxLabelText(Text = "News article")]
        NewsArticle,

        [GovUkRadioCheckboxLabelText(Text = "Social media")]
        SocialMedia,

        [GovUkRadioCheckboxLabelText(Text = "Company intranet")]
        CompanyIntranet,

        [GovUkRadioCheckboxLabelText(Text = "Employer union")]
        EmployerUnion,

        [GovUkRadioCheckboxLabelText(Text = "Internet search for a company")]
        InternetSearch,

        [GovUkRadioCheckboxLabelText(Text = "Charity")]
        Charity,

        [GovUkRadioCheckboxLabelText(Text = "Lobby group")]
        LobbyGroup,

        [GovUkRadioCheckboxLabelText(Text = "By having to report gender pay gap data")]
        Report,

        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other

    }

    public enum WhyVisitGpgSite
    {

        [GovUkRadioCheckboxLabelText(Text = "I wanted to find out what the gender pay gap is")]
        FindOutAboutGpg,

        [GovUkRadioCheckboxLabelText(Text = "I reported my organisation's gender pay gap data")]
        ReportOrganisationGpgData,

        [GovUkRadioCheckboxLabelText(Text = "I wanted to understand how I can close my organisation's gender pay gap")]
        CloseOrganisationGpg,

        [GovUkRadioCheckboxLabelText(Text = "I viewed a specific organisation's gender pay gap")]
        ViewSpecificOrganisationGpg,

        [GovUkRadioCheckboxLabelText(Text = "I wanted to know what action other organisations are taking to close the gender pay gap")]
        ActionsToCloseGpg,

        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other

    }

    public enum WhoAreYou
    {

        [GovUkRadioCheckboxLabelText(Text = "An employee interested in your organisation’s gender pay gap data?")]
        EmployeeInterestedInOrganisationData,

        [GovUkRadioCheckboxLabelText(Text = "A manager involved in gender pay gap reporting or diversity and inclusion?")]
        ManagerInvolvedInGpgReport,

        [GovUkRadioCheckboxLabelText(Text = "A person responsible for reporting your organisation’s gender pay gap?")]
        ResponsibleForReportingGpg,

        [GovUkRadioCheckboxLabelText(Text = "A person interested in the gender pay gap generally?")]
        PersonInterestedInGeneralGpg,

        [GovUkRadioCheckboxLabelText(Text = "A person interested in a specific organisation’s gender pay gap?")]
        PersonInterestedInSpecificOrganisationGpg,

        [GovUkRadioCheckboxLabelText(Text = "Other")]
        Other

    }

}
