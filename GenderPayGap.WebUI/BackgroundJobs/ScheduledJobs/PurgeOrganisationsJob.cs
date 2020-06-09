using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
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
            JobHelpers.RunAndLogJob(() => PurgeOrganisationsAction().Wait(), nameof(PurgeOrganisations));
        }

        private async Task PurgeOrganisationsAction()
        {
            DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.PurgeUnusedOrganisationDays);

            List<Organisation> organisationsToPurge = GetOrganisationsToPurge(deadline);

            foreach (Organisation org in organisationsToPurge)
            {
                await PurgeOrganisation(org);
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
                .Where(
                    o => o.Created < deadline
                         && !o.Returns.Any()
                         && !o.OrganisationScopes.Any(sc => sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.OutOfScope)
                         && !o.UserOrganisations.Any(
                             uo => uo.Method == RegistrationMethods.Manual || uo.PINConfirmedDate != null || uo.PINSentDate > deadline)
                         && !o.OrganisationAddresses.Any(a => a.CreatedByUserId == -1))
                .ToList();
        }

        private async Task PurgeOrganisation(Organisation org)
        {
            EmployerSearchModel searchRecord = org.ToEmployerSearchResult(true);

            auditLogger.AuditChangeToOrganisation(
                AuditedAction.PurgeOrganisation,
                org,
                new
                {
                    org.OrganisationId,
                    Address = org.GetLatestAddress()?.GetAddressString(),
                    org.EmployerReference,
                    org.CompanyNumber,
                    org.OrganisationName,
                    org.SectorType,
                    org.Status,
                    SicCodes = org.GetSicSectionIdsString(),
                    SicSource = org.GetSicSource(),
                    org.DateOfCessation
                });

            // Un-register all users for this Organisation
            org.UserOrganisations.ForEach(uo => dataRepository.Delete(uo));

            // Soft-Delete the Organisation
            org.SetStatus(OrganisationStatuses.Deleted, details: "Organisation deleted by PurgeOrganisationJob");

            dataRepository.SaveChangesAsync().Wait();
            
        }

    }
}
