using System;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        public Functions(
            IMessenger messenger,
            IDataRepository dataRepository,
            IScopeBusinessLogic scopeBL,
            ISearchBusinessLogic searchBusinessLogic,
            IGovNotifyAPI govNotifyApi,
            UpdateFromCompaniesHouseService updateFromCompaniesHouseService)
        {
            _Messenger = messenger;
            _DataRepository = dataRepository;
            _ScopeBL = scopeBL;
            _SearchBusinessLogic = searchBusinessLogic;
            _updateFromCompaniesHouseService = updateFromCompaniesHouseService;
            this.govNotifyApi = govNotifyApi;
        }

        #region Properties

        public readonly IMessenger _Messenger;
        public readonly IDataRepository _DataRepository;
        private readonly IScopeBusinessLogic _ScopeBL;
        private readonly ISearchBusinessLogic _SearchBusinessLogic;
        private readonly IGovNotifyAPI govNotifyApi;
        private readonly UpdateFromCompaniesHouseService _updateFromCompaniesHouseService;

        private static readonly ConcurrentSet<string> RunningJobs = new ConcurrentSet<string>();

        #endregion

        #region Function Logging

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

        #endregion

    }
}
