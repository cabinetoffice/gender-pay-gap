using System;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
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
            string runId = JobHelpers.CreateRunId();
            DateTime startTime = DateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(SendNotifyEmail), startTime);

            try
            {
                emailSendingService.SendEmailFromQueue(notifyEmail);
            }
            catch (Exception ex)
            {
                CustomLogger.Error("EMAIL FAILURE: Notify email failed to send queued email",
                    new
                    {
                        NotifyEmail = notifyEmail,
                        Error = ex
                    });
                throw;
            }

            DateTime endTime = VirtualDateTime.Now;
            CustomLogger.Information(
                $"Function finished: {nameof(SendNotifyEmail)}. Successfully received message from queue and passed to GovNotifyAPI",
                new {
                    runId, 
                    Environment = Config.EnvironmentName, 
                    endTime, 
                    TimeTakenInSeconds = (endTime - startTime).TotalSeconds,
                    notifyEmail
                });
        }

    }
}
