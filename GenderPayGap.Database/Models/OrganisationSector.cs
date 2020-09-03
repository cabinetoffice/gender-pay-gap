using System;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OrganisationSector
    {

        [JsonProperty]
        public long OrganisationSectorId { get; set; }
        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public OrganisationSectors Sector { get; set; }

        [JsonProperty]
        public DateTime SectorDate { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public string SectorDetails { get; set; }
        [JsonProperty]
        public long? ByUserId { get; set; }

        public virtual User ByUser { get; set; }
        public virtual Organisation Organisation { get; set; }

    }
}

