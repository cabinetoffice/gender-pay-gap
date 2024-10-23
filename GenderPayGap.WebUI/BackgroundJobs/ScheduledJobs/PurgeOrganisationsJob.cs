using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Services;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class PurgeOrganisationsJob
    {

        private readonly AuditLogger auditLogger;

        private readonly IDataRepository dataRepository;

        public PurgeOrganisationsJob(IDataRepository dataRepository, AuditLogger auditLogger)
        {
            this.dataRepository = dataRepository;
            this.auditLogger = auditLogger;
        }


        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        public void PurgeOrganisations()
        {
            JobHelpers.RunAndLogJob(PurgeOrganisationsAction, nameof(PurgeOrganisations));
        }

        private void PurgeOrganisationsAction()
        {
            DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.PurgeUnusedOrganisationDays);

            List<Organisation> organisationsToPurge = GetOrganisationsToPurge(deadline);

            foreach (Organisation org in organisationsToPurge)
            {
                PurgeOrganisation(org);
            }

            if (organisationsToPurge.Count > 0)
            {
                CustomLogger.Information(
                    $"Executed {nameof(PurgeOrganisations)} successfully: {organisationsToPurge.Count} organisations deleted",
                    new {Environment = Config.EnvironmentName});
            }
        }

        private List<Organisation> GetOrganisationsToPurge(DateTime deadline)
        {
            return dataRepository.GetAll<Organisation>()
                .Where(o => o.Status != OrganisationStatuses.Deleted)
                .Where(o => o.Created < deadline)
                .Where(o => !o.Returns.Any())
                .Where(o => !o.OrganisationScopes.Any(sc => sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.OutOfScope))
                // We need the AsEnumerable here because EF gets upset about method calls - so we get the list at this point and then can filter it using a method call
                .AsEnumerable()
                .Where(o => !o.UserOrganisations.Any(uo => uo.Method == RegistrationMethods.Manual || uo.HasBeenActivated() || uo.PINSentDate > deadline))
                .ToList();
        }

        private void PurgeOrganisation(Organisation org)
        {
            auditLogger.AuditChangeToOrganisation(
                AuditedAction.PurgeOrganisation,
                org,
                new
                {
                    org.OrganisationId,
                    Address = org.GetLatestAddress()?.GetAddressString(),
                    org.CompanyNumber,
                    org.OrganisationName,
                    org.SectorType,
                    org.Status,
                    SicCodes = org.GetSicSectionIdsString()
                });

            // Un-register all users for this Organisation
            org.UserOrganisations.ForEach(uo => dataRepository.Delete(uo));

            // Soft-Delete the Organisation
            org.SetStatus(OrganisationStatuses.Deleted, details: "Organisation deleted by PurgeOrganisationJob");

            dataRepository.SaveChanges();
            
        }

    }
}
