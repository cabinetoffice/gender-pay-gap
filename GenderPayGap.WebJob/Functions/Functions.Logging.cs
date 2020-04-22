using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GenderPayGap.WebJob
{

    public partial class Functions
    {

        private static readonly ConcurrentDictionary<string, SemaphoreSlim> FileLocks =
            new ConcurrentDictionary<string, SemaphoreSlim>(StringComparer.OrdinalIgnoreCase);

        [Singleton(Mode = SingletonMode.Listener)] //Ensures execution on only one instance with one listener
        public async Task LogEvent([QueueTrigger(QueueNames.LogEvent)] string queueMessage, ILogger log)
        {
            string runId = JobHelpers.CreateRunId();
            DateTime startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(LogEvent), startTime);

            //Retrieve long messages from file storage
            string filepath = GetLargeQueueFilepath(queueMessage);
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                queueMessage = await Global.FileRepository.ReadAsync(filepath);
            }

            var wrapper = JsonConvert.DeserializeObject<LogEventWrapperModel>(queueMessage);

            //Calculate the daily log file path
            string LogRoot = Config.GetAppSetting("LogPath");
            string FilePath;
            switch (wrapper.LogLevel)
            {
                case LogLevel.Trace:
                    FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, "TraceLog.csv");
                    break;
                case LogLevel.Debug:
                    FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, "DebugLog.csv");
                    break;
                case LogLevel.Information:
                    FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, "InfoLog.csv");
                    break;
                case LogLevel.Warning:
                    FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, "WarningLog.csv");
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, "ErrorLog.csv");
                    break;
                default:
                    throw new ArgumentException("Invalid Log Level", nameof(LogLevel));
            }

            string DailyPath = Path.Combine(
                Path.GetPathRoot(FilePath),
                Path.GetDirectoryName(FilePath),
                Path.GetFileNameWithoutExtension(FilePath) + "_" + VirtualDateTime.Now.ToString("yyMMdd") + Path.GetExtension(FilePath));

            if (!FileLocks.ContainsKey(DailyPath))
            {
                FileLocks[DailyPath] = new SemaphoreSlim(1, 1);
            }

            SemaphoreSlim fileLock = FileLocks[DailyPath];

            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            await fileLock.WaitAsync();
            try
            {
                //Write to the log entry
                await Global.FileRepository.AppendCsvRecordAsync(DailyPath, wrapper.LogEntry);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                fileLock.Release();
            }


            //Delete the large file
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                await Global.FileRepository.DeleteFileAsync(filepath);
            }

            DateTime endTime = VirtualDateTime.Now;
            CustomLogger.Information(
                $"Function finished: {nameof(LogEvent)}. {queueMessage} successfully",
                new {runId, Environment = Config.EnvironmentName, endTime, TimeTakenInSeconds = (endTime - startTime).TotalSeconds});
        }

        public async Task LogEventPoison([QueueTrigger(QueueNames.LogEvent + "-poison")]
            string queueMessage,
            ILogger log)
        {
            //Retrieve long messages from file storage
            string filepath = GetLargeQueueFilepath(queueMessage);
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                //Get the large file
                queueMessage = await Global.FileRepository.ReadAsync(filepath);
                //Delete the large file
                await Global.FileRepository.DeleteFileAsync(filepath);
            }

            log.LogError($"Could not log event, Details: {queueMessage}");

            DateTime time = VirtualDateTime.Now;
            CustomLogger.Error(
                $"Function failed: {nameof(LogEventPoison)}",
                new {environment = Config.EnvironmentName, time, queueMessage});
        }

        [Singleton(Mode = SingletonMode.Listener)] //Ensures execution on only one instance with one listener
        public async Task LogRecord([QueueTrigger(QueueNames.LogRecord)] string queueMessage, ILogger log)
        {
            string runId = JobHelpers.CreateRunId();
            DateTime startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(LogRecord), startTime);

            //Retrieve long messages from file storage
            string filepath = GetLargeQueueFilepath(queueMessage);
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                queueMessage = await Global.FileRepository.ReadAsync(filepath);
            }

            //Get the log event details
            var wrapper = JsonConvert.DeserializeObject<LogRecordWrapperModel>(queueMessage);

            //Calculate the daily log file path
            string LogRoot = Config.GetAppSetting("LogPath");
            string FilePath = Path.Combine(LogRoot, wrapper.ApplicationName, wrapper.FileName);
            string DailyPath = Path.Combine(
                Path.GetPathRoot(FilePath),
                Path.GetDirectoryName(FilePath),
                Path.GetFileNameWithoutExtension(FilePath) + "_" + VirtualDateTime.Now.ToString("yyMMdd") + Path.GetExtension(FilePath));

            if (!FileLocks.ContainsKey(DailyPath))
            {
                FileLocks[DailyPath] = new SemaphoreSlim(1, 1);
            }

            SemaphoreSlim fileLock = FileLocks[DailyPath];

            //Asynchronously wait to enter the Semaphore. If no-one has been granted access to the Semaphore, code execution will proceed, otherwise this thread waits here until the semaphore is released 
            await fileLock.WaitAsync();
            try
            {
                FileLocks[DailyPath] = fileLock;

                //Write to the log entry
                await Global.FileRepository.AppendCsvRecordAsync(DailyPath, wrapper.Record);
            }
            finally
            {
                //When the task is ready, release the semaphore. It is vital to ALWAYS release the semaphore when we are ready, or else we will end up with a Semaphore that is forever locked.
                //This is why it is important to do the Release within a try...finally clause; program execution may crash or take a different path, this way you are guaranteed execution
                fileLock.Release();
            }

            //Delete the large file
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                await Global.FileRepository.DeleteFileAsync(filepath);
            }

            DateTime endTime = VirtualDateTime.Now;
            CustomLogger.Information(
                $"Function finished: {nameof(LogRecord)}. {queueMessage} successfully",
                new {runId, Environment = Config.EnvironmentName, endTime, TimeTakenInSeconds = (endTime - startTime).TotalSeconds});
        }

        public async Task LogRecordPoison([QueueTrigger(QueueNames.LogRecord + "-poison")]
            string queueMessage,
            ILogger log)
        {
            //Retrieve long messages from file storage
            string filepath = GetLargeQueueFilepath(queueMessage);
            if (!string.IsNullOrWhiteSpace(filepath))
            {
                //Get the large file
                queueMessage = await Global.FileRepository.ReadAsync(filepath);
                //Delete the large file
                await Global.FileRepository.DeleteFileAsync(filepath);
            }

            log.LogError($"Could not log record: Details:{queueMessage}");

            DateTime time = VirtualDateTime.Now;
            CustomLogger.Error(
                $"Function failed: {nameof(LogRecordPoison)}",
                new {environment = Config.EnvironmentName, time, queueMessage});
        }

        private static string GetLargeQueueFilepath(string queueMessage)
        {
            if (!queueMessage.StartsWith("file:"))
            {
                return null;
            }

            string filepath = queueMessage.AfterFirst("file:", includeWhenNoSeparator: false);
            return filepath;
        }

    }

}
