using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public async Task UpdateOrphanOrganisationsAsync([TimerTrigger(typeof(MidnightSchedule), RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            string funcName = nameof(UpdateOrphanOrganisationsAsync);

            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.OrphanOrganisations);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(funcName) && await Global.FileRepository.GetFileExistsAsync(filePath))
                {
                    log.LogDebug($"Skipped {funcName} at start up.");
                    return;
                }

                // Flag the UpdateUnregisteredOrganisations web job as started
                StartedJobs.Add(funcName);

                await UpdateOrphanOrganisationsAsync(filePath, log);

                log.LogDebug($"Executed {funcName}:successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed {funcName}:{ex.Message}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

        public async Task UpdateOrphanOrganisationsAsync(string filePath, ILogger log)
        {
            string funcName = nameof(UpdateOrphanOrganisationsAsync);

            // Ensure the UpdateUnregisteredOrganisations web job is not already running
            if (RunningJobs.Contains(funcName))
            {
                log.LogDebug($"Skipped {funcName} because already running.");
                return;
            }

            try
            {
                // Flag the UpdateUnregisteredOrganisations web job as running
                RunningJobs.Add(funcName);

                // Cache the latest unregistered organisations
                List<UnregisteredOrganisationsFileModel> unregisteredOrganisations = await GetOrphanOrganisationsAsync();

                int year = SectorTypes.Private.GetAccountingStartDate().Year;

                // Write yearly records to csv files
                await WriteRecordsForYearAsync(
                    filePath,
                    year,
                    async () => {
                        foreach (UnregisteredOrganisationsFileModel model in unregisteredOrganisations)
                        {
                            // get organisation scope and submission per year
                            Return returnByYear = await _SubmissionBL.GetLatestSubmissionBySnapshotYearAsync(model.OrganisationId, year);
                            OrganisationScope scopeByYear = await _ScopeBL.GetLatestScopeBySnapshotYearAsync(model.OrganisationId, year);

                            // update file model with year data
                            model.HasSubmitted = returnByYear == null ? "False" : "True";
                            model.ScopeStatus = scopeByYear?.ScopeStatus;
                        }

                        return unregisteredOrganisations;
                    });
            }
            finally
            {
                RunningJobs.Remove(funcName);
            }
        }

        private async Task<List<UnregisteredOrganisationsFileModel>> GetOrphanOrganisationsAsync()
        {
            // Get all the latest organisations with no registrations
            DateTime pinExpiresDate = Global.PinExpiresDate;
            List<Organisation> unregisteredOrgs = await _DataRepository.GetEntities<Organisation>()
                .Where(
                    o => o.Status == OrganisationStatuses.Active
                         && (o.LatestScope.ScopeStatus == ScopeStatuses.InScope
                             || o.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope)
                         && (o.UserOrganisations == null
                             || !o.UserOrganisations.Any(
                                 uo => uo.PINConfirmedDate != null
                                       || uo.Method == RegistrationMethods.Manual
                                       || uo.Method == RegistrationMethods.PinInPost
                                       && uo.PINSentDate.HasValue
                                       && uo.PINSentDate.Value > pinExpiresDate)))
                .Include(o => o.LatestAddress)
                .ToListAsync();

            return unregisteredOrgs.Select(
                    org => {
                        // Read the latest address for the organisation
                        OrganisationAddress latestAddress = org.LatestAddress;
                        if (latestAddress == null)
                        {
                            throw new ArgumentException($"Organisation {org.OrganisationId} has no latest address associated");
                        }

                        // Ensure the address lines don't start with null or whitespaces
                        var addressLines = new List<string>();
                        foreach (string line in new[] {latestAddress.Address1, latestAddress.Address2, latestAddress.Address3})
                        {
                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                addressLines.Add(line);
                            }
                        }

                        for (int i = addressLines.Count; i < 3; i++)
                        {
                            addressLines.Add(string.Empty);
                        }

                        // Format post code with the po boxes
                        string postCode = latestAddress.PostCode;
                        if (!string.IsNullOrWhiteSpace(postCode) && !string.IsNullOrWhiteSpace(latestAddress.PoBox))
                        {
                            postCode = latestAddress.PoBox + ", " + postCode;
                        }
                        else if (!string.IsNullOrWhiteSpace(latestAddress.PoBox) && string.IsNullOrWhiteSpace(postCode))
                        {
                            postCode = latestAddress.PoBox;
                        }

                        // Convert two letter country codes to full country names
                        string countryCode = Country.FindTwoLetterCode(latestAddress.Country);

                        // Retrieve the SectorType reporting snapshot date (d MMMM yyyy)
                        string expires = org.SectorType.GetAccountingStartDate().AddYears(1).AddDays(-1).ToString("d MMMM yyyy");

                        // Generate csv row
                        return new UnregisteredOrganisationsFileModel {
                            OrganisationId = org.OrganisationId,
                            EmployerReference = org.EmployerReference,
                            Sector = org.SectorType,
                            Company = org.OrganisationName,
                            Address1 = addressLines[0],
                            Address2 = addressLines[1],
                            Address3 = addressLines[2],
                            City = latestAddress.TownCity,
                            Postcode = postCode,
                            County = latestAddress.County,
                            Country = string.IsNullOrWhiteSpace(countryCode) || countryCode.EqualsI("GB") ? latestAddress.Country : null,
                            CreatedByUserId = latestAddress.CreatedByUserId,
                            Expires = expires
                        };
                    })
                .OrderBy(model => model.Company)
                .ToList();
        }

    }

}
