using System;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class RegisterLogModel
    {

        public DateTime StatusDate { get; set; }
        public string Status { get; set; }
        public string ActionBy { get; set; }
        public string Details { get; set; }
        public SectorTypes Sector { get; set; }
        public string Organisation { get; set; }
        public string CompanyNo { get; set; }
        public string Address { get; set; }
        public string SicCodes { get; set; }
        public string UserFirstname { get; set; }
        public string UserLastname { get; set; }
        public string UserJobtitle { get; set; }
        public string UserEmail { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactJobTitle { get; set; }
        public string ContactOrganisation { get; set; }
        public string ContactPhoneNumber { get; set; }

    }
}
