using System;
using System.Collections.Generic;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{

    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class OrganisationPublicSectorType
    {

        [JsonProperty]
        public long OrganisationPublicSectorTypeId { get; set; }

        [JsonProperty]
        public int PublicSectorTypeId { get; set; }

        [JsonProperty]
        public long OrganisationId { get; set; }

        [JsonProperty]
        public string Source { get; set; }

        [JsonProperty]
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual PublicSectorType PublicSectorType { get; set; }

    }
}
