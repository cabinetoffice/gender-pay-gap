using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
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
                .Where(u => u.EmailVerifiedDate == null)
                .Where(u => u.EmailVerifySendDate == null || u.EmailVerifySendDate.Value < deadline)
                .Include(u => u.UserOrganisations)
                .ToList();

            foreach (User user in users)
            {
                PurgeUser(user, pinExpiryDate);
            }
        }

        private void PurgeUser(User user, DateTime pinExpiryDate)
        {
            auditLogger.AuditChangeToUser(
                AuditedAction.PurgeUser,
                user,
                new {user.UserId, user.EmailAddress, user.JobTitle, user.Fullname},
                null);

            DeleteUserAndAuditLogs(user);
        }

        private static void DeleteUserAndAuditLogs(User user)
        {
            var dataRepository = MvcApplication.ContainerIoC.Resolve<IDataRepository>();

            List<AuditLog> auditLogs = dataRepository
                .GetAll<AuditLog>()
                .Where(log => log.ImpersonatedUser.UserId == user.UserId)
                .Where(log => log.Action == AuditedAction.AdminResendVerificationEmail)
                .ToList();

            foreach (AuditLog log in auditLogs)
            {
                dataRepository.Delete(log);
            }

            dataRepository.Delete(user);
            dataRepository.SaveChangesAsync().Wait();
        }

    }
}
