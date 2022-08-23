using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class User
    {

        public User()
        {
            UserOrganisations = new HashSet<UserOrganisation>();
            UserStatuses = new HashSet<UserStatus>();
            ReminderEmails = new HashSet<ReminderEmail>();
        }

        [JsonProperty]
        public long UserId { get; set; }
        [JsonProperty]
        public string JobTitle { get; set; }
        [JsonProperty]
        public string Firstname { get; set; }
        [JsonProperty]
        public string Lastname { get; set; }
        [JsonProperty]
        public string EmailAddressDB { get; set; }
        [JsonProperty]
        public string NewEmailAddressDB { get; set; }
        [JsonProperty]
        public DateTime? NewEmailAddressRequestDate { get; set; }
        [JsonProperty]
        public string ContactJobTitle { get; set; }
        [JsonProperty]
        public string ContactFirstName { get; set; }
        [JsonProperty]
        public string ContactLastName { get; set; }
        [JsonProperty]
        public string ContactOrganisation { get; set; }
        [JsonProperty]
        public string ContactEmailAddressDB { get; set; }
        [JsonProperty]
        public string ContactPhoneNumber { get; set; }
        [JsonProperty]
        public string PasswordHash { get; set; }
        [JsonProperty]
        public string Salt { get; set; }
        [JsonProperty]
        public HashingAlgorithm HashingAlgorithm { get; set; }
        [JsonProperty]
        public string EmailVerifyHash { get; set; }
        [JsonProperty]
        public DateTime? EmailVerifySendDate { get; set; }
        [JsonProperty]
        public DateTime? EmailVerifiedDate { get; set; }
        [JsonProperty]
        public UserStatuses Status { get; set; }

        [JsonProperty]
        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public string StatusDetails { get; set; }
        [JsonProperty]
        public int LoginAttempts { get; set; }
        [JsonProperty]
        public DateTime? LoginDate { get; set; }
        [JsonProperty]
        public string PasswordResetCode { get; set; }
        [JsonProperty]
        public DateTime? ResetSendDate { get; set; }
        [JsonProperty]
        public int ResetAttempts { get; set; }

        /// <summary>
        ///     The last time the user attempted to verify their email address but failed
        /// </summary>
        [JsonProperty]
        public DateTime? VerifyAttemptDate { get; set; }

        /// <summary>
        ///     How many times the user attempted to verify their email address but failed
        /// </summary>
        [JsonProperty]
        public int VerifyAttempts { get; set; }

        [JsonProperty]
        public DateTime Created { get; set; } = VirtualDateTime.Now;
        [JsonProperty]
        public DateTime Modified { get; set; } = VirtualDateTime.Now;

        [JsonProperty]
        public bool SendUpdates { get; set; }
        [JsonProperty]
        public bool AllowContact { get; set; }
        [JsonProperty]
        public DateTime? AcceptedPrivacyStatement { get; set; }

        [JsonProperty]
        public bool HasBeenAnonymised { get; set; }

        public virtual ICollection<UserOrganisation> UserOrganisations { get; set; }
        public virtual ICollection<UserStatus> UserStatuses { get; set; }
        public virtual ICollection<ReminderEmail> ReminderEmails { get; set; }

    }
}
