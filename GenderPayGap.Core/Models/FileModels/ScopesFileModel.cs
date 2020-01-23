using System;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class ScopesFileModel
    {

        public long OrganisationScopeId { get; set; }

        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public string DUNSNumber { get; set; }
        public string EmployerReference { get; set; }
        public ScopeStatuses ScopeStatus { get; set; }
        public DateTime ScopeStatusDate { get; set; } = VirtualDateTime.Now;
        public RegisterStatuses RegisterStatus { get; set; }

        public DateTime RegisterStatusDate { get; set; } = VirtualDateTime.Now;

        public string ContactFirstname { get; set; }

        public string ContactLastname { get; set; }

        public string ContactEmailAddress { get; set; }

        public bool? ReadGuidance { get; set; }

        public string Reason { get; set; }
        public string CampaignId { get; set; }

    }
}
