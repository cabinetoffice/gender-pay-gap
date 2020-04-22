using System;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.WebUI.BackgroundJobs {

    internal static class JobHelpers {

        public static string CreateRunId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        public static void LogFunctionStart(string runId, string functionName, DateTime startTime)
        {
            CustomLogger.Information($"Function started: {functionName}", new {runId, environment = Config.EnvironmentName, startTime});
        }

        public static void LogFunctionEnd(string runId, string functionName, DateTime startTime)
        {
            DateTime endTime = VirtualDateTime.Now;
            CustomLogger.Information(
                $"Function finished: {functionName}",
                new {runId, Environment = Config.EnvironmentName, endTime, TimeTakenInSeconds = (endTime - startTime).TotalSeconds});
        }

        public static void LogFunctionError(string runId, string functionName, DateTime startTime, Exception ex)
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