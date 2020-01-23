using System;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Models.Scope
{
    [Serializable]
    public class ScopeViewModel
    {

        public long OrganisationScopeId { get; set; }
        public ScopeStatuses ScopeStatus { get; set; }
        public DateTime StatusDate { get; set; }
        public DateTime SnapshotDate { get; set; }
        public long LatestReturnId { get; set; }
        public RegisterStatuses RegisterStatus { get; set; }

    }
}
