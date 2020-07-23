using System;
using System.Collections.Generic;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class SicSection
    {

        public SicSection()
        {
            SicCodes = new HashSet<SicCode>();
        }

        [JsonProperty]
        public string SicSectionId { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual ICollection<SicCode> SicCodes { get; set; }

    }
}
