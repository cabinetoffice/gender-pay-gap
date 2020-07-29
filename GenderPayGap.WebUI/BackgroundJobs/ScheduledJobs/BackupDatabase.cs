using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{
    public class BackupDatabase
    {

        private readonly IFileRepository fileRepository;

        public BackupDatabase(IFileRepository fileRepository)
        {
            this.fileRepository = fileRepository;
        }

        public void CreateDatabaseBackup()
        {
            if (Global.UsePostgresDb)
            {
                string runId = JobHelpers.CreateRunId();
                DateTime startTime = VirtualDateTime.Now;
                JobHelpers.LogFunctionStart(runId, nameof(BackupDatabase), startTime);

                try
                {
                    string tempFileName = $"pg_temp_backup_{startTime.ToString("yyyy-dd-M--HH-mm-ss")}.bak";
                    PostgreSqlDump(tempFileName);

                    string executableLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                    string tempFilePath = Path.Combine(executableLocation, tempFileName);
                    string filePath = Path.Combine(Global.DatabaseBackupsLocation, $"GPG_database_backup_{startTime.ToString("yyyy-dd-M--HH-mm-ss")}.bak");
                    fileRepository.Write(filePath, File.ReadAllText(tempFilePath));

                    File.Delete(tempFilePath);

                    JobHelpers.LogFunctionEnd(runId, nameof(BackupDatabase), startTime);
                }
                catch (Exception ex)
                {
                    JobHelpers.LogFunctionError(runId, nameof(BackupDatabase), startTime, ex);
                    throw;
                }
            }
        }

        private void PostgreSqlDump(string outFile)
        {
            Environment.SetEnvironmentVariable("PGPASSWORD", DatabasePassword());

            string dumpCommand = "pg_executable\\pg_dump -Fc"
                                 + " -h "
                                 + DatabaseHost()
                                 + " -p "
                                 + DatabasePort()
                                 + " -d "
                                 + DatabaseName()
                                 + " -U "
                                 + DatabaseUser();
            string batchContent = dumpCommand + " > " + "\"" + outFile + "\"" + "\n";

            if (File.Exists(outFile))
            {
                File.Delete(outFile);
            }

            Execute(batchContent).Wait();
        }

        private string DatabaseHost()
        {
            return Regex.Match(Global.DatabaseConnectionString, @"Server=(.+?);").Groups[1].Value;
        }

        private string DatabasePort()
        {
            return Regex.Match(Global.DatabaseConnectionString, @"Port=(.+?);").Groups[1].Value;
        }

        private string DatabaseName()
        {
            return Regex.Match(Global.DatabaseConnectionString, @"Database=(.+?);").Groups[1].Value;
        }

        private string DatabaseUser()
        {
            return Regex.Match(Global.DatabaseConnectionString, @"User Id=(.+?);").Groups[1].Value;
        }

        private string DatabasePassword()
        {
            return Regex.Match(Global.DatabaseConnectionString, @"Password=(.+?);").Groups[1].Value;
        }

        private Task Execute(string dumpCommand)
        {
            return Task.Run(
                () =>
                {
                    string batFilePath = Path.Combine(
                        Path.GetTempPath(),
                        $"{Guid.NewGuid()}." + (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "bat" : "sh"));
                    try
                    {
                        var batchContent = "";
                        batchContent += $"{dumpCommand}";

                        File.WriteAllText(batFilePath, batchContent, Encoding.ASCII);
                        ProcessStartInfo info = ProcessInfoByOS(batFilePath);
                        Process proc = Process.Start(info);

                        proc.WaitForExit();
                        int exit = proc.ExitCode;
                        proc.Close();
                    }
                    catch (Exception e)
                    {
                        CustomLogger.Error(e.Message);
                    }
                    finally
                    {
                        if (File.Exists(batFilePath))
                        {
                            File.Delete(batFilePath);
                        }
                    }
                });
        }

        private static ProcessStartInfo ProcessInfoByOS(string batFilePath)
        {
            ProcessStartInfo info;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                info = new ProcessStartInfo(batFilePath);
            }
            else
            {
                info = new ProcessStartInfo("sh") {Arguments = $"{batFilePath}"};
            }

            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
            info.RedirectStandardError = true;
            info.RedirectStandardOutput = true;

            return info;
        }

    }
}
