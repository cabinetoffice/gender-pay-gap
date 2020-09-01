using System;
using System.Collections.Generic;
using System.Linq;
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
                GetAndAnonymiseFeedback();

                JobHelpers.LogFunctionEnd(runId, nameof(AnonymiseFeedbackAction), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(AnonymiseFeedbackAction), startTime, ex);

                //Rethrow the error
                throw;
            }
        }

        public void GetAndAnonymiseFeedback()
        {
            DateTime threeYearsAgo = VirtualDateTime.Now.AddYears(-3);

            List<Feedback> feedbackItemsToAnonymise = dataRepository.GetAll<Feedback>()
                .Where(f => f.CreatedDate < threeYearsAgo)
                .Where(f => !f.HasBeenAnonymised)
                .ToList();

            foreach (Feedback feedbackItem in feedbackItemsToAnonymise)
            {
                AnonymiseFeedbackItem(feedbackItem);
            }

            dataRepository.SaveChangesAsync().Wait();
        }

        public void AnonymiseFeedbackItem(Feedback feedback)
        {
            feedback.OtherSourceText = Anonymise(feedback.OtherSourceText);
            feedback.OtherReasonText = Anonymise(feedback.OtherReasonText);
            feedback.OtherPersonText = Anonymise(feedback.OtherPersonText);
            feedback.EmailAddress = Anonymise(feedback.EmailAddress);
            feedback.PhoneNumber = Anonymise(feedback.PhoneNumber);
            feedback.Details = Anonymise(feedback.Details);

            feedback.HasBeenAnonymised = true;
        }

        private static string Anonymise(string original)
        {
            return string.IsNullOrWhiteSpace(original)
                ? "not supplied"
                : "supplied";
        }

    }
}
