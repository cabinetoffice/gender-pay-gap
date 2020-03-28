using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{
    public partial class User
    {

        public User()
        {
            AddressStatus = new HashSet<AddressStatus>();
            OrganisationStatus = new HashSet<OrganisationStatus>();
            ReturnStatus = new HashSet<ReturnStatus>();
            UserOrganisations = new HashSet<UserOrganisation>();
            InactiveUserOrganisations = new HashSet<InactiveUserOrganisation>();
            UserSettings = new HashSet<UserSetting>();
            UserStatusesByUser = new HashSet<UserStatus>();
            UserStatuses = new HashSet<UserStatus>();
            ReminderEmails = new HashSet<ReminderEmail>();
        }

        public long UserId { get; set; }
        public string JobTitle { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddressDB { get; set; }
        public string ContactJobTitle { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactOrganisation { get; set; }
        public string ContactEmailAddressDB { get; set; }
        public string ContactPhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public HashingAlgorithm HashingAlgorithm { get; set; }
        public string EmailVerifyHash { get; set; }
        public DateTime? EmailVerifySendDate { get; set; }
        public DateTime? EmailVerifiedDate { get; set; }
        public UserStatuses Status { get; set; }

        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        public string StatusDetails { get; set; }
        public int LoginAttempts { get; set; }
        public DateTime? LoginDate { get; set; }
        public DateTime? ResetSendDate { get; set; }
        public int ResetAttempts { get; set; }

        /// <summary>
        ///     The last time the user attempted to verify their email address but failed
        /// </summary>
        public DateTime? VerifyAttemptDate { get; set; }

        /// <summary>
        ///     How many times the user attempted to verify their email address but failed
        /// </summary>
        public int VerifyAttempts { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;
        public DateTime Modified { get; set; } = VirtualDateTime.Now;

        public virtual ICollection<AddressStatus> AddressStatus { get; set; }
        public virtual ICollection<OrganisationStatus> OrganisationStatus { get; set; }
        public virtual ICollection<ReturnStatus> ReturnStatus { get; set; }
        public virtual ICollection<UserOrganisation> UserOrganisations { get; set; }
        public virtual ICollection<InactiveUserOrganisation> InactiveUserOrganisations { get; set; }
        public virtual ICollection<UserSetting> UserSettings { get; set; }
        public virtual ICollection<UserStatus> UserStatusesByUser { get; set; }
        public virtual ICollection<UserStatus> UserStatuses { get; set; }
        public virtual ICollection<ReminderEmail> ReminderEmails { get; set; }

    }
}
