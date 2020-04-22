using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        //Update the search indexes
        public async Task UpdateSearchAsync([TimerTrigger("20 * * * *" /* once per hour, at 20 minutes past the hour */)]
            TimerInfo timer,
            ILogger log)
        {
            string runId = JobHelpers.CreateRunId();
            DateTime startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(UpdateSearchAsync), startTime);

            try
            {
                await UpdateAllSearchIndexesAsync(log);
                JobHelpers.LogFunctionEnd(runId, nameof(UpdateSearchAsync), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(UpdateSearchAsync), startTime, ex);

                //Rethrow the error
                throw;
            }
        }

        private async Task UpdateAllSearchIndexesAsync(ILogger log, string userEmail = null, bool force = false)
        {
            log.LogInformation($"-- Started the updating of index {Global.SearchIndexName}");
            await UpdateSearchAsync(log, Global.SearchRepository, Global.SearchIndexName, userEmail, force);
            log.LogInformation($"-- Updating of index {Global.SearchIndexName} completed");
            log.LogInformation($"-- Started the updating of index {Global.SicCodesIndexName}");
            await UpdateSearchAsync(log, Global.SicCodeSearchRepository, Global.SicCodesIndexName, userEmail, force);
            log.LogInformation($"-- Updating of index {Global.SicCodesIndexName} completed");
        }

        public async Task UpdateSearchAsync(ILogger log, string userEmail = null, bool force = false)
        {
            await UpdateSearchAsync(log, Global.SearchRepository, Global.SearchIndexName, userEmail, force);
        }

        private async Task UpdateSearchAsync<T>(ILogger log,
            ISearchRepository<T> searchRepositoryToUpdate,
            string indexNameToUpdate,
            string userEmail = null,
            bool force = false)
        {
            if (RunningJobs.Contains(nameof(UpdateSearchAsync)))
            {
                log.LogInformation("The set of running jobs already contains 'UpdateSearch'");
                return;
            }

            try
            {
                await searchRepositoryToUpdate.CreateIndexIfNotExistsAsync(indexNameToUpdate);

                if (typeof(T) == typeof(EmployerSearchModel))
                {
                    await AddDataToIndexAsync(log);
                }
                else if (typeof(T) == typeof(SicCodeSearchModel))
                {
                    await AddDataToSicCodesIndexAsync(log);
                }
                else
                {
                    throw new ArgumentException($"Type {typeof(T)} is not a valid type.");
                }
            }
            finally
            {
                RunningJobs.Remove(nameof(UpdateSearchAsync));
            }
        }

        private async Task AddDataToSicCodesIndexAsync(ILogger log)
        {
            List<SicCodeSearchModel> listOfSicCodeRecords = await GetListOfSicCodeSearchModelsFromFileAsync(log);

            if (listOfSicCodeRecords == null || !listOfSicCodeRecords.Any())
            {
                log.LogInformation($"No records to be added to index {Global.SicCodesIndexName}");
                return;
            }

            await Global.SicCodeSearchRepository.RefreshIndexDataAsync(listOfSicCodeRecords);
        }

        private async Task AddDataToIndexAsync(ILogger log)
        {
            log.LogInformation("UpdateSearchAsync: Loading SIC codes from file");
            List<SicCodeSearchModel> listOfSicCodeRecords = await GetListOfSicCodeSearchModelsFromFileAsync(log);

            log.LogInformation("UpdateSearchAsync: Loading organisations");

            IQueryable<Organisation> organisations = _DataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active || o.Status == OrganisationStatuses.Retired)
                .Where(
                    o => o.Returns.Any(r => r.Status == ReturnStatuses.Submitted)
                         || o.OrganisationScopes.Any(
                             sc => sc.Status == ScopeRowStatuses.Active
                                   && (sc.ScopeStatus == ScopeStatuses.InScope || sc.ScopeStatus == ScopeStatuses.PresumedInScope)));

            //Remove the test organisations
            if (!string.IsNullOrWhiteSpace(Global.TestPrefix))
            {
                organisations = organisations.Where(o => !o.OrganisationName.StartsWithI(Global.TestPrefix));
            }

            organisations = organisations
                .Include(o => o.OrganisationNames)
                .Include(o => o.OrganisationSicCodes)
                .Include(o => o.Returns)
                .Include(o => o.OrganisationAddresses);

            List<Organisation> materialisedOrganisations = organisations.ToList();

            log.LogInformation("UpdateSearchAsync: Converting organisations to search results");
            List<EmployerSearchModel> selectionList =
                materialisedOrganisations.Select(o => o.ToEmployerSearchResult(false, listOfSicCodeRecords)).ToList();

            log.LogInformation("UpdateSearchAsync: Refreshing index");
            if (selectionList.Any())
            {
                await Global.SearchRepository.RefreshIndexDataAsync(selectionList);
            }

            log.LogInformation("UpdateSearchAsync: done with organisations");
        }

        private static async Task<List<SicCodeSearchModel>> GetListOfSicCodeSearchModelsFromFileAsync(ILogger log)
        {
            List<SicCodeSearchModel> listOfSicCodeRecords = null;

            try
            {
                IEnumerable<string> files = await Global.FileRepository.GetFilesAsync(Global.DataPath, Filenames.SicSectorSynonyms);
                string sicSectorSynonymsFilePath = files.OrderByDescending(f => f).FirstOrDefault();

                #region Pattern

                string rootDirMightIndicateCurrentLocation = string.IsNullOrEmpty(Global.FileRepository.RootDir)
                    ? "root"
                    : $"path {Global.FileRepository.RootDir}";
                string fileInPathMessage = $"pattern {Filenames.SicSectorSynonyms} in {rootDirMightIndicateCurrentLocation}.";
                if (string.IsNullOrEmpty(sicSectorSynonymsFilePath))
                {
                    string unableToFindPathMessage = $"Unable to find {fileInPathMessage}";
                    log.LogError(unableToFindPathMessage);
                    return null;
                }

                log.LogInformation($"Found {fileInPathMessage}");

                #endregion

                #region File exist

                bool fileExists = await Global.FileRepository.GetFileExistsAsync(sicSectorSynonymsFilePath);
                if (!fileExists)
                {
                    string fileDoesNotExistMessage = $"File does not exist {sicSectorSynonymsFilePath}.";
                    log.LogError(fileDoesNotExistMessage);
                    return null;
                }

                log.LogInformation($"File exists {sicSectorSynonymsFilePath}");

                #endregion

                List<SicCodeSearchModel> csv = await Global.FileRepository.ReadCSVAsync<SicCodeSearchModel>(sicSectorSynonymsFilePath);

                listOfSicCodeRecords = csv.OrderBy(o => o.SicCodeId).ToList();

                #region Records found

                if (!listOfSicCodeRecords.Any())
                {
                    string noRecordsFoundMessage = $"No records found in '{sicSectorSynonymsFilePath}'";
                    log.LogError(noRecordsFoundMessage);
                    return listOfSicCodeRecords;
                }

                log.LogInformation($"Number of records found {listOfSicCodeRecords.Count}");

                #endregion
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred in GetListOfSicCodeSearchModelsFromFile function");
            }

            return listOfSicCodeRecords;
        }

    }
}
