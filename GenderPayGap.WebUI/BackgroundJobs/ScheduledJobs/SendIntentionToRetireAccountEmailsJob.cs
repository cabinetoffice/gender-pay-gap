using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class SendIntentionToRetireAccountEmailsJob
    {
        private readonly IDataRepository dataRepository;

        public SendIntentionToRetireAccountEmailsJob(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        public void SendIntentionToRetireAccountEmails()
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(SendIntentionToRetireAccountEmails), startTime);

            try
            {
                // TODO Add clever stuff here

                JobHelpers.LogFunctionEnd(runId, nameof(SendIntentionToRetireAccountEmails), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(SendIntentionToRetireAccountEmails), startTime, ex);

                //Rethrow the error
                throw;
            }
        }
    }
}
