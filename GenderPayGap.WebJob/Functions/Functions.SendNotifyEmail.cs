using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        /// <summary>
        ///     Handling healthy queued Notify email messages. After 5 failed attempts message is added to poisoned queue.
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        public async Task SendNotifyEmail([QueueTrigger(QueueNames.SendNotifyEmail)]
            CloudQueueMessage queueMessage,
            ILogger log)
        {
            string runId = JobHelpers.CreateRunId();
            DateTime startTime = DateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(SendNotifyEmail), startTime);

            NotifyEmail notifyEmail;
            try
            {
                notifyEmail = JsonConvert.DeserializeObject<NotifyEmail>(queueMessage.AsString);
            }
            catch (Exception ex)
            {
                DateTime errorEndTime = VirtualDateTime.Now;
                CustomLogger.Error(
                    $"Function failed: {nameof(SendNotifyEmail)}. Failed to deserialise Notify email from queue",
                    new
                    {
                        runId,
                        environment = Config.EnvironmentName,
                        endTime = errorEndTime,
                        TimeTakenToErrorInSeconds = (errorEndTime - startTime).TotalSeconds,
                        Exception = ex
                    });
                throw;
            }

            emailSendingService.SendEmailFromQueue(notifyEmail);

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

        /// <summary>
        ///     Handling poison-queued Notify email message.
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        public async Task SendNotifyEmailPoisonAsync([QueueTrigger(QueueNames.SendNotifyEmail + "-poison")]
            string queueMessage)
        {
            CustomLogger.Error("EMAIL FAILURE: Notify email in poison queue", new {queueMessage});
        }

    }
}
