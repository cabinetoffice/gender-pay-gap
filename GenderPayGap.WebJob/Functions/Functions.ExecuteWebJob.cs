using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        [Singleton(Mode = SingletonMode.Listener)] //Ensures execution on only one instance with one listener
        public async Task ExecuteWebjob([QueueTrigger(QueueNames.ExecuteWebJob)]
            string queueMessage,
            ILogger log)
        {
            string runId = CreateRunId();
            DateTime startTime = VirtualDateTime.Now;
            LogFunctionStart(runId, nameof(ExecuteWebjob), startTime);

            var wrapper = JsonConvert.DeserializeObject<QueueWrapper>(queueMessage);
            wrapper.Message = JsonConvert.DeserializeObject<string>(wrapper.Message);
            wrapper.Message = Regex.Unescape(wrapper.Message).TrimI("\"");

            NameValueCollection parameters = wrapper.Message.FromQueryString();
            string command = parameters["command"];
            if (string.IsNullOrWhiteSpace(command))
            {
                command = wrapper.Message;
            }

            switch (command)
            {
                case "UpdateFile":
                    await UpdateFileAsync(log, parameters["filePath"], parameters["action"]);
                    break;
                case "UpdateSearch":
                    await UpdateSearchAsync(log, parameters["userEmail"], true);
                    break;
                case "UpdateDownloadFiles":
                    await UpdateDownloadFilesAsync(log, parameters["userEmail"], true);
                    break;
                default:
                    DateTime errorEndTime = VirtualDateTime.Now;
                    CustomLogger.Error(
                        $"Function failed: {nameof(ExecuteWebjob)}",
                        new
                        {
                            runId,
                            environment = Config.EnvironmentName,
                            errorEndTime,
                            TimeTakenToErrorInSeconds = (errorEndTime - startTime).TotalSeconds,
                            queueMessage
                        });
                    throw new Exception("Could not execute webjob:" + queueMessage);
            }

            DateTime endTime = VirtualDateTime.Now;
            CustomLogger.Information(
                $"Function finished: {nameof(ExecuteWebjob)}. {command} successfully",
                new
                {
                    runId, 
                    Environment = Config.EnvironmentName, 
                    endTime, 
                    TimeTakenInSeconds = (endTime - startTime).TotalSeconds
                });
        }

        public async Task ExecuteWebjobPoisonAsync([QueueTrigger(QueueNames.ExecuteWebJob + "-poison")]
            string queueMessage,
            ILogger log)
        {
            log.LogError($"Could not execute Webjob, Details: {queueMessage}");

            //Send Email to GEO reporting errors
            await _Messenger.SendGeoMessageAsync("GPG - GOV WEBJOBS ERROR", "Could not execute Webjob:" + queueMessage);
        }

    }
}
