using System;
using GenderPayGap.Core;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{
    public partial class UserOrganisation
    {

        public long UserId { get; set; }
        public long OrganisationId { get; set; }
        public string PIN { get; set; }
        public DateTime? PINSentDate { get; set; }
        public string PITPNotifyLetterId { get; set; }
        public DateTime? PINConfirmedDate { get; set; }
        public DateTime? ConfirmAttemptDate { get; set; }
        public int ConfirmAttempts { get; set; }
        public DateTime Created { get; set; } = VirtualDateTime.Now;
        public DateTime Modified { get; set; } = VirtualDateTime.Now;
        public long? AddressId { get; set; }
        public RegistrationMethods Method { get; set; } = RegistrationMethods.Unknown;

        public virtual Organisation Organisation { get; set; }

        public virtual OrganisationAddress Address { get; set; }

        public virtual User User { get; set; }

    }
}
