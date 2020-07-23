using Newtonsoft.Json;

namespace GenderPayGap.Database.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DataProtectionKey
    {

        [JsonProperty]
        public int Id { get; set; }

        [JsonProperty]
        public string FriendlyName { get; set; }

        [JsonProperty]
        public string Xml { get; set; }

    }
}
