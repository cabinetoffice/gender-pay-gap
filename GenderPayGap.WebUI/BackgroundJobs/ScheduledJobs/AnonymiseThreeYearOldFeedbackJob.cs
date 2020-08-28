using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database.Models;
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

        public void AnonymiseFeedbackAction()
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(AnonymiseFeedbackAction), startTime);

            try
            {
                DateTime threeYearsAgo = VirtualDateTime.Now.AddYears(-3);

                List<Feedback> feedback = dataRepository.GetAll<Feedback>()
                    .Where(f => DateTime.Compare(f.CreatedDate, threeYearsAgo) <= 0)
                    .ToList();

                foreach (Feedback feedbackItem in feedback)
                {
                    AnonymiseFeedback(feedbackItem);
                }

                JobHelpers.LogFunctionEnd(runId, nameof(AnonymiseFeedbackAction), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(AnonymiseFeedbackAction), startTime, ex);

                //Rethrow the error
                throw;
            }
        }

        public void AnonymiseFeedback(Feedback feedback)
        {
            // TODO: Anonymise individual feedback item here (check what to anonymise to)
        }

    }
}
