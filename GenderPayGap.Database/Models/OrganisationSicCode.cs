using System;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OrganisationSicCode
    {

        [JsonProperty]
        public int SicCodeId { get; set; }
        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public DateTime Created { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public long OrganisationSicCodeId { get; set; }
        [JsonProperty]
        public string Source { get; set; }
        [JsonProperty]
        public DateTime? Retired { get; set; }

        public virtual Organisation Organisation { get; set; }
        public virtual SicCode SicCode { get; set; }

        public bool IsRetired()
        {
            return Retired.HasValue;
        }

    }
}
