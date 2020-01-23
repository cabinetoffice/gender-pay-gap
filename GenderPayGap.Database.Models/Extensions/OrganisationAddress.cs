using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{
    [Serializable]
    public partial class OrganisationAddress
    {

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            var addressModel = obj as AddressModel;
            if (addressModel != null)
            {
                return GetAddressModel().Equals(addressModel);
            }

            var address = obj as OrganisationAddress;
            if (address == null)
            {
                return false;
            }

            return AddressId == address.AddressId;
        }

        #region Methods

        public List<string> GetList()
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(Address1))
            {
                list.Add(Address1.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(Address2))
            {
                list.Add(Address2.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(Address3))
            {
                list.Add(Address3.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(TownCity))
            {
                list.Add(TownCity.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(County))
            {
                list.Add(County.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(Country))
            {
                list.Add(Country.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(PostCode))
            {
                list.Add(PostCode.TrimI());
            }

            if (!string.IsNullOrWhiteSpace(PoBox))
            {
                list.Add(PoBox.TrimI());
            }

            return list;
        }

        public bool EqualsI(OrganisationAddress address)
        {
            string add1 = GetAddressString();
            string add2 = address == null ? null : address.GetAddressString();
            return add1.EqualsI(add2);
        }

        public string GetAddressString(string delimiter = ", ")
        {
            return GetList().ToDelimitedString(delimiter);
        }

        public AddressModel GetAddressModel()
        {
            return new AddressModel {
                Address1 = Address1,
                Address2 = Address2,
                Address3 = Address3,
                City = TownCity,
                County = County,
                Country = Country,
                PostCode = PostCode,
                PoBox = PoBox
            };
        }

        public UserOrganisation GetFirstRegistration()
        {
            return UserOrganisations.OrderBy(uo => uo.PINConfirmedDate).FirstOrDefault(uo => uo.PINConfirmedDate > Created);
        }

        public DateTime GetFirstRegisteredDate()
        {
            UserOrganisation firstRegistration = GetFirstRegistration();
            return firstRegistration?.PINConfirmedDate ?? Created;
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

            AddressStatuses.Add(
                new AddressStatus {
                    AddressId = AddressId,
                    Status = status,
                    StatusDate = statusDate.Value,
                    StatusDetails = details,
                    ByUserId = byUserId
                });
            Status = status;
            StatusDate = statusDate.Value;
            StatusDetails = details;
        }

        #endregion

        public bool AddressMatches(OrganisationAddress other)
        {
            return String.Equals(Address1, other.Address1, StringComparison.Ordinal)
                   && String.Equals(Address2, other.Address2, StringComparison.Ordinal)
                   && String.Equals(Address3, other.Address3, StringComparison.Ordinal)
                   && String.Equals(TownCity, other.TownCity, StringComparison.Ordinal)
                   && String.Equals(County, other.County, StringComparison.Ordinal)
                   && String.Equals(Country, other.Country, StringComparison.Ordinal)
                   && String.Equals(PostCode, other.PostCode, StringComparison.Ordinal)
                   && String.Equals(PoBox, other.PoBox, StringComparison.Ordinal);
        }

    }
}
