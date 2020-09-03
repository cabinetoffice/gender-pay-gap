using System;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OrganisationName
    {

        [JsonProperty]
        public long OrganisationNameId { get; set; }
        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public string Name { get; set; }
        [JsonProperty]
        public string Source { get; set; }
        [JsonProperty]
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual Organisation Organisation { get; set; }
    }
}
