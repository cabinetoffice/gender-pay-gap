using GenderPayGap.Database.Models;
using Newtonsoft.Json;

namespace GenderPayGap.Database.Backup
{

    [JsonObject(MemberSerialization.OptIn)]
    public class AllData
    {

        [JsonProperty]
        public List<AuditLog> AuditLogs { get; set; }
        [JsonProperty]
        public List<DataProtectionKey> DataProtectionKeys { get; set; }
        [JsonProperty]
        public List<DraftReturn> DraftReturns { get; set; }
        [JsonProperty]
        public List<Feedback> Feedbacks { get; set; }
        [JsonProperty]
        public List<InactiveUserOrganisation> InactiveUserOrganisations { get; set; }
        [JsonProperty]
        public List<Organisation> Organisations { get; set; }
        [JsonProperty]
        public List<OrganisationAddress> OrganisationAddresses { get; set; }
        [JsonProperty]
        public List<OrganisationName> OrganisationNames { get; set; }
        [JsonProperty]
        public List<OrganisationPublicSectorType> OrganisationPublicSectorTypes { get; set; }
        [JsonProperty]
        public List<OrganisationScope> OrganisationScopes { get; set; }
        [JsonProperty]
        public List<OrganisationSicCode> OrganisationSicCodes { get; set; }
        [JsonProperty]
        public List<OrganisationStatus> OrganisationStatuses { get; set; }
        [JsonProperty]
        public List<PublicSectorType> PublicSectorTypes { get; set; }
        [JsonProperty]
        public List<ReminderEmail> ReminderEmails { get; set; }
        [JsonProperty]
        public List<Return> Returns { get; set; }
        [JsonProperty]
        public List<SicCode> SicCodes { get; set; }
        [JsonProperty]
        public List<SicSection> SicSections { get; set; }
        [JsonProperty]
        public List<User> Users { get; set; }
        [JsonProperty]
        public List<UserOrganisation> UserOrganisations { get; set; }
        [JsonProperty]
        public List<UserStatus> UserStatuses { get; set; }

    }

}
