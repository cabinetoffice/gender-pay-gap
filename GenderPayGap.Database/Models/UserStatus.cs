using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserStatus
    {

        [JsonProperty]
        public long UserStatusId { get; set; }
        [JsonProperty]
        public long UserId { get; set; }
        [JsonProperty]
        public UserStatuses Status { get; set; }
        [JsonProperty]
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public string StatusDetails { get; set; }
        [JsonProperty]
        public long ByUserId { get; set; }

        public virtual User ByUser { get; set; }
        public virtual User User { get; set; }

    }
}
