using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
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
            JobHelpers.RunAndLogJob(UpdateDownloadFilesAction, nameof(UpdateDownloadFiles));
        }

        private void UpdateDownloadFilesAction()
        {
            CustomLogger.Information($"UpdateDownloadFiles: Loading Organisations");
            // IMPORTANT: This variable isn't used, but running this query makes the next query much faster
            List<Organisation> activeOrganisations = dataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active)
                .Include(o => o.OrganisationNames)
                .Include(o => o.OrganisationAddresses)
                .Include(o => o.OrganisationSicCodes)
                .ToList();

            CustomLogger.Information($"UpdateDownloadFiles: Loading Returns");
            List<Return> allReturns = dataRepository.GetAll<Return>()
                .Where(r => r.Organisation.Status == OrganisationStatuses.Active)
                .Include(r => r.Organisation)
                .ToList();

            CustomLogger.Information($"UpdateDownloadFiles: Creating downloads for each year");
            foreach (int year in ReportingYearsHelper.GetReportingYears())
            {
                CustomLogger.Information($"UpdateDownloadFiles: Creating download for year {year}");

                CustomLogger.Information($"UpdateDownloadFiles: - Filtering Returns");
                List<Return> returns = allReturns
                    .Where(r => r.AccountingDate.Year == year)
                    .Where(r => r.Status == ReturnStatuses.Submitted)
                    .ToList();

                CustomLogger.Information($"UpdateDownloadFiles: - Converting Returns into results");
                List<DownloadResult> downloadData = returns.ToList()
                    .Select(r => r.ToDownloadResult())
                    .OrderBy(d => d.EmployerName)
                    .ToList();

                CustomLogger.Information($"UpdateDownloadFiles: - Saving results to file");
                string filePath = GetDownloadFileLocationForYear(year);

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

        public static string GetDownloadFileLocationForYear(int year)
        {
            string fileName = $"GPGData_{year}-{year + 1}.csv";
            string filePath = Path.Combine(Global.DownloadsLocation, fileName);
            return filePath;
        }

        private void SaveCsvFile(IEnumerable records, string relativeFilePath)
        {
            CsvWriter.Write<object>(
                (memoryStream, streamReader, streamWriter, csvWriter) =>
                {
                    csvWriter.Configuration.ShouldQuote = (s, context) => true;
                    csvWriter.Configuration.TrimOptions = TrimOptions.InsideQuotes;

                    var options = new TypeConverterOptions {Formats = new[] {"yyyy/MM/dd HH:mm:ss"}};
                    csvWriter.Configuration.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                    csvWriter.Configuration.TypeConverterOptionsCache.AddOptions<DateTime?>(options);

                    csvWriter.WriteRecords(records);
                    streamWriter.Flush();
                    memoryStream.Position = 0;

                    string csvFileContents = streamReader.ReadToEnd();

                    // Save CSV to storage
                    fileRepository.Write(relativeFilePath, csvFileContents);

                    return null;
                });
        }

    }
}
