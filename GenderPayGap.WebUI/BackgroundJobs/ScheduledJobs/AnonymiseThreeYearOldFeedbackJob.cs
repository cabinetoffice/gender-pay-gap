using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class AnonymiseThreeYearOldFeedbackJob
    {

        private readonly IDataRepository dataRepository;

        public AnonymiseThreeYearOldFeedbackJob(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        public void AnonymiseFeedback()
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(AnonymiseFeedback), startTime);

            try
            {
                // TODO: Add clever stuff here

                JobHelpers.LogFunctionEnd(runId, nameof(AnonymiseFeedback), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(AnonymiseFeedback), startTime, ex);

                //Rethrow the error
                throw;
            }
        }

    }
}
