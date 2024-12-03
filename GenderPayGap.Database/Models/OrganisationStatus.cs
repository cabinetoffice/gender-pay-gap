using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OrganisationStatus
    {

        [JsonProperty]
        public long OrganisationStatusId { get; set; }
        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public OrganisationStatuses Status { get; set; }

        [JsonProperty]
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public string StatusDetails { get; set; }
        [JsonProperty]
        public long? ByUserId { get; set; }

        public virtual User ByUser { get; set; }
        public virtual Organisation Organisation { get; set; }

    }
}
