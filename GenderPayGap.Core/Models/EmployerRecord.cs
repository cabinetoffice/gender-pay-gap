using System;
using System.Collections.Generic;
using System.Linq;
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
        public OrganisationSectors OrganisationSector { get; set; }

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

        //Public Sector
        public string EmailDomains { get; set; }
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

        public List<string> GetAddressList()
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(Address1))
            {
                list.Add(Address1);
            }

            if (!string.IsNullOrWhiteSpace(Address2))
            {
                list.Add(Address2);
            }

            if (!string.IsNullOrWhiteSpace(Address3))
            {
                list.Add(Address3);
            }

            if (!string.IsNullOrWhiteSpace(City))
            {
                list.Add(City);
            }

            if (!string.IsNullOrWhiteSpace(County))
            {
                list.Add(County);
            }

            if (!string.IsNullOrWhiteSpace(Country))
            {
                list.Add(Country);
            }

            if (!string.IsNullOrWhiteSpace(PoBox))
            {
                list.Add(PoBox);
            }

            return list;
        }

        public bool HasAnyAddress()
        {
            return !string.IsNullOrWhiteSpace(Address1)
                   || !string.IsNullOrWhiteSpace(Address2)
                   || !string.IsNullOrWhiteSpace(Address3)
                   || !string.IsNullOrWhiteSpace(City)
                   || !string.IsNullOrWhiteSpace(County)
                   || !string.IsNullOrWhiteSpace(Country)
                   || !string.IsNullOrWhiteSpace(PostCode)
                   || !string.IsNullOrWhiteSpace(PoBox);
        }

        public bool IsValidAddress()
        {
            bool isUK = Country.IsUK();
            if (isUK)
            {
                return !string.IsNullOrWhiteSpace(Address1)
                       || !string.IsNullOrWhiteSpace(Address2)
                       || !string.IsNullOrWhiteSpace(Address3)
                       || !string.IsNullOrWhiteSpace(City)
                       || !string.IsNullOrWhiteSpace(PostCode)
                       || !string.IsNullOrWhiteSpace(PoBox);
            }

            return !string.IsNullOrWhiteSpace(Country)
                   && (!string.IsNullOrWhiteSpace(Address1)
                       || !string.IsNullOrWhiteSpace(Address2)
                       || !string.IsNullOrWhiteSpace(Address3)
                       || !string.IsNullOrWhiteSpace(City)
                       || !string.IsNullOrWhiteSpace(County)
                       || !string.IsNullOrWhiteSpace(PostCode)
                       || !string.IsNullOrWhiteSpace(PoBox));
        }

        public SortedSet<int> GetSicCodes()
        {
            var codes = new SortedSet<int>();
            foreach (string sicCode in SicCodeIds.SplitI())
            {
                codes.Add(sicCode.ToInt32());
            }

            return codes;
        }

        public AddressModel GetAddressModel()
        {
            return new AddressModel {
                Address1 = Address1,
                Address2 = Address2,
                Address3 = Address3,
                City = City,
                County = County,
                Country = Country,
                PostCode = PostCode,
                PoBox = PoBox
            };
        }

        public bool IsAuthorised(string emailAddress)
        {
            if (!emailAddress.IsEmailAddress())
            {
                throw new ArgumentException("Bad email address");
            }

            if (string.IsNullOrWhiteSpace(EmailDomains))
            {
                return false;
            }

            List<string> emailDomains = EmailDomains.SplitI(";")
                .Select(ep => ep.ContainsI("*@") ? ep : ep.Contains('@') ? "*" + ep : "*@" + ep)
                .ToList();
            return emailDomains.Count > 0 && emailAddress.LikeAny(emailDomains);
        }

    }
}
