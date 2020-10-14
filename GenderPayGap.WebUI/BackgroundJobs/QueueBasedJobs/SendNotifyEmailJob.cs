using System;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.Services;

namespace GenderPayGap.WebUI.BackgroundJobs.QueueBasedJobs
{
    public class SendNotifyEmailJob
    {
        private readonly EmailSendingService emailSendingService;

        public SendNotifyEmailJob(EmailSendingService emailSendingService)
        {
            this.emailSendingService = emailSendingService;
        }


        public void SendNotifyEmail(NotifyEmail notifyEmail)
        {
            JobHelpers.RunAndLogJob(() => SendNotifyEmailAction(notifyEmail), nameof(SendNotifyEmail));
        }

        private void SendNotifyEmailAction(NotifyEmail notifyEmail)
        {
            try
            {
                emailSendingService.SendEmailFromQueue(notifyEmail);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(
                    "EMAIL FAILURE: Notify email failed to send queued email",
                    new {NotifyEmail = notifyEmail, Error = ex});
                throw;
            }
        }

    }
}
