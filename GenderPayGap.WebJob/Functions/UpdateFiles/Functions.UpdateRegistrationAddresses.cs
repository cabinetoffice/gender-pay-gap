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

        public async Task UpdateRegistrationAddressesAsync([TimerTrigger("40 1 * * *" /* 01:40 once per day */)]
            TimerInfo timer,
            ILogger log)
        {
            string funcName = nameof(UpdateRegistrationAddressesAsync);

            var runId = CreateRunId();
            var startTime = DateTime.Now;
            LogFunctionStart(runId,  nameof(UpdateRegistrationAddressesAsync), startTime);
            
            try
            {
                string filePath = Path.Combine(Global.DownloadsPath, Filenames.RegistrationAddresses);

                //Dont execute on startup if file already exists
                if (!StartedJobs.Contains(funcName) && await Global.FileRepository.GetFileExistsAsync(filePath))
                {
                    log.LogDebug($"Skipped {funcName} at start up.");
                    return;
                }

                // Flag the UpdateRegistrationAddresses web job as started
                StartedJobs.Add(funcName);

                await UpdateRegistrationAddressesAsync(filePath, log);

                LogFunctionEnd(runId, nameof(UpdateRegistrationAddressesAsync), startTime);
            }
            catch (Exception ex)
            {
                LogFunctionError(runId, nameof(UpdateRegistrationAddressesAsync), startTime, ex );
                
                //Rethrow the error
                throw;
            }
        }

        public async Task UpdateRegistrationAddressesAsync(string filePath, ILogger log)
        {
            string funcName = nameof(UpdateRegistrationAddressesAsync);

            // Ensure the UpdateRegistrationAddresses web job is not already running
            if (RunningJobs.Contains(funcName))
            {
                log.LogDebug($"Skipped {funcName} because already running.");
                return;
            }

            try
            {
                // Flag the UpdateRegistrationAddresses web job as running
                RunningJobs.Add(funcName);

                // Cache the latest registration addresses
                List<RegistrationAddressesFileModel> latestRegistrationAddresses = await GetLatestRegistrationAddressesAsync();

                // Write yearly records to csv files
                await WriteRecordsPerYearAsync(
                    filePath,
                    async year => {
                        foreach (RegistrationAddressesFileModel model in latestRegistrationAddresses)
                        {
                            // get organisation scope and submission per year
                            Return returnByYear = await _SubmissionBL.GetLatestSubmissionBySnapshotYearAsync(model.OrganisationId, year);
                            OrganisationScope scopeByYear = await _ScopeBL.GetLatestScopeBySnapshotYearAsync(model.OrganisationId, year);

                            // update file model with year data
                            model.HasSubmitted = returnByYear == null ? "False" : "True";
                            model.ScopeStatus = scopeByYear?.ScopeStatus;
                        }

                        return latestRegistrationAddresses;
                    });
            }
            finally
            {
                RunningJobs.Remove(funcName);
            }
        }

        public async Task<List<RegistrationAddressesFileModel>> GetLatestRegistrationAddressesAsync()
        {
            // Get all the latest verified organisation registrations
            List<Organisation> verifiedOrgs = await _DataRepository.GetEntities<Organisation>()
                .Where(uo => uo.LatestRegistration != null)
                .Include(uo => uo.LatestRegistration)
                .Include(uo => uo.LatestAddress)
                .Include(uo => uo.LatestReturn)
                .Include(uo => uo.LatestScope)
                .ToListAsync();

            return verifiedOrgs.Select(
                    vo => {
                        // Read the latest address for the organisation
                        OrganisationAddress latestAddress = vo.LatestAddress;
                        if (latestAddress == null)
                        {
                            throw new Exception(
                                $"Organisation {vo.OrganisationId} has a latest registration with no Organisation Address associated");
                        }

                        // Get the latest user for the organisation
                        User latestRegistrationUser = vo.LatestRegistration?.User;
                        if (latestAddress == null)
                        {
                            throw new Exception($"Organisation {vo.OrganisationId} has a latest registration with no User associated");
                        }

                        // Ensure the address lines don't start with null or whitespaces
                        var addressLines = new List<string>();
                        foreach (string line in new[] {latestAddress.Address1, latestAddress.Address2, latestAddress.Address3})
                        {
                            if (string.IsNullOrWhiteSpace(line) == false)
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
                        string expires = vo.SectorType.GetAccountingStartDate().AddYears(1).AddDays(-1).ToString("d MMMM yyyy");

                        // Generate csv row
                        return new RegistrationAddressesFileModel {
                            OrganisationId = vo.OrganisationId,
                            EmployerReference = vo.EmployerReference,
                            Sector = vo.SectorType,
                            LatestUserJobTitle = latestRegistrationUser.JobTitle,
                            LatestUserFullName = latestRegistrationUser.Fullname,
                            LatestUserStatus = latestRegistrationUser.Status.ToString(),
                            Company = vo.OrganisationName,
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
