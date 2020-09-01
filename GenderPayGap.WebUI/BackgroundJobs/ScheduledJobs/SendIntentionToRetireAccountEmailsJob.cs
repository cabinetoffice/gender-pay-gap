using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
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
                SendReminderEmails();

                JobHelpers.LogFunctionEnd(runId, nameof(SendIntentionToRetireAccountEmails), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(SendIntentionToRetireAccountEmails), startTime, ex);

                //Rethrow the error
                throw;
            }
        }

        public void SendReminderEmails()
        {
            DateTime threeYearsAgo = VirtualDateTime.Now.AddYears(-3);
            DateTime threeYearsAgoMinusThirtyDays = threeYearsAgo.AddDays(30);
            DateTime threeYearsAgoMinusSevenDays = threeYearsAgo.AddDays(7);

            List<User> usersToSendThirtyDayReminders = dataRepository.GetAll<User>()
                .Where(u => u.LoginDate == threeYearsAgoMinusThirtyDays)
                .ToList();

            List<User> usersToSendSevenDayReminders = dataRepository.GetAll<User>()
                .Where(u => u.LoginDate == threeYearsAgoMinusSevenDays)
                .ToList();

            foreach (User user in usersToSendThirtyDayReminders)
            {
                // TODO: Send 30 day reminder email
            }

            foreach (User user in usersToSendSevenDayReminders)
            {
                // TODO: Send 7 day reminder email
            }

            dataRepository.SaveChangesAsync().Wait();
        }

        public void RetireUsers()
        {
            DateTime threeYearsAgo = VirtualDateTime.Now.AddYears(-3);

            List<User> usersToRetire = dataRepository.GetAll<User>()
                .Where(u => u.LoginDate < threeYearsAgo)
                .ToList();

            foreach (User user in usersToRetire)
            {
                // TODO: Retire user
            }

        }
    }
}
