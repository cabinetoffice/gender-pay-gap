﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
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

        //Update data for viewing service
        public async Task UpdateDownloadFiles([TimerTrigger("10 * * * *" /* at 10 minutes past each hour */, RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            var runId = CreateRunId();
            var startTime = VirtualDateTime.Now;
            LogFunctionStart(runId, nameof(UpdateDownloadFiles), startTime);

            try
            {
                await UpdateDownloadFilesAsync(log);
            }
            catch (Exception ex)
            {
                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", ex.Message);
                //Rethrow the error
                throw;
            }

            LogFunctionEnd(runId, nameof(UpdateDownloadFiles), startTime);
        }

        //Update GPG download file
        public async Task UpdateDownloadFilesAsync(ILogger log)
        {
            CustomLogger.Information("UpdateDownloadFiles: Checking there is a directory");
            //Get the downloads location
            string downloadsLocation = Global.DownloadsLocation;

            //Ensure we have a directory
            if (!await Global.FileRepository.GetDirectoryExistsAsync(downloadsLocation))
            {
                await Global.FileRepository.CreateDirectoryAsync(downloadsLocation);
            }

            CustomLogger.Information("UpdateDownloadFiles: Getting return years");
            List<int> returnYears = _DataRepository.GetAll<Return>()
                .Where(r => r.Status == ReturnStatuses.Submitted)
                .Select(r => r.AccountingDate.Year)
                .Distinct()
                .ToList();

            CustomLogger.Information($"UpdateDownloadFiles: - Loading Returns");
            List<Organisation> activeOrganisations = _DataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Include(o => o.OrganisationNames)
                .Include(o => o.OrganisationAddresses)
                .Include(o => o.OrganisationSicCodes)
                .Include(o => o.Returns)
                .ToList();

            foreach (int year in returnYears)
            {
                CustomLogger.Information($"UpdateDownloadFiles: Creating download for year {year}");

                string downloadFilePattern = $"GPGData_{year}-{year + 1}.csv";
                IEnumerable<string> files = await Global.FileRepository.GetFilesAsync(downloadsLocation, downloadFilePattern);
                string oldDownloadFilePath = files.FirstOrDefault();

                CustomLogger.Information($"UpdateDownloadFiles: - Filtering Returns");
                List<Return> returns = activeOrganisations.SelectMany(o => o.Returns)
                    .Where(r => r.AccountingDate.Year == year)
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .ToList();

                returns.RemoveAll(r => r.Organisation.OrganisationName.StartsWithI(Global.TestPrefix));

                CustomLogger.Information($"UpdateDownloadFiles: - Converting Returns into results");
                List<DownloadResult> downloadData = returns.ToList()
                    .Select(r => r.ToDownloadResult())
                    .OrderBy(d => d.EmployerName)
                    .ToList();

                CustomLogger.Information($"UpdateDownloadFiles: - Saving results to file");
                string newFilePath =
                    Global.FileRepository.GetFullPath(Path.Combine(downloadsLocation, $"GPGData_{year}-{year + 1}.csv"));
                try
                {
                    if (downloadData.Any())
                    {
                        await Global.FileRepository.SaveCSVAsync(downloadData, newFilePath, oldDownloadFilePath);
                    }
                    else if (!string.IsNullOrWhiteSpace(oldDownloadFilePath))
                    {
                        await Global.FileRepository.DeleteFileAsync(oldDownloadFilePath);
                    }
                }
                catch (Exception ex)
                {
                    log.LogError(ex, ex.Message);
                }
                CustomLogger.Information($"UpdateDownloadFiles: Done for year {year}");
            }

            CustomLogger.Information($"UpdateDownloadFiles: Done");
        }

    }
}
