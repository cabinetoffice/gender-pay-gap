using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task PurgeUsers()
        {
            string runId = JobHelpers.CreateRunId();
            DateTime startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(PurgeUsers), startTime);

            DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.EmailVerificationExpiryDays);
            DateTime pinExpiryDate = VirtualDateTime.Now.AddDays(0 - Global.PinInPostExpiryDays);

            List<User> users = dataRepository.GetAll<User>()
                .Where(u => u.EmailVerifiedDate == null)
                .Where(u => u.EmailVerifySendDate == null || u.EmailVerifySendDate.Value < deadline)
                .Include(u => u.UserOrganisations)
                .ToList();

            foreach (User user in users)
            {
                PurgeUser(user, pinExpiryDate, runId, startTime);
            }

            JobHelpers.LogFunctionEnd(runId, nameof(PurgeUsers), startTime);
        }

        private void PurgeUser(User user, DateTime pinExpiryDate, string runId, DateTime startTime)
        {
            try
            {
                auditLogger.AuditChangeToUser(
                    AuditedAction.PurgeUser,
                    user,
                    new {user.UserId, user.EmailAddress, user.JobTitle, user.Fullname},
                    null);

                DeleteUserAndAuditLogs(user);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(PurgeUsers), startTime, ex);
            }
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
