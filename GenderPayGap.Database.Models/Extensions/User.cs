using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.Database
{

    [Serializable]
    [DebuggerDisplay("{UserId}, {EmailAddress}, {Status}")]
    public partial class User
    {

        [NotMapped]
        public static bool EncryptEmails = Config.GetAppSetting("EncryptEmails").ToBoolean(true);

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
                if (!string.IsNullOrWhiteSpace(value) && EncryptEmails)
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
        public string ContactEmailAddress
        {
            get => string.IsNullOrWhiteSpace(ContactEmailAddressDB) ? ContactEmailAddressDB : Encryption.DecryptData(ContactEmailAddressDB);
            set
            {
                if (!string.IsNullOrWhiteSpace(value) && EncryptEmails)
                {
                    ContactEmailAddressDB = Encryption.EncryptData(value);
                }
                else
                {
                    ContactEmailAddressDB = value;
                }
            }
        }

        [NotMapped]
        public string Fullname => (Firstname + " " + Lastname).TrimI();

        [NotMapped]
        public string ContactFullname => (ContactFirstName + " " + ContactLastName).TrimI();

        [NotMapped]
        public TimeSpan LockRemaining =>
            LoginDate == null || LoginAttempts < Config.Configuration["MaxLoginAttempts"].ToInt32()
                ? TimeSpan.Zero
                : LoginDate.Value.AddMinutes(Config.Configuration["LockoutMinutes"].ToInt32()) - VirtualDateTime.Now;

        [NotMapped]
        public bool SendUpdates
        {
            get => GetSetting(UserSettingKeys.SendUpdates) == "True";
            set => SetSetting(UserSettingKeys.SendUpdates, value.ToString());
        }

        [NotMapped]
        public bool AllowContact
        {
            get => GetSetting(UserSettingKeys.AllowContact) == "True";
            set => SetSetting(UserSettingKeys.AllowContact, value.ToString());
        }

        [NotMapped]
        public DateTime? AcceptedPrivacyStatement
        {
            get
            {
                string value = GetSetting(UserSettingKeys.AcceptedPrivacyStatement);
                if (value == null)
                {
                    return null;
                }

                return DateTime.Parse(value);
            }
            set => SetSetting(UserSettingKeys.AcceptedPrivacyStatement, value.HasValue ? value.Value.ToString() : null);
        }

        public bool IsAdministrator()
        {
            if (!EmailAddress.IsEmailAddress())
            {
                throw new ArgumentException("Bad email address");
            }

            if (string.IsNullOrWhiteSpace(Global.AdminEmails))
            {
                throw new ArgumentException("Missing AdminEmails from web.config");
            }

            return EmailAddress.LikeAny(Global.AdminEmails.SplitI(";"));
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

        public string GetSetting(UserSettingKeys key)
        {
            UserSetting setting = UserSettings.FirstOrDefault(s => s.Key == key);

            if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
            {
                return setting.Value;
            }

            return null;
        }

        public void SetSetting(UserSettingKeys key, string value)
        {
            UserSetting setting = UserSettings.FirstOrDefault(s => s.Key == key);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (setting != null)
                {
                    UserSettings.Remove(setting);
                }
            }
            else if (setting == null)
            {
                UserSettings.Add(new UserSetting(key, value));
            }
            else if (setting.Value != value)
            {
                setting.Value = value;
                setting.Modified = VirtualDateTime.Now;
            }
        }

        public string GetVerifyUrl()
        {
            if (EmailVerifiedDate != null)
            {
                return null;
            }

            string verifyCode = Encryption.EncryptQuerystring(UserId + ":" + Created.ToSmallDateTime());
            string verifyUrl = $"/register/verify-email?code={verifyCode}";
            return verifyUrl;
        }

        public string GetPasswordResetUrl()
        {
            string resetCode = Encryption.EncryptQuerystring(UserId + ":" + VirtualDateTime.Now.ToSmallDateTime());
            string resetUrl = $"/register/enter-new-password?code={resetCode}";
            return resetUrl;
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
