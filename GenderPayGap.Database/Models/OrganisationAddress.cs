using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class OrganisationAddress
    {

        public OrganisationAddress()
        {
            UserOrganisations = new HashSet<UserOrganisation>();
        }

        [JsonProperty]
        public long AddressId { get; set; }
        [JsonProperty]
        public long CreatedByUserId { get; set; }
        [JsonProperty]
        public string Address1 { get; set; }
        [JsonProperty]
        public string Address2 { get; set; }
        [JsonProperty]
        public string Address3 { get; set; }
        [JsonProperty]
        public string TownCity { get; set; }
        [JsonProperty]
        public string County { get; set; }
        [JsonProperty]
        public string Country { get; set; }
        [JsonProperty]
        public string PoBox { get; set; }
        [JsonProperty]
        public string PostCode { get; set; }
        [JsonProperty]
        public AddressStatuses Status { get; set; }
        [JsonProperty]
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public string StatusDetails { get; set; }
        [JsonProperty]
        public DateTime Created { get; set; }

        [JsonProperty]
        public long OrganisationId { get; set; }
        [JsonProperty]
        public string Source { get; set; }

        [JsonProperty]
        public bool? IsUkAddress { get; set; }

        public virtual Organisation Organisation { get; set; }

        public virtual ICollection<UserOrganisation> UserOrganisations { get; set; }

    }
}
