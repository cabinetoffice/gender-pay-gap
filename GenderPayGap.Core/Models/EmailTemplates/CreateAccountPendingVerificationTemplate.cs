using GenderPayGap.Core.Abstractions;

namespace GenderPayGap.Core.Models
{

    public class CreateAccountPendingVerificationTemplate : AEmailTemplate
    {

        public string Url { get; set; }

    }

}
