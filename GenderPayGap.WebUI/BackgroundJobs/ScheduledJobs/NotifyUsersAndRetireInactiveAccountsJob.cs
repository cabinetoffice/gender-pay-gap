using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Services;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class NotifyUsersAndRetireInactiveAccountsJob
    {
        private readonly IDataRepository dataRepository;
        private readonly EmailSendingService emailSendingService;

        public NotifyUsersAndRetireInactiveAccountsJob(IDataRepository dataRepository, EmailSendingService emailSendingService)
        {
            this.dataRepository = dataRepository;
            this.emailSendingService = emailSendingService;
        }

        public void NotifyUsersAndRetireInactiveAccounts()
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(NotifyUsersAndRetireInactiveAccounts), startTime);

            try
            {
                SendReminderEmails();

                RetireUsers();

                JobHelpers.LogFunctionEnd(runId, nameof(NotifyUsersAndRetireInactiveAccounts), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(NotifyUsersAndRetireInactiveAccounts), startTime, ex);

                //Rethrow the error
                throw;
            }
        }

        public void SendReminderEmails()
        {
            DateTime threeYearsAgo = VirtualDateTime.Now.AddYears(-3);
            DateTime threeYearsAgoMinusThirtyDays = threeYearsAgo.AddDays(30);
            DateTime threeYearsAgoMinusSevenDays = threeYearsAgo.AddDays(7);

            List<User> usersToSendReminders = dataRepository.GetAll<User>()
                .Where(u => u.LoginDate == threeYearsAgoMinusThirtyDays || u.LoginDate == threeYearsAgoMinusSevenDays)
                .ToList();

            foreach (User user in usersToSendReminders)
            {
                // TODO: Update loginUrl
                string daysRemaining = user.LoginDate == threeYearsAgoMinusThirtyDays ? "30" : "7";
                emailSendingService.SendAccountRetirementNotificationEmail(user.ContactEmailAddress, daysRemaining, "example url");
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

            dataRepository.SaveChangesAsync().Wait();

        }
    }
}
