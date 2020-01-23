using System;
using System.Collections.Generic;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class DnBOrgsModel
    {

        public string DUNSNumber { get; set; }
        public string EmployerReference { get; set; }

        public SectorTypes SectorType { get; set; }

        public string CompanyNumber { get; set; }

        public string ContactPrefix { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }

        public string ContactJobtitle { get; set; }
        public string OrganisationName { get; set; }
        public string NameSource { get; set; }
        public DateTime? NameChanged { get; set; }

        public string OldName { get; set; }
        public string OldNameSource { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string County { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string PoBox { get; set; }
        public string AddressSource { get; set; }
        public DateTime? AddressChanged { get; set; }

        public string OldAddress { get; set; }
        public string OldAddressSource { get; set; }

        public string NationalRegion { get; set; }
        public string SicCode { get; set; }
        public string SicSource { get; set; }
        public DateTime? SicChanged { get; set; }
        public string OldSicCode { get; set; }
        public string OldSicSource { get; set; }

        public string SicDescription { get; set; }
        public string SicSector { get; set; }
        public string URL { get; set; }
        public DateTime? DateOfCessation { get; set; }
        public string CompanyStatus { get; set; }
        public DateTime? StatusCheckedDate { get; set; }
        public DateTime? ImportedDate { get; set; }
        public string EmailDomains { get; set; }
        public string PublicSectorDecription { get; set; }
        public long? OrganisationId { get; set; }

        public string GetContactName()
        {
            return new[] {ContactPrefix, ContactFirstName, ContactLastName}.ToDelimitedString(" ");
        }

        //True is org dissolved before current accounting date
        public bool GetIsDissolved()
        {
            return DateOfCessation != null && DateOfCessation < SectorType.GetAccountingStartDate();
        }

        public SortedSet<int> GetSicCodesIds()
        {
            var codes = new SortedSet<int>();
            foreach (string sicCode in SicCode.SplitI())
            {
                codes.Add(sicCode.ToInt32());
            }

            return codes;
        }

        public List<string> GetList()
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(AddressLine1))
            {
                list.Add(AddressLine1.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(AddressLine2))
            {
                list.Add(AddressLine2.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(AddressLine3))
            {
                list.Add(AddressLine3.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(City))
            {
                list.Add(City.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(County))
            {
                list.Add(County.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(Country))
            {
                list.Add(Country.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(PostalCode))
            {
                list.Add(PostalCode.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(PoBox))
            {
                list.Add(PoBox.TrimI());
            }

            return list;
        }

        public string GetAddress(string delimiter = ", ")
        {
            return GetList().ToDelimitedString(delimiter);
        }

        public bool IsValidAddress()
        {
            bool isUK = Country.IsUK();
            if (isUK)
            {
                return !string.IsNullOrWhiteSpace(AddressLine1)
                       || !string.IsNullOrWhiteSpace(AddressLine2)
                       || !string.IsNullOrWhiteSpace(AddressLine3)
                       || !string.IsNullOrWhiteSpace(City)
                       || !string.IsNullOrWhiteSpace(PostalCode)
                       || !string.IsNullOrWhiteSpace(PoBox);
            }

            return !string.IsNullOrWhiteSpace(Country)
                   && (!string.IsNullOrWhiteSpace(AddressLine1)
                       || !string.IsNullOrWhiteSpace(AddressLine2)
                       || !string.IsNullOrWhiteSpace(AddressLine3)
                       || !string.IsNullOrWhiteSpace(City)
                       || !string.IsNullOrWhiteSpace(County)
                       || !string.IsNullOrWhiteSpace(PostalCode)
                       || !string.IsNullOrWhiteSpace(PoBox));
        }

    }
}
