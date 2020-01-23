using System.Collections.Generic;

namespace GenderPayGap.Core.Classes
{
    public class NotifyEmail
    {

        public string EmailAddress { get; set; }
        public string TemplateId { get; set; }
        public Dictionary<string, dynamic> Personalisation { get; set; } = null;

    }
}
