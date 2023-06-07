using System;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OrganisationScope
    {

        [JsonProperty]
        public long OrganisationScopeId { get; set; }
        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public ScopeStatuses ScopeStatus { get; set; }
        [JsonProperty]
        public DateTime ScopeStatusDate { get; set; } = VirtualDateTime.Now;

        [JsonProperty]
        public RegisterStatuses RegisterStatus { get; set; }
        [JsonProperty]
        public DateTime RegisterStatusDate { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public bool? ReadGuidance { get; set; }
        [JsonProperty]
        public string Reason { get; set; }
        [JsonProperty]
        public DateTime SnapshotDate { get; set; }
        [JsonProperty]
        public ScopeRowStatuses Status { get; set; }
        [JsonProperty]
        public string StatusDetails { get; set; }

        public virtual Organisation Organisation { get; set; }

        public bool IsInScopeVariant()
        {
            return ScopeStatus.IsInScopeVariant();
        }

        public bool IsScopePresumed()
        {
            return ScopeStatus.IsScopePresumed();
        }

    }
}
