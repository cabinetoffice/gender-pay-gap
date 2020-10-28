using System;
using System.Collections.Generic;
using System.Linq;
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
            JobHelpers.RunAndLogJob(NotifyUsersAndRetireInactiveAccountsAction, nameof(NotifyUsersAndRetireInactiveAccounts));
        }

        private void NotifyUsersAndRetireInactiveAccountsAction()
        {
            SendReminderEmails();

            RetireUsers();
        }

        private void SendReminderEmails()
        {
            DateTime threeYearsAgo = VirtualDateTime.Now.AddYears(-3);
            DateTime threeYearsAgoMinusThirtyDays = threeYearsAgo.AddDays(30);
            DateTime threeYearsAgoMinusSevenDays = threeYearsAgo.AddDays(7);

            List<User> usersToSendReminders = dataRepository.GetAll<User>()
                .Where(u => u.LoginDate >= threeYearsAgoMinusThirtyDays && u.LoginDate < threeYearsAgoMinusThirtyDays.AddDays(1) || u.LoginDate >= threeYearsAgoMinusSevenDays && u.LoginDate < threeYearsAgoMinusSevenDays.AddDays(1) )
                .ToList();

            foreach (User user in usersToSendReminders)
            {
                string daysRemaining = InThirtyDayRange(user, threeYearsAgoMinusThirtyDays) ? "30" : "7";
                emailSendingService.SendAccountRetirementNotificationEmail(user.EmailAddress, user.Fullname, daysRemaining);
            }

        }

        private bool InThirtyDayRange(User user, DateTime threeYearsAgoMinusThirtyDays)
        {
            return user.LoginDate >= threeYearsAgoMinusThirtyDays && user.LoginDate < threeYearsAgoMinusThirtyDays.AddDays(1);
        }

        private void RetireUsers()
        {
            DateTime threeYearsAgo = VirtualDateTime.Now.AddYears(-3);

            List<User> usersToRetire = dataRepository.GetAll<User>()
                .Where(u => u.LoginDate < threeYearsAgo)
                .Where(u => !u.HasBeenAnonymised)
                .ToList();

            foreach (User user in usersToRetire)
            {
                AnonymiseUser(user);
                
                AnonymiseAuditLogsForUser(user.UserId);
            }
            
            dataRepository.SaveChanges();
        }

        private void AnonymiseUser(User user)
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

            user.HasBeenAnonymised = true;
            
            user.SetStatus(UserStatuses.Retired, user, "User retired by RetireInactiveAccountsJob");
        }

        private void AnonymiseAuditLogsForUser(long userId)
        {
            var actionsToAnonymise = new List<AuditedAction>
            {
                AuditedAction.UserChangeEmailAddress, 
                AuditedAction.UserChangeName, 
                AuditedAction.UserChangeJobTitle, 
                AuditedAction.UserChangePhoneNumber, 
                AuditedAction.PurgeRegistration, 
                AuditedAction.PurgeUser, 
                AuditedAction.RegistrationLog
            };

            List<AuditLog> userAuditLogs = dataRepository.GetAll<AuditLog>()
                .Where(al => al.OriginalUserId == userId || al.ImpersonatedUserId == userId)
                .Where(al => actionsToAnonymise.Contains(al.Action))
                .ToList();

            foreach (AuditLog auditLog in userAuditLogs)
            {
                auditLog.Details = new Dictionary<string, string> {{"Anonymised", "Anonymised"}};
            }
        
        }
    }
}
