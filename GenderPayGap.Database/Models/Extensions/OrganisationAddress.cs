using System;
using System.Collections.Generic;

namespace GenderPayGap.Database
{
    public static class OrganisationAddressExtensions
    {
        public static List<string> GetAddressLines(this OrganisationAddress address)
        {
            var list = new List<string>();
            if (address == null)
            {
                return list;
            }

            if (!string.IsNullOrWhiteSpace(address.Address1))
            {
                list.Add(address.Address1.Trim());
            }

            if (!string.IsNullOrWhiteSpace(address.Address2))
            {
                list.Add(address.Address2.Trim());
            }

            if (!string.IsNullOrWhiteSpace(address.Address3))
            {
                list.Add(address.Address3.Trim());
            }

            if (!string.IsNullOrWhiteSpace(address.TownCity))
            {
                list.Add(address.TownCity.Trim());
            }

            if (!string.IsNullOrWhiteSpace(address.County))
            {
                list.Add(address.County.Trim());
            }

            if (!string.IsNullOrWhiteSpace(address.Country))
            {
                list.Add(address.Country.Trim());
            }
            
            if (!string.IsNullOrWhiteSpace(address.GetPostCodeInAllCaps()))
            {
                list.Add(address.GetPostCodeInAllCaps().Trim());
            }

            if (!string.IsNullOrWhiteSpace(address.PoBox))
            {
                list.Add(address.PoBox.Trim());
            }

            return list;
        }
    }

    [Serializable]
    public partial class OrganisationAddress
    {

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var address = obj as OrganisationAddress;
            if (address == null)
            {
                return false;
            }

            return AddressId == address.AddressId;
        }

        public string GetAddressString(string delimiter = ", ")
        {
            return string.Join(delimiter, this.GetAddressLines());
        }

        public bool AddressMatches(OrganisationAddress other)
        {
            return string.Equals(Address1, other.Address1, StringComparison.Ordinal)
                   && string.Equals(Address2, other.Address2, StringComparison.Ordinal)
                   && string.Equals(Address3, other.Address3, StringComparison.Ordinal)
                   && string.Equals(TownCity, other.TownCity, StringComparison.Ordinal)
                   && string.Equals(County, other.County, StringComparison.Ordinal)
                   && string.Equals(Country, other.Country, StringComparison.Ordinal)
                   && string.Equals(GetPostCodeInAllCaps(), other.GetPostCodeInAllCaps(), StringComparison.Ordinal)
                   && string.Equals(PoBox, other.PoBox, StringComparison.Ordinal);
        }

    }
}
