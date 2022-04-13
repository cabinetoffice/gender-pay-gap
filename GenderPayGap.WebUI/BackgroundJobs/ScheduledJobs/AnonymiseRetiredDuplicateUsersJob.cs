using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Services;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class AnonymiseRetiredDuplicateUsersJob
    {

        private readonly AuditLogger auditLogger;
        private readonly IDataRepository dataRepository;

        public AnonymiseRetiredDuplicateUsersJob(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        //Remove any duplicate retired user details
        public void AnonymiseRetiredDuplicateUsers()
        {
            JobHelpers.RunAndLogJob(AnonymiseRetiredDuplicateUsersAction, nameof(AnonymiseRetiredDuplicateUsers));
        }

        private void AnonymiseRetiredDuplicateUsersAction()
        {
            List<User> users = dataRepository.GetAll<User>().ToList();

            var usersWithMultipleAccounts = users
                .Where(u => u.HasBeenAnonymised == false)
                .GroupBy(u => u.EmailAddress).Where(e => e.Count() > 1)
                .SelectMany(g => g)
                .ToList();

            foreach (var user in usersWithMultipleAccounts.Where(u => u.Status == UserStatuses.Retired))
            {
                AnonymiseRetiredDuplicateUser(user);
            }

            dataRepository.SaveChanges();
        }

        private void AnonymiseRetiredDuplicateUser(User user)
        {
            // Anonymise user data
            List<AuditLog> userAuditLogs = dataRepository.GetAll<AuditLog>()
            .Where(al => al.OriginalUserId == user.UserId || al.ImpersonatedUserId == user.UserId).ToList();
            
            UserAnonymisationHelper.AnonymiseAndRetireUser(user, "Duplicate retired user anonymised by AnonymiseRetiredDuplicateUsersJob");
            UserAnonymisationHelper.AnonymiseAuditLogsForUser(userAuditLogs);
        }
    }
}
