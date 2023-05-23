using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Organisation
    {

        public Organisation()
        {
            OrganisationAddresses = new HashSet<OrganisationAddress>();
            OrganisationNames = new HashSet<OrganisationName>();
            OrganisationScopes = new HashSet<OrganisationScope>();
            OrganisationSicCodes = new HashSet<OrganisationSicCode>();
            OrganisationStatuses = new HashSet<OrganisationStatus>();
            Returns = new HashSet<Return>();
            UserOrganisations = new HashSet<UserOrganisation>();
        }

        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public string CompanyNumber { get; set; }
        [JsonProperty]
        public string OrganisationName { get; set; }
        [JsonProperty]
        public SectorTypes SectorType { get; set; }
        [JsonProperty]
        public OrganisationStatuses Status { get; set; }
        [JsonProperty]
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public string StatusDetails { get; set; }
        [JsonProperty]
        public DateTime Created { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public string EmployerReference { get; set; }
        [JsonProperty]
        public DateTime? DateOfCessation { get; set; }
        [JsonProperty]
        public long? LatestPublicSectorTypeId { get; set; }

        [JsonProperty]
        public DateTime? LastCheckedAgainstCompaniesHouse { get; set; }
        [JsonProperty]
        public bool OptedOutFromCompaniesHouseUpdate { get; set; } = false;

        public virtual OrganisationPublicSectorType LatestPublicSectorType { get; set; }
        public virtual ICollection<OrganisationAddress> OrganisationAddresses { get; set; }
        public virtual ICollection<OrganisationName> OrganisationNames { get; set; }
        public virtual ICollection<OrganisationScope> OrganisationScopes { get; set; }
        public virtual ICollection<OrganisationSicCode> OrganisationSicCodes { get; set; }
        public virtual ICollection<OrganisationStatus> OrganisationStatuses { get; set; }
        public virtual ICollection<Return> Returns { get; set; }
        public virtual ICollection<UserOrganisation> UserOrganisations { get; set; }

    }
}
