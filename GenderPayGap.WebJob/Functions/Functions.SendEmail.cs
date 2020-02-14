using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Abstractions;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        /// <summary>
        ///     Handling healthy queued Stannp messages. After 5 failed attempts message is added to poisoned queue.
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        public async Task SendEmail([QueueTrigger(QueueNames.SendEmail)] string queueMessage, ILogger log)
        {
            string runId = CreateRunId();
            DateTime startTime = VirtualDateTime.Now;
            LogFunctionStart(runId, nameof(SendEmail), startTime);

            var wrapper = JsonConvert.DeserializeObject<QueueWrapper>(queueMessage);
            wrapper.Message = Regex.Unescape(wrapper.Message).TrimI("\"");
            Type messageType = typeof(SendGeoMessageModel).Assembly.GetType(wrapper.Type, true);
            object parameters = JsonConvert.DeserializeObject(wrapper.Message, messageType);

            if (parameters is SendGeoMessageModel)
            {
                var pars = (SendGeoMessageModel) parameters;
                if (!await _Messenger.SendGeoMessageAsync(pars.subject, pars.message, pars.test))
                {
                    DateTime errorEndTime = VirtualDateTime.Now;
                    CustomLogger.Error(
                        $"Function failed: {nameof(SendEmail)}. Could not send email message to GEO for queued message: {queueMessage}",
                        new
                        {
                            runId,
                            environment = Config.EnvironmentName,
                            errorEndTime,
                            TimeTakenToErrorInSeconds = (errorEndTime - startTime).TotalSeconds
                        });

                    throw new Exception("Could not send email message to GEO for queued message:" + queueMessage);
                }
            }
            else
            {
                try
                {
                    await _Messenger.SendEmailTemplateAsync((AEmailTemplate) parameters);
                }
                catch
                {
                    DateTime errorEndTime = VirtualDateTime.Now;
                    CustomLogger.Error(
                        $"Function failed: {nameof(SendEmail)}. Could not send email for unknown type '{wrapper.Type}'. Queued message: {queueMessage}",
                        new
                        {
                            runId,
                            environment = Config.EnvironmentName,
                            errorEndTime,
                            TimeTakenToErrorInSeconds = (errorEndTime - startTime).TotalSeconds
                        });

                    throw new Exception($"Could not send email for unknown type '{wrapper.Type}'. Queued message:" + queueMessage);
                }
            }

            DateTime endTime = VirtualDateTime.Now;
            CustomLogger.Information(
                $"Function finished: {nameof(SendEmail)}. {wrapper.Type} successfully",
                new {runId, Environment = Config.EnvironmentName, endTime, TimeTakenInSeconds = (endTime - startTime).TotalSeconds});
        }

        /// <summary>
        ///     Handling all email message sends
        /// </summary>
        /// <param name="queueMessage"></param>
        /// <param name="log"></param>
        [Singleton] //Ensures execution on only one instance
        public async Task SendEmailPoisonAsync([QueueTrigger(QueueNames.SendEmail + "-poison")]
            string queueMessage,
            ILogger log)
        {
            log.LogError($"Could not send email for queued message, Details:{queueMessage}");

            //Send Email to GEO reporting errors
            await _Messenger.SendGeoMessageAsync("GPG - GOV WEBJOBS ERROR", "Could not send email for queued message:" + queueMessage);
        }

    }
}
