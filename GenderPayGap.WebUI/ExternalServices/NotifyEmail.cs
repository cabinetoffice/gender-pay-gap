namespace GenderPayGap.WebUI.ExternalServices
{
    public class NotifyEmail
    {

        public string EmailAddress { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, dynamic> Personalisation { get; set; } = null;

    }
}
