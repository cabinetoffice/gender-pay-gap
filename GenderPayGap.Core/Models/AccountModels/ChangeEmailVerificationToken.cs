using System;

namespace GenderPayGap.Core.Models
{

    public class ChangeEmailVerificationToken
    {

        public long UserId { get; set; }

        public string NewEmailAddress { get; set; }

        public DateTime TokenTimestamp { get; set; }

    }

}
