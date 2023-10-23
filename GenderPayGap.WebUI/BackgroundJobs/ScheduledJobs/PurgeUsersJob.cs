using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Services;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class PurgeUsersJob
    {

        private readonly AuditLogger auditLogger;
        private readonly IDataRepository dataRepository;

        public PurgeUsersJob(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }


        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        public void PurgeUsers()
        {
            JobHelpers.RunAndLogJob(PurgeUsersAction, nameof(PurgeUsers));
        }

        private void PurgeUsersAction()
        {
            DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.EmailVerificationExpiryDays);
            DateTime pinExpiryDate = VirtualDateTime.Now.AddDays(0 - Global.PinInPostExpiryDays);

            List<User> users = dataRepository.GetAll<User>()
                .Where(u => u.Status == UserStatuses.New || u.Status == UserStatuses.Active)
                .Where(u => u.UserRole == UserRole.Employer)  // Only purge Employer users 
                .Where(u => u.EmailVerifiedDate == null)
                .Where(u => u.EmailVerifySendDate == null || u.EmailVerifySendDate.Value < deadline)
                .Include(u => u.UserOrganisations)
                .ToList();

            foreach (User user in users)
            {
                PurgeUser(user);
            }
        }

        private void PurgeUser(User user)
        {
            auditLogger.AuditChangeToUser(
                AuditedAction.PurgeUser,
                user,
                new {user.UserId, user.EmailAddress, user.JobTitle, user.Fullname});

            user.SetStatus(UserStatuses.Retired, user, "User retired by PurgeUserJob");
            dataRepository.SaveChanges();
        }

    }
}
