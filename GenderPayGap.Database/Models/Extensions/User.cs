using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
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
            get => DecryptEmailAddress(EmailAddressDB);
            set => EmailAddressDB = EncryptEmailAddress(value);
        }

        [NotMapped]
        public string NewEmailAddress
        {
            get => DecryptEmailAddress(NewEmailAddressDB);
            set => NewEmailAddressDB = EncryptEmailAddress(value);
        }

        public static string DecryptEmailAddress(string emailAddressDb)
        {
            if (!string.IsNullOrWhiteSpace(emailAddressDb))
            {
                try
                {
                    return Encryption.DecryptData(emailAddressDb);
                }
                catch (CryptographicException) { }
            }

            return emailAddressDb;
        }

        public static string EncryptEmailAddress(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return Encryption.EncryptData(value.ToLower());
            }

            return value;
        }

        [NotMapped]
        public string Fullname => (Firstname + " " + Lastname).Trim();

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
