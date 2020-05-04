using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class PurgeRegistrationsJob
    {

        private readonly AuditLogger auditLogger;

        private readonly IDataRepository dataRepository;

        public PurgeRegistrationsJob(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }


        //Remove any incomplete registrations
        public async Task PurgeRegistrations()
        {
            string runId = JobHelpers.CreateRunId();
            DateTime startTime = DateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(PurgeRegistrations), startTime);
            try
            {
                DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.PurgeUnconfirmedPinDays);
                List<UserOrganisation> registrations = await dataRepository.GetAll<UserOrganisation>()
                    .Where(u => u.PINConfirmedDate == null && u.PINSentDate != null && u.PINSentDate.Value < deadline)
                    .ToListAsync();
                foreach (UserOrganisation registration in registrations)
                {
                    auditLogger.AuditChangeToUser(
                        AuditedAction.PurgeRegistration,
                        registration.User,
                        new
                        {
                            registration.UserId,
                            registration.User.EmailAddress,
                            registration.OrganisationId,
                            registration.Organisation.EmployerReference,
                            registration.Organisation.OrganisationName,
                            registration.Method,
                            registration.PINSentDate,
                            registration.PINConfirmedDate
                        },
                        null);
                    dataRepository.Delete(registration);
                    await dataRepository.SaveChangesAsync();
                }

                JobHelpers.LogFunctionEnd(runId, nameof(PurgeRegistrations), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(PurgeRegistrations), startTime, ex);

                //Rethrow the error
                throw;
            }
        }

    }
}
