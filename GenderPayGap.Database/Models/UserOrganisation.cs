using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class UserOrganisation
    {

        [JsonProperty]
        public long UserId { get; set; }
        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public string PIN { get; set; }
        [JsonProperty]
        public DateTime? PINSentDate { get; set; }
        [JsonProperty]
        public string PITPNotifyLetterId { get; set; }
        [JsonProperty]
        public DateTime? PINConfirmedDate { get; set; }
        [JsonProperty]
        public DateTime? ConfirmAttemptDate { get; set; }
        [JsonProperty]
        public int ConfirmAttempts { get; set; }
        [JsonProperty]
        public DateTime Created { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public RegistrationMethods Method { get; set; } = RegistrationMethods.Unknown;

        public virtual Organisation Organisation { get; set; }

        public virtual User User { get; set; }

    }
}
