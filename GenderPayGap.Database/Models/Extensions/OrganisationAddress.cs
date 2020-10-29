using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;

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
                list.Add(address.Address1.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(address.Address2))
            {
                list.Add(address.Address2.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(address.Address3))
            {
                list.Add(address.Address3.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(address.TownCity))
            {
                list.Add(address.TownCity.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(address.County))
            {
                list.Add(address.County.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(address.Country))
            {
                list.Add(address.Country.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(address.PostCode))
            {
                list.Add(address.PostCode.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(address.PoBox))
            {
                list.Add(address.PoBox.TrimI());
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
            return this.GetAddressLines().ToDelimitedString(delimiter);
        }

        public void SetStatus(AddressStatuses status, long byUserId, string details = null, DateTime? statusDate = null)
        {
            if (status == Status && details == StatusDetails && statusDate == null)
            {
                return;
            }

            if (statusDate == null || statusDate == DateTime.MinValue)
            {
                statusDate = VirtualDateTime.Now;
            }

            Status = status;
            StatusDate = statusDate.Value;
            StatusDetails = details;
        }

        public bool AddressMatches(OrganisationAddress other)
        {
            return string.Equals(Address1, other.Address1, StringComparison.Ordinal)
                   && string.Equals(Address2, other.Address2, StringComparison.Ordinal)
                   && string.Equals(Address3, other.Address3, StringComparison.Ordinal)
                   && string.Equals(TownCity, other.TownCity, StringComparison.Ordinal)
                   && string.Equals(County, other.County, StringComparison.Ordinal)
                   && string.Equals(Country, other.Country, StringComparison.Ordinal)
                   && string.Equals(PostCode, other.PostCode, StringComparison.Ordinal)
                   && string.Equals(PoBox, other.PoBox, StringComparison.Ordinal);
        }

    }
}
