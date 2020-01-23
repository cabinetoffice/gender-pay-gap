using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        //Merge all event log files from all instances into 1 single file per month
        public async Task MergeLogs([TimerTrigger("01:00:00:00", RunOnStartup = true)]
            TimerInfo timer,
            ILogger log)
        {
            try
            {
                //Backup the log files first
                await ArchiveAzureStorageAsync();

                var actions = new List<Task>();

                #region WebServer Logs

                string webServerlogPath = Path.Combine(Global.LogPath, "GenderPayGap.WebUI");
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webServerlogPath, "ErrorLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webServerlogPath, "DebugLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webServerlogPath, "WarningLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webServerlogPath, "InfoLog"));
                actions.Add(MergeCsvLogsAsync<BadSicLogModel>(log, webServerlogPath, "BadSicLog"));
                actions.Add(MergeCsvLogsAsync<SubmissionLogModel>(log, webServerlogPath, "SubmissionLog"));
                actions.Add(MergeCsvLogsAsync<RegisterLogModel>(log, webServerlogPath, "RegistrationLog"));
                actions.Add(MergeCsvLogsAsync<SearchLogModel>(log, webServerlogPath, "SearchLog"));
                actions.Add(MergeCsvLogsAsync<UserLogModel>(log, webServerlogPath, "UserLog"));

                #endregion

                #region IdentityServer Logs

                string identityServerlogPath = Path.Combine(Global.LogPath, "GenderPayGap.IdentityServer4");
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, identityServerlogPath, "ErrorLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, identityServerlogPath, "DebugLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, identityServerlogPath, "WarningLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, identityServerlogPath, "InfoLog"));

                #endregion

                #region Webjob Logs

                string webJoblogPath = Path.Combine(Global.LogPath, "GenderPayGap.WebJob");
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webJoblogPath, "ErrorLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webJoblogPath, "DebugLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webJoblogPath, "WarningLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webJoblogPath, "InfoLog"));
                actions.Add(MergeCsvLogsAsync<EmailSendLogModel>(log, webJoblogPath, "EmailSendLog"));
                actions.Add(MergeCsvLogsAsync<LogEntryModel>(log, webJoblogPath, "StannpSendLog"));
                actions.Add(MergeCsvLogsAsync<ManualChangeLogModel>(log, webJoblogPath, "ManualChangeLog"));

                #endregion

                await Task.WhenAll(actions);

                log.LogDebug($"Executed {nameof(MergeLogs)} successfully");
            }
            catch (Exception ex)
            {
                string message = $"Failed webjob ({nameof(MergeLogs)}):{ex.Message}:{ex.GetDetailsText()}";

                //Send Email to GEO reporting errors
                await _Messenger.SendGeoMessageAsync("GPG - WEBJOBS ERROR", message);
                //Rethrow the error
                throw;
            }
        }

        private static async Task MergeCsvLogsAsync<T>(ILogger log, string logPath, string prefix, string extension = ".csv")
        {
            //Get all the daily log files
            IEnumerable<string> files = await Global.FileRepository.GetFilesAsync(logPath, $"{prefix}_*{extension}");
            List<string> fileList = files.OrderBy(o => o).ToList();

            //Get all files before today
            DateTime startDate = VirtualDateTime.Now.Date;

            foreach (string file in fileList)
            {
                try
                {
                    //Get the date from this daily log filename
                    string dateSuffix = Path.GetFileNameWithoutExtension(file).AfterFirst("_");
                    if (string.IsNullOrWhiteSpace(dateSuffix) || dateSuffix.Length < 6)
                    {
                        continue;
                    }

                    DateTime date = dateSuffix.FromShortestDateTime();

                    //Ignore log files with no date in the filename
                    if (date == DateTime.MinValue)
                    {
                        continue;
                    }

                    //Ignore todays daily log file 
                    if (date.Date >= startDate)
                    {
                        continue;
                    }

                    //Get the monthly log file for this files date
                    string monthLog = Path.Combine(logPath, $"{prefix}_{date:yyMM}{extension}");

                    //Read all the records from this daily log file
                    List<T> records = await Global.FileRepository.ReadCSVAsync<T>(file);

                    //Add the records to its monthly log file
                    await Global.FileRepository.AppendCsvRecordsAsync(monthLog, records);

                    //Delete this daily log file
                    await Global.FileRepository.DeleteFileAsync(file);
                }
                catch (Exception ex)
                {
                    string message =
                        $"ERROR: Webjob ({nameof(MergeLogs)}) could not merge file '{file}':{ex.Message}:{ex.GetDetailsText()}";
                    log.LogError(ex, message);
                }
            }

            DateTime archiveDeadline = DateTime.Now.AddYears(-1).Date;

            foreach (string file in fileList)
            {
                try
                {
                    //Get the date from this daily log filename
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string dateSuffix = fileName.AfterFirst("_");
                    if (string.IsNullOrWhiteSpace(dateSuffix))
                    {
                        continue;
                    }

                    if (dateSuffix.Length != 4)
                    {
                        continue;
                    }

                    //Get the date of this log from the filename
                    int year = dateSuffix.Substring(0, 2).ToInt32().ToFourDigitYear();
                    int month = dateSuffix.Substring(2, 2).ToInt32();
                    var logDate = new DateTime(year, month, 1);

                    //Dont archive logs newer than 1 year
                    if (logDate >= archiveDeadline)
                    {
                        continue;
                    }

                    string archivePath = Path.Combine(logPath, year.ToString());
                    if (!await Global.FileRepository.GetDirectoryExistsAsync(archivePath))
                    {
                        await Global.FileRepository.CreateDirectoryAsync(archivePath);
                    }

                    //Ensure we have a unique filename
                    string ext = Path.GetExtension(file);
                    string archiveFilePath = Path.Combine(archivePath, fileName) + ext;

                    var c = 0;
                    while (await Global.FileRepository.GetFileExistsAsync(archiveFilePath))
                    {
                        c++;
                        archiveFilePath = Path.Combine(archivePath, fileName) + $" ({c}){ext}";
                    }

                    //Copy to the archive folder
                    await Global.FileRepository.CopyFileAsync(file, archiveFilePath, false);

                    //Delete the old file
                    if (await Global.FileRepository.GetFileExistsAsync(archiveFilePath))
                    {
                        await Global.FileRepository.DeleteFileAsync(file);
                    }
                }
                catch (Exception ex)
                {
                    string message =
                        $"ERROR: Webjob ({nameof(MergeLogs)}) could not archive file '{file}':{ex.Message}:{ex.GetDetailsText()}";
                    log.LogError(ex, message);
                }
            }
        }

    }
}
