using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Helpers;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class UpdatePublicFacingDownloadFilesJob
    {
        private readonly IDataRepository dataRepository;
        private readonly IFileRepository fileRepository;

        public UpdatePublicFacingDownloadFilesJob(
            IDataRepository dataRepository,
            IFileRepository fileRepository)
        {
            this.dataRepository = dataRepository;
            this.fileRepository = fileRepository;
        }


        public void UpdateDownloadFiles()
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(UpdateDownloadFiles), startTime);

            try
            {
                UpdateDownloadFilesInner();
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(UpdateDownloadFiles), startTime, ex );
          
                //Rethrow the error
                throw;
            }

            JobHelpers.LogFunctionEnd(runId, nameof(UpdateDownloadFiles), startTime);
        }

        public void UpdateDownloadFilesInner()
        {
            CustomLogger.Information($"UpdateDownloadFiles: Loading Returns");
            List<Organisation> activeOrganisations = dataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Include(o => o.OrganisationNames)
                .Include(o => o.OrganisationAddresses)
                .Include(o => o.OrganisationSicCodes)
                .Include(o => o.Returns)
                .ToList();

            foreach (int year in ReportingYearsHelper.GetReportingYears())
            {
                CustomLogger.Information($"UpdateDownloadFiles: Creating download for year {year}");

                CustomLogger.Information($"UpdateDownloadFiles: - Filtering Returns");
                List<Return> returns = activeOrganisations.SelectMany(o => o.Returns)
                    .Where(r => r.AccountingDate.Year == year)
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .ToList();

                CustomLogger.Information($"UpdateDownloadFiles: - Converting Returns into results");
                List<DownloadResult> downloadData = returns.ToList()
                    .Select(r => r.ToDownloadResult())
                    .OrderBy(d => d.EmployerName)
                    .ToList();

                CustomLogger.Information($"UpdateDownloadFiles: - Saving results to file");
                string filePath = Path.Combine(Global.DownloadsLocation, $"GPGData_{year}-{year + 1}.csv");

                try
                {
                    SaveCsvFile(downloadData, filePath);
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex.Message, new {Error = ex});
                }
                CustomLogger.Information($"UpdateDownloadFiles: Done for year {year}");
            }

            CustomLogger.Information($"UpdateDownloadFiles: Done");
        }

        private void SaveCsvFile(IEnumerable records, string relativeFilePath)
        {
            var csvConfiguration = new CsvConfiguration(CultureInfo.CurrentCulture) { ShouldQuote = (s, context) => true, TrimOptions = TrimOptions.InsideQuotes };

            using (var memoryStream = new MemoryStream())
            using (var streamReader = new StreamReader(memoryStream))
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter, csvConfiguration))
            {
                // Create CSV file (as string)
                csvWriter.WriteRecords(records);
                streamWriter.Flush();
                memoryStream.Position = 0;
                string csvFileContents = streamReader.ReadToEnd();

                //Save CSV to storage
                fileRepository.Write(relativeFilePath, csvFileContents);
            }
        }

    }
}
