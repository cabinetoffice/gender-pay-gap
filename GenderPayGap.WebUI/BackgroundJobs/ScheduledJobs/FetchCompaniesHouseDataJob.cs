using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Services;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
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


        public void FetchCompaniesHouseData()
        {
            JobHelpers.RunAndLogSingletonJob(UpdateFromCompaniesHouse, nameof(FetchCompaniesHouseData));
        }

        private void UpdateFromCompaniesHouse(string runId)
        {
            int maxNumCallCompaniesHouseApi = Global.MaxNumCallsCompaniesHouseApiPerFiveMins;

            for (var i = 0; i < maxNumCallCompaniesHouseApi; i++)
            {
                long organisationId = dataRepository.GetAll<Organisation>()
                    .Where(org => !org.OptedOutFromCompaniesHouseUpdate && org.CompanyNumber != null && org.CompanyNumber != "")
                    .OrderByDescending(org => org.LastCheckedAgainstCompaniesHouse == null)
                    .ThenBy(org => org.LastCheckedAgainstCompaniesHouse)
                    .Select(org => org.OrganisationId)
                    .FirstOrDefault();

                if (organisationId != 0)
                {
                    CustomLogger.Information($"Start update companies house data organisation id: {organisationId}. Run id: {runId}");
                    updateFromCompaniesHouseService.UpdateOrganisationDetails(organisationId);
                    CustomLogger.Information($"End update companies house data organisation id: {organisationId}. Run id: {runId}");
                }
            }
        }

    }
}
