using System;
using System.Collections.Generic;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class EmployerRecord
    {

        public Dictionary<string, string> References = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public long OrganisationId { get; set; }
        public string EmployerReference { get; set; }
        public string CompanyNumber { get; set; }
        public DateTime? DateOfCessation { get; set; }

        public string OrganisationName { get; set; }
        public SectorTypes SectorType { get; set; }

        public string NameSource { get; set; }
        public long ActiveAddressId { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string PostCode { get; set; }
        public string PoBox { get; set; }
        public string AddressSource { get; set; }

        public bool? IsUkAddress { get; set; }

        public string SicCodeIds { get; set; }
        public string SicSource { get; set; }

        public string RegistrationStatus { get; set; }

        public string SicSectors { get; set; }


        public string GetFullAddress()
        {
            var list = new List<string>();
            list.Add(Address1);
            list.Add(Address2);
            list.Add(Address3);
            list.Add(City);
            list.Add(County);
            list.Add(Country);
            list.Add(PostCode);
            list.Add(PoBox);
            return list.ToDelimitedString(", ");
        }

    }
}
