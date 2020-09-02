using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
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

        private void SendReminderEmails()
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
                emailSendingService.SendAccountRetirementNotificationEmail(user.EmailAddress, daysRemaining, "example url");
            }

        }

        private void RetireUsers()
        {
            DateTime threeYearsAgo = VirtualDateTime.Now.AddYears(-3);

            List<User> usersToRetire = dataRepository.GetAll<User>()
                .Where(u => u.LoginDate < threeYearsAgo)
                .Where(u => u.Status != UserStatuses.Retired)
                .ToList();

            foreach (User user in usersToRetire)
            {
                user.Firstname = $"User{user.UserId}";
                user.Lastname = $"User{user.UserId}";
                user.JobTitle = "Anonymised";
                user.EmailAddress = "Anonymised";
                user.ContactFirstName = "Anonymised";
                user.ContactLastName = "Anonymised";
                user.ContactJobTitle = "Anonymised";
                user.ContactOrganisation = "Anonymised";
                user.ContactEmailAddress = "Anonymised";
                user.ContactPhoneNumber = "Anonymised";
                user.Salt = "Anonymised";
                user.PasswordHash = "Anonymised";
                user.Status = UserStatuses.Retired;

                List<AuditedAction> actionsToAnonymise = new List<AuditedAction>(new []
                    {
                        AuditedAction.UserChangeEmailAddress, 
                        AuditedAction.UserChangeName, 
                        AuditedAction.UserChangeJobTitle, 
                        AuditedAction.UserChangePhoneNumber, 
                        AuditedAction.PurgeRegistration, 
                        AuditedAction.PurgeUser, 
                        AuditedAction.RegistrationLog
                    });

                List<AuditLog> userAuditLogs = dataRepository.GetAll<AuditLog>()
                    .Where(al => al.OriginalUserId == user.UserId || al.ImpersonatedUserId == user.UserId)
                    .Where(al => actionsToAnonymise.Contains(al.Action))
                    .ToList();

                foreach (AuditLog auditLog in userAuditLogs)
                {
                    auditLog.DetailsString = "Anonymised";
                }
            }

            dataRepository.SaveChangesAsync().Wait();

        }
    }
}
