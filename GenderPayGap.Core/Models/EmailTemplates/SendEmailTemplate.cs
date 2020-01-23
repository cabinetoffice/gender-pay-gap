using GenderPayGap.Core.Abstractions;

namespace GenderPayGap.Core.Models
{

    public class SendEmailTemplate : AEmailTemplate
    {

        public string Subject { get; set; }

        public string MessageBody { get; set; }

    }

}
