using System;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ReturnStatus
    {

        [JsonProperty]
        public long ReturnStatusId { get; set; }
        [JsonProperty]
        public long ReturnId { get; set; }
        [JsonProperty]
        public ReturnStatuses Status { get; set; }

        [JsonProperty]
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public string StatusDetails { get; set; }
        [JsonProperty]
        public long ByUserId { get; set; }

        public virtual User ByUser { get; set; }
        public virtual Return Return { get; set; }

    }
}
