using GenderPayGap.WebUI.ExternalServices;
using Notify.Models.Responses;

public class MockGovNotify : IGovNotifyAPI
{

    public EmailNotificationResponse SendEmail(NotifyEmail notifyEmail)
    {
        return new EmailNotificationResponse {id = "MOCK_RESPONSE_ID"};
    }


    public LetterNotificationResponse SendLetter(string templateId,
        Dictionary<string, dynamic> personalisation,
        string clientReference = null)
    {
        return new LetterNotificationResponse {id = "MOCK_RESPONSE_ID"};
    }

}
