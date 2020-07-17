using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Services;

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
        public void PurgeRegistrations()
        {
            JobHelpers.RunAndLogJob(PurgeRegistrationsAction, nameof(PurgeRegistrations));
        }

        private void PurgeRegistrationsAction()
        {
            DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.PurgeUnconfirmedPinDays);

            List<UserOrganisation> registrations = dataRepository.GetAll<UserOrganisation>()
                .Where(uo => uo.PINConfirmedDate == null)
                .Where(uo => uo.PINSentDate != null)
                .Where(uo => uo.PINSentDate.Value < deadline)
                .ToList();

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
                    });
                dataRepository.Delete(registration);
                dataRepository.SaveChangesAsync().Wait();
            }
        }

    }
}
