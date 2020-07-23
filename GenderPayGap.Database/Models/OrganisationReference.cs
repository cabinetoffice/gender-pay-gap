using System;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OrganisationReference
    {

        [JsonProperty]
        public long OrganisationReferenceId { get; set; }
        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public string ReferenceName { get; set; }
        [JsonProperty]
        public string ReferenceValue { get; set; }
        [JsonProperty]
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual Organisation Organisation { get; set; }

    }
}
