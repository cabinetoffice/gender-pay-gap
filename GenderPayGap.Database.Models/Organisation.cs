using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{
    public partial class Organisation
    {

        public Organisation()
        {
            OrganisationAddresses = new HashSet<OrganisationAddress>();
            OrganisationNames = new HashSet<OrganisationName>();
            OrganisationReferences = new HashSet<OrganisationReference>();
            OrganisationScopes = new HashSet<OrganisationScope>();
            OrganisationSicCodes = new HashSet<OrganisationSicCode>();
            OrganisationStatuses = new HashSet<OrganisationStatus>();
            Returns = new HashSet<Return>();
            UserOrganisations = new HashSet<UserOrganisation>();
        }

        public long OrganisationId { get; set; }
        public string CompanyNumber { get; set; }
        public string OrganisationName { get; set; }
        public SectorTypes SectorType { get; set; }
        public OrganisationStatuses Status { get; set; }
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        public string StatusDetails { get; set; }
        public DateTime Created { get; set; } = VirtualDateTime.Now;
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        public string EmployerReference { get; set; }
        public DateTime? DateOfCessation { get; set; }
        public long? LatestAddressId { get; set; }
        public long? LatestReturnId { get; set; }
        public long? LatestScopeId { get; set; }
        public long? LatestRegistrationUserId { get; set; }
        public long? LatestRegistrationOrganisationId { get; set; }
        public long? LatestPublicSectorTypeId { get; set; }

        public DateTime? LastCheckedAgainstCompaniesHouse { get; set; }
        public bool OptedOutFromCompaniesHouseUpdate { get; set; } = false;

        public string SecurityCode { get; set; }
        public DateTime? SecurityCodeExpiryDateTime { get; set; }
        public DateTime? SecurityCodeCreatedDateTime { get; set; }

        public virtual OrganisationAddress LatestAddress { get; set; }
        public virtual UserOrganisation LatestRegistration { get; set; }
        public virtual Return LatestReturn { get; set; }
        public virtual OrganisationScope LatestScope { get; set; }
        public virtual OrganisationPublicSectorType LatestPublicSectorType { get; set; }
        public virtual ICollection<OrganisationAddress> OrganisationAddresses { get; set; }
        public virtual ICollection<OrganisationName> OrganisationNames { get; set; }
        public virtual ICollection<OrganisationReference> OrganisationReferences { get; set; }
        public virtual ICollection<OrganisationScope> OrganisationScopes { get; set; }
        public virtual ICollection<OrganisationSicCode> OrganisationSicCodes { get; set; }
        public virtual ICollection<OrganisationStatus> OrganisationStatuses { get; set; }
        public virtual ICollection<Return> Returns { get; set; }
        public virtual ICollection<UserOrganisation> UserOrganisations { get; set; }

    }
}
