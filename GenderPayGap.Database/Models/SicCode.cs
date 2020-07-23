using System;
using System.Collections.Generic;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class SicCode
    {

        public SicCode()
        {
            OrganisationSicCodes = new HashSet<OrganisationSicCode>();
        }

        [JsonProperty]
        public int SicCodeId { get; set; }
        [JsonProperty]
        public string SicSectionId { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public string Synonyms { get; set; }
        [JsonProperty]
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual SicSection SicSection { get; set; }
        public virtual ICollection<OrganisationSicCode> OrganisationSicCodes { get; set; }

    }
}
