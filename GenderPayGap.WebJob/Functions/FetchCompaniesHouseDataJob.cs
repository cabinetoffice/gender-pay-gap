using System;
using System.Linq;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;

namespace GenderPayGap.WebJob
{
    public class FetchCompaniesHouseDataJob
    {
        private readonly IDataRepository dataRepository;
        private readonly UpdateFromCompaniesHouseService updateFromCompaniesHouseService;

        public FetchCompaniesHouseDataJob(
            IDataRepository dataRepository,
            UpdateFromCompaniesHouseService updateFromCompaniesHouseService)
        {
            this.dataRepository = dataRepository;
            this.updateFromCompaniesHouseService = updateFromCompaniesHouseService;
        }


        [Singleton(Mode = SingletonMode.Listener)]
        public void FetchCompaniesHouseData([TimerTrigger("*/5 * * * *" /* evry 5 minutes */)] TimerInfo timer)
        {
            var runId = JobHelpers.CreateRunId();
            var startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId,  nameof(FetchCompaniesHouseData), startTime);
            
            try
            {
                UpdateFromCompaniesHouse(runId);

                JobHelpers.LogFunctionEnd(runId, nameof(FetchCompaniesHouseData), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(FetchCompaniesHouseData), startTime, ex );
                throw;
            }
        }

        private void UpdateFromCompaniesHouse(string runId)
        {
            int maxNumCallCompaniesHouseApi = Config.GetAppSetting("MaxNumCallsCompaniesHouseApiPerFiveMins").ToInt32(500);

            for (var i = 0; i < maxNumCallCompaniesHouseApi; i++)
            {
                long organisationId = dataRepository.GetAll<Organisation>()
                    .Where(org => !org.OptedOutFromCompaniesHouseUpdate && org.CompanyNumber != null && org.CompanyNumber != "")
                    .OrderByDescending(org => org.LastCheckedAgainstCompaniesHouse == null)
                    .ThenBy(org => org.LastCheckedAgainstCompaniesHouse)
                    .Select(org => org.OrganisationId)
                    .FirstOrDefault();

                CustomLogger.Information($"Start update companies house data organisation id: {organisationId}. Run id: {runId}");
                updateFromCompaniesHouseService.UpdateOrganisationDetails(organisationId);
                CustomLogger.Information($"End update companies house data organisation id: {organisationId}. Run id: {runId}");
            }
        }

    }
}
