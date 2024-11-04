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
using GenderPayGap.Database;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Download;
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
            List<Organisation> nonDeletedOrganisations = dataRepository.GetAll<Organisation>()
                .Where(o => o.Status != OrganisationStatuses.Deleted)
                .Include(o => o.OrganisationNames)
                .Include(o => o.OrganisationAddresses)
                .Include(o => o.OrganisationSicCodes)
                .ToList();

            CustomLogger.Information($"UpdateDownloadFiles: Loading Returns");
            List<Return> allReturns = dataRepository.GetAll<Return>()
                .Where(r => r.Organisation.Status != OrganisationStatuses.Deleted)
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
                    .Select(r => ToDownloadResult(r))
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

        private static DownloadResult ToDownloadResult(Return ret)
        {
            return new DownloadResult {
                EmployerName = ret.Organisation.GetName(ret.StatusDate)?.Name ?? ret.Organisation.OrganisationName,
                EmployerId = ret.OrganisationId,
                Address = ret.Organisation.GetLatestAddress()?.GetAddressString(),
                PostCode = ret.Organisation.GetLatestAddress()?.GetPostCodeInAllCaps(),
                CompanyNumber = ret.Organisation?.CompanyNumber,
                SicCodes = ret.Organisation?.GetSicCodeIdsString(ret.StatusDate, "," + Environment.NewLine),
                DiffMeanHourlyPercent = ret.DiffMeanHourlyPayPercent,
                DiffMedianHourlyPercent = ret.DiffMedianHourlyPercent,
                DiffMeanBonusPercent = ret.DiffMeanBonusPercent,
                DiffMedianBonusPercent = ret.DiffMedianBonusPercent,
                MaleBonusPercent = ret.MaleMedianBonusPayPercent,
                FemaleBonusPercent = ret.FemaleMedianBonusPayPercent,
                MaleLowerQuartile = ret.MaleLowerPayBand,
                FemaleLowerQuartile = ret.FemaleLowerPayBand,
                MaleLowerMiddleQuartile = ret.MaleMiddlePayBand,
                FemaleLowerMiddleQuartile = ret.FemaleMiddlePayBand,
                MaleUpperMiddleQuartile = ret.MaleUpperPayBand,
                FemaleUpperMiddleQuartile = ret.FemaleUpperPayBand,
                MaleTopQuartile = ret.MaleUpperQuartilePayBand,
                FemaleTopQuartile = ret.FemaleUpperQuartilePayBand,
                CompanyLinkToGPGInfo = ret.CompanyLinkToGPGInfo,
                ResponsiblePerson = ret.ResponsiblePerson,
                EmployerSize = ret.OrganisationSize.GetDisplayName(),
                CurrentName = ret.Organisation?.OrganisationName,
                SubmittedAfterTheDeadline = ret.IsLateSubmission,
                DueDate = ret.GetDueDate(),
                DateSubmitted = ret.Modified
            };
        }

    }
}
