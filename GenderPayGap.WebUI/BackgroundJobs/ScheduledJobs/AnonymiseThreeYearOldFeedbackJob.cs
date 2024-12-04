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

        public void AnonymiseFeedback()
        {
            JobHelpers.RunAndLogJob(GetAndAnonymiseFeedback, nameof(AnonymiseFeedback));
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

            dataRepository.SaveChanges();
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
