using System.Collections.Generic;
using GenderPayGap.Core.Classes;
using Notify.Models.Responses;

namespace GenderPayGap.Core.Interfaces
{
    public interface IGovNotifyAPI
    {

        EmailNotificationResponse SendEmail(NotifyEmail notifyEmail);

        LetterNotificationResponse SendLetter(string templateId,
            Dictionary<string, dynamic> personalisation,
            string clientReference = null);

    }
}
