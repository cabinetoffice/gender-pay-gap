using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
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
            JobHelpers.RunAndLogSingletonJob(NotifyUsersAndRetireInactiveAccountsAction, nameof(NotifyUsersAndRetireInactiveAccounts));
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
                .Where(u =>
                    (u.LoginDate >= threeYearsAgoMinusThirtyDays && u.LoginDate < threeYearsAgoMinusThirtyDays.AddDays(1)) ||
                    (u.LoginDate >= threeYearsAgoMinusSevenDays && u.LoginDate < threeYearsAgoMinusSevenDays.AddDays(1))
                )
                .Where(u => !u.HasBeenAnonymised)
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
                List<AuditLog> userAuditLogs = dataRepository.GetAll<AuditLog>()
                    .Where(al => al.OriginalUserId == user.UserId || al.ImpersonatedUserId == user.UserId).ToList();

                UserAnonymisationHelper.AnonymiseAndRetireUser(user, "User retired by RetireInactiveAccountsJob");
                UserAnonymisationHelper.AnonymiseAuditLogsForUser(userAuditLogs);
            }
            
            dataRepository.SaveChanges();
        }
    }
}
