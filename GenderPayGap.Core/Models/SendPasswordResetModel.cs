namespace GenderPayGap.Core.Models
{
    public class SendPasswordResetModel
    {

        public string resetUrl { get; set; }
        public string emailAddress { get; set; }
        public string resetCode { get; set; }
        public bool test { get; set; }

    }
}
