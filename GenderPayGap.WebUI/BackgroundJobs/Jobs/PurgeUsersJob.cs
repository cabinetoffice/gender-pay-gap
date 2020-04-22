using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.BackgroundJobs.Jobs
{
    public class PurgeUsersJob
    {
        private readonly IDataRepository dataRepository;

        public PurgeUsersJob(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        public async Task PurgeUsers()
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(PurgeUsers), startTime);

            DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.EmailVerificationExpiryDays);
            DateTime pinExpiryDate = VirtualDateTime.Now.AddDays(0 - Global.PinInPostExpiryDays);

            List<User> users = dataRepository.GetAll<User>()
                .Where(u => u.EmailVerifiedDate == null)
                .Where(u => (u.EmailVerifySendDate == null || u.EmailVerifySendDate.Value < deadline))
                .Include(u => u.UserOrganisations)
                .ToList();

            foreach (User user in users)
            {
                PurgeUser(user, pinExpiryDate, runId, startTime);
            }

            JobHelpers.LogFunctionEnd(runId, nameof(PurgeUsers), startTime);
        }

        private static async void PurgeUser(User user, DateTime pinExpiryDate, string runId, DateTime startTime)
        {
            try
            {
                var logItem = new ManualChangeLogModel(
                    nameof(PurgeUsers),
                    ManualActions.Delete,
                    AppDomain.CurrentDomain.FriendlyName,
                    nameof(user.UserId),
                    user.UserId.ToString(),
                    null,
                    JsonConvert.SerializeObject(new { user.UserId, user.EmailAddress, user.JobTitle, user.Fullname }),
                    null);

                DeleteUserAndAuditLogs(user);
                await Global.ManualChangeLog.WriteAsync(logItem);
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
                .Where(log => log.Action == AuditedAction.AdminResendVerificationEmail).ToList();

            foreach (AuditLog log in auditLogs)
            {
                dataRepository.Delete(log);
            }

            dataRepository.Delete(user);
            dataRepository.SaveChangesAsync().Wait();
        }


    }
}
