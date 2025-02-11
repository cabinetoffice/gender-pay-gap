using System.Collections.Concurrent;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.WebUI.BackgroundJobs {

    internal enum JobErrorsLogged
    {
        Automatically,
        Manually
    }

    internal static class JobHelpers {

        private static readonly ConcurrentDictionary<string, string> ActionsCurrentlyRunning = [];

        public static void RunAndLogSingletonJob(Action action, string actionName, JobErrorsLogged logErrors = JobErrorsLogged.Automatically)
        {
            RunAndLogSingletonJob(unusedRunId => action(), actionName, logErrors);
        }

        public static void RunAndLogSingletonJob(Action<string> action, string actionName, JobErrorsLogged logErrors = JobErrorsLogged.Automatically)
        {
            if (ActionsCurrentlyRunning.TryAdd(actionName, ""))
            {
                try
                {
                    RunAndLogJob(action, actionName, logErrors);
                }
                finally
                {
                    ActionsCurrentlyRunning.Remove(actionName, out _);
                }
            }
            else
            {
                CustomLogger.Information($"Function already running: {actionName}", new {environment = Config.EnvironmentName});
            }
        }
        
        public static void RunAndLogJob(Action action, string actionName, JobErrorsLogged logErrors = JobErrorsLogged.Automatically)
        {
            RunAndLogJob(_ => action(), actionName, logErrors);
        }

        public static void RunAndLogJob(Action<string> action, string actionName, JobErrorsLogged logErrors = JobErrorsLogged.Automatically)
        {
            string runId = CreateRunId();
            DateTime startTime = VirtualDateTime.Now;
            LogFunctionStart(runId, actionName, startTime);
            try
            {
                action(runId);

                LogFunctionEnd(runId, actionName, startTime);
            }
            catch (Exception ex)
            {
                if (logErrors == JobErrorsLogged.Automatically)
                {
                    LogFunctionError(runId, actionName, startTime, ex);
                }

                //Rethrow the error
                throw;
            }
        }

        private static string CreateRunId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        private static void LogFunctionStart(string runId, string functionName, DateTime startTime)
        {
            CustomLogger.Information($"Function started: {functionName}", new {runId, environment = Config.EnvironmentName, startTime});
        }

        private static void LogFunctionEnd(string runId, string functionName, DateTime startTime)
        {
            DateTime endTime = VirtualDateTime.Now;
            CustomLogger.Information(
                $"Function finished: {functionName}",
                new {runId, Environment = Config.EnvironmentName, endTime, TimeTakenInSeconds = (endTime - startTime).TotalSeconds});
        }

        private static void LogFunctionError(string runId, string functionName, DateTime startTime, Exception ex)
        {
            DateTime endTime = VirtualDateTime.Now;
            CustomLogger.Error(
                $"Function failed: {functionName}",
                new
                {
                    runId,
                    environment = Config.EnvironmentName,
                    endTime,
                    TimeTakenToErrorInSeconds = (endTime - startTime).TotalSeconds,
                    Exception = ex
                });
        }

    }
}