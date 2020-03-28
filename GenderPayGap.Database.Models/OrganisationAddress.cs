using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{
    public partial class OrganisationAddress
    {

        public OrganisationAddress()
        {
            AddressStatuses = new HashSet<AddressStatus>();
            Organisations = new HashSet<Organisation>();
            UserOrganisations = new HashSet<UserOrganisation>();
            InactiveUserOrganisations = new HashSet<InactiveUserOrganisation>();
        }

        public long AddressId { get; set; }
        public long CreatedByUserId { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string TownCity { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string PoBox { get; set; }
        public string PostCode { get; set; }
        public AddressStatuses Status { get; set; }
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        public string StatusDetails { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        public long OrganisationId { get; set; }
        public string Source { get; set; }

        public bool? IsUkAddress { get; set; }

        public virtual Organisation Organisation { get; set; }
        public virtual ICollection<AddressStatus> AddressStatuses { get; set; }
        public virtual ICollection<Organisation> Organisations { get; set; }
        public virtual ICollection<UserOrganisation> UserOrganisations { get; set; }
        public virtual ICollection<InactiveUserOrganisation> InactiveUserOrganisations { get; set; }

    }
}
