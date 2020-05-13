using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class PurgeOrganisationsJob
    {

        private readonly IDataRepository dataRepository;

        public PurgeOrganisationsJob(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        //Remove any unverified users their addresses, UserOrgs, Org and addresses and archive to zip
        public async Task PurgeOrganisations()
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = DateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(PurgeOrganisations), startTime);
            try
            {
                DateTime deadline = VirtualDateTime.Now.AddDays(0 - Global.PurgeUnusedOrganisationDays);
                List<Organisation> orgs = await dataRepository.GetAll<Organisation>()
                    .Where(
                        o => o.Created < deadline
                             && !o.Returns.Any()
                             && !o.OrganisationScopes.Any(
                                 sc => sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.OutOfScope)
                             && !o.UserOrganisations.Any(
                                 uo => uo.Method == RegistrationMethods.Manual || uo.PINConfirmedDate != null || uo.PINSentDate > deadline)
                             && !o.OrganisationAddresses.Any(a => a.CreatedByUserId == -1))
                    .ToListAsync();

                if (orgs.Any())
                {
                    var count = 0;
                    foreach (Organisation org in orgs)
                    {
                        var logItem = new ManualChangeLogModel(
                            nameof(PurgeOrganisations),
                            ManualActions.Delete,
                            AppDomain.CurrentDomain.FriendlyName,
                            nameof(org.OrganisationId),
                            org.OrganisationId.ToString(),
                            null,
                            JsonConvert.SerializeObject(
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
                                }),
                            null);
                        EmployerSearchModel searchRecord = org.ToEmployerSearchResult(true);

                        await dataRepository.BeginTransactionAsync(
                            async () => {
                                try
                                {
                                    org.UserOrganisations.ForEach(uo => dataRepository.Delete(uo));
                                    await dataRepository.SaveChangesAsync();

                                    dataRepository.Delete(org);
                                    await dataRepository.SaveChangesAsync();

                                    dataRepository.CommitTransaction();
                                }
                                catch (Exception ex)
                                {
                                    dataRepository.RollbackTransaction();
                                    CustomLogger.Error(
                                        $"{nameof(PurgeOrganisations)}: Failed to purge organisation {org.OrganisationId} '{org.OrganisationName}' "
                                        + $"ERROR: {ex.Message}:{ex.GetDetailsText()}",
                                        new
                                        {
                                            Error = ex
                                        });
                                }
                            });
                        //Remove this organisation from the search index
                        await Global.SearchRepository.RemoveFromIndexAsync(new[] { searchRecord });

                        await Global.ManualChangeLog.WriteAsync(logItem);
                        count++;
                    }

                    CustomLogger.Information($"Executed {nameof(PurgeOrganisations)} successfully: {count} organisations deleted", new
                    {
                        runId,
                        Environment = Config.EnvironmentName,
                    });
                }

                JobHelpers.LogFunctionEnd(runId, nameof(PurgeOrganisations), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(PurgeOrganisations), startTime, ex);

                //Rethrow the error
                throw;
            }
        }

    }
}
