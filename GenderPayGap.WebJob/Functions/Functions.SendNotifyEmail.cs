using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
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
            NotifyEmail notifyEmail;
            try
            {
                notifyEmail = JsonConvert.DeserializeObject<NotifyEmail>(queueMessage.AsString);
            }
            catch (Exception ex)
            {
                CustomLogger.Error("EMAIL FAILURE: Failed to deserialise Notify email from queue", ex);
                throw;
            }

            govNotifyApi.SendEmail(notifyEmail);
            CustomLogger.Information("Successfully received message from queue and passed to GovNotifyAPI", new {notifyEmail});
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
