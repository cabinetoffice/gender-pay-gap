namespace GenderPayGap.Core.Models
{
    public class SendRegistrationDeclinedModel
    {

        public string returnUrl { get; set; }
        public string emailAddress { get; set; }
        public string reason { get; set; }
        public bool test { get; set; }

    }
}
