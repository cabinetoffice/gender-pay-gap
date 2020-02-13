using System;
using System.Linq;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.WebJobs;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        [Singleton(Mode = SingletonMode.Listener)]
        public void FetchCompaniesHouseData([TimerTrigger("*/5 * * * *")] TimerInfo timer)
        {
            var runId = CreateRunId();
            var startTime = VirtualDateTime.Now;
            LogFunctionStart(runId,  nameof(FetchCompaniesHouseData), startTime);
            
            try
            {
                UpdateFromCompaniesHouse(runId);
                
                LogFunctionEnd(runId, nameof(FetchCompaniesHouseData), startTime);
            }
            catch (Exception ex)
            {
                LogFunctionError(runId, nameof(FetchCompaniesHouseData), startTime, ex );
                throw;
            }
        }

        private void UpdateFromCompaniesHouse(string runId)
        {
            int maxNumCallCompaniesHouseApi = Config.GetAppSetting("MaxNumCallsCompaniesHouseApiPerFiveMins").ToInt32(500);

            for (var i = 0; i < maxNumCallCompaniesHouseApi; i++)
            {
                long organisationId = _DataRepository.GetAll<Organisation>()
                    .Where(org => !org.OptedOutFromCompaniesHouseUpdate && org.CompanyNumber != null && org.CompanyNumber != "")
                    .OrderByDescending(org => org.LastCheckedAgainstCompaniesHouse == null)
                    .ThenBy(org => org.LastCheckedAgainstCompaniesHouse)
                    .Select(org => org.OrganisationId)
                    .FirstOrDefault();

                CustomLogger.Information($"Start update companies house data organisation id: {organisationId}. Run id: {runId}");
                _updateFromCompaniesHouseService.UpdateOrganisationDetails(organisationId);
                CustomLogger.Information($"End update companies house data organisation id: {organisationId}. Run id: {runId}");
            }
        }

    }
}
