using System.Globalization;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Helpers;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class BackupDatabaseToJsonFileJob
    {

        private const string BackupDirectory = "database-backup-json-files";
        private const string BackupFileNamePrefix = "database-backup-json-";
        private const string BackupFileNameSuffix = ".json.gz";

        private readonly IDataRepository dataRepository;
        private readonly IFileRepository fileRepository;

        public BackupDatabaseToJsonFileJob(
            IDataRepository dataRepository,
            IFileRepository fileRepository)
        {
            this.dataRepository = dataRepository;
            this.fileRepository = fileRepository;
        }


        public void RunBackup()
        {
            JobHelpers.RunAndLogJob(RunBackupAction, nameof(RunBackup));
        }

        private void RunBackupAction()
        {
            BackupDatabaseToJsonFile();
            DeleteOldJsonFiles();
        }

        private void BackupDatabaseToJsonFile()
        {
            string allDataString = BackupHelper.LoadAllDataFromDatabaseInfoJsonString(dataRepository);
            byte[] allDataZippedBytes = BackupHelper.Zip(allDataString);

            string currentDateTime = VirtualDateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss");
            string fileName = $"{BackupFileNamePrefix}{currentDateTime}{BackupFileNameSuffix}";
            string relativeFilePath = $"{BackupDirectory}/{fileName}";

            fileRepository.Write(relativeFilePath, allDataZippedBytes);
        }

        private void DeleteOldJsonFiles()
        {
            DateTime oldestAllowedBackupFile = VirtualDateTime.Now.Subtract(Global.TimeToKeepBackupFiles);

            List<string> fileNames = fileRepository.GetFiles(BackupDirectory);

            foreach (string fileName in fileNames)
            {
                if (fileName.StartsWith(BackupFileNamePrefix) && fileName.EndsWith(BackupFileNameSuffix))
                {
                    string fileNameDateString = fileName.Substring(
                        BackupFileNamePrefix.Length,
                        fileName.Length - BackupFileNamePrefix.Length - BackupFileNameSuffix.Length);

                    if (DateTime.TryParseExact(
                        fileNameDateString,
                        "yyyy-MM-ddTHH-mm-ss",
                        CultureInfo.CurrentCulture,
                        DateTimeStyles.None,
                        out DateTime fileNameDate))
                    {
                        if (fileNameDate < oldestAllowedBackupFile)
                        {
                            string relativeFilePath = $"{BackupDirectory}/{fileName}";
                            fileRepository.Delete(relativeFilePath);
                        }
                    }
                }
            }
        }

    }
}
