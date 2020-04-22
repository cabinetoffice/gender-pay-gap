using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class PurgeRegistrationsJob
    {

        private readonly IDataRepository dataRepository;

        public PurgeRegistrationsJob(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        //Remove any incomplete registrations
        public async Task PurgeRegistrations()
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = DateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(PurgeRegistrations), startTime);
            try
            {
                DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.PurgeUnconfirmedPinDays);
                List<UserOrganisation> registrations = await dataRepository.GetAll<UserOrganisation>()
                    .Where(u => u.PINConfirmedDate == null && u.PINSentDate != null && u.PINSentDate.Value < deadline)
                    .ToListAsync();
                foreach (UserOrganisation registration in registrations)
                {
                    var logItem = new ManualChangeLogModel(
                        nameof(PurgeRegistrations),
                        ManualActions.Delete,
                        AppDomain.CurrentDomain.FriendlyName,
                        $"{nameof(UserOrganisation.OrganisationId)}:{nameof(UserOrganisation.UserId)}",
                        $"{registration.OrganisationId}:{registration.UserId}",
                        null,
                        JsonConvert.SerializeObject(
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
                            }),
                        null);
                    dataRepository.Delete(registration);
                    await dataRepository.SaveChangesAsync();
                    await Global.ManualChangeLog.WriteAsync(logItem);
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
