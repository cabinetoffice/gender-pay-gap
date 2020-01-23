using System;

namespace GenderPayGap.WebUI.Models.Scope
{

    [Serializable]
    public class ScopingViewModel
    {

        public long OrganisationId { get; set; }
        public string DUNSNumber { get; set; }
        public string OrganisationName { get; set; }

        public string OrganisationAddress { get; set; }

        public DateTime AccountingDate { get; set; }

        public string CampaignId { get; set; } = "-1";

        public string StartUrl { get; set; }
        public bool IsChangeJourney { get; set; }
        public bool IsOutOfScopeJourney { get; set; }
        public bool IsSecurityCodeExpired { get; set; }
        public bool UserIsRegistered { get; set; }

        public EnterCodesViewModel EnterCodes { get; set; } = new EnterCodesViewModel();
        public EnterAnswersViewModel EnterAnswers { get; set; } = new EnterAnswersViewModel();

        public ScopeViewModel LastScope { get; set; }
        public ScopeViewModel ThisScope { get; set; }

        public string OrgAddressHtml => OrganisationAddress?.Replace(",", "<br />");

    }

}
