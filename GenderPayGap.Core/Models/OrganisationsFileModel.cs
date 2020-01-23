using System;

namespace GenderPayGap.Core.Models
{
    public class OrganisationsFileModel
    {

        public long OrganisationId { get; set; }
        public string DUNSNumber { get; set; }
        public string EmployerReference { get; set; }
        public string OrganisationName { get; set; }
        public string CompanyNo { get; set; }
        public SectorTypes Sector { get; set; }
        public OrganisationStatuses Status { get; set; }
        public DateTime StatusDate { get; set; }
        public string StatusDetails { get; set; }
        public string Address { get; set; }
        public string SicCodes { get; set; }
        public DateTime? LatestRegistrationDate { get; set; }
        public RegistrationMethods? LatestRegistrationMethod { get; set; }
        public DateTime? LatestReturn { get; set; }
        public ScopeStatuses? ScopeStatus { get; set; }
        public DateTime? ScopeDate { get; set; }
        public DateTime Created { get; set; }

        #region SecurityCode information

        public string SecurityCode { get; set; }
        public DateTime? SecurityCodeExpiryDateTime { get; set; }
        public DateTime? SecurityCodeCreatedDateTime { get; set; }

        #endregion

    }
}
