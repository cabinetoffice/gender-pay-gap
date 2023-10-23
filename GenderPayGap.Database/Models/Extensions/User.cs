using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using GenderPayGap.Core;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{

    [Serializable]
    [DebuggerDisplay("{UserId}, {EmailAddress}, {Status}")]
    public partial class User
    {

        [NotMapped]
        public string EmailAddress
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EmailAddressDB))
                {
                    try
                    {
                        return Encryption.DecryptData(EmailAddressDB);
                    }
                    catch (CryptographicException) { }
                }

                return EmailAddressDB;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    EmailAddressDB = Encryption.EncryptData(value.ToLower());
                }
                else
                {
                    EmailAddressDB = value;
                }
            }
        }
        
        [NotMapped]
        public string NewEmailAddress
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(NewEmailAddressDB))
                {
                    try
                    {
                        return Encryption.DecryptData(NewEmailAddressDB);
                    }
                    catch (CryptographicException) { }
                }

                return NewEmailAddressDB;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    NewEmailAddressDB = Encryption.EncryptData(value.ToLower());
                }
                else
                {
                    NewEmailAddressDB = value;
                }
            }
        }

        [NotMapped]
        public string Fullname => (Firstname + " " + Lastname).TrimI();

        public bool IsFullAdministrator()
        {
            return UserRole == UserRole.Admin;
        }

        public bool IsReadOnlyAdministrator()
        {
            return UserRole == UserRole.AdminReadOnly;
        }

        public bool IsFullOrReadOnlyAdministrator()
        {
            return UserRole == UserRole.Admin || UserRole == UserRole.AdminReadOnly;
        }

        /// <summary>
        ///     Determines if the user is the only registration of any of their UserOrganisations
        /// </summary>
        public bool IsSoleUserOfOneOrMoreOrganisations()
        {
            return UserOrganisations.Any(uo => uo.GetAssociatedUsers().Any() == false);
        }

        public void SetStatus(UserStatuses status, User byUser, string details = null)
        {
            //ByUser must be an object and not the id itself otherwise a foreign key exception is thrown with EF core due to being unable to resolve the ByUserId
            if (status == Status && details == StatusDetails)
            {
                return;
            }

            UserStatuses.Add(
                new UserStatus {
                    User = this,
                    Status = status,
                    StatusDate = VirtualDateTime.Now,
                    StatusDetails = details,
                    ByUser = byUser
                });
            Status = status;
            StatusDate = VirtualDateTime.Now;
            StatusDetails = details;
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var target = (User) obj;
            return UserId == target.UserId;
        }

    }

}
