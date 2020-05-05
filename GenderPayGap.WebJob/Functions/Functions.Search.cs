using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebJob
{
    public partial class Functions
    {

        //Update the search indexes
        public async Task UpdateSearchAsync([TimerTrigger("20 * * * *" /* once per hour, at 20 minutes past the hour */)]
            TimerInfo timer)
        {
            string runId = JobHelpers.CreateRunId();
            DateTime startTime = VirtualDateTime.Now;
            JobHelpers.LogFunctionStart(runId, nameof(UpdateSearchAsync), startTime);

            try
            {
                await UpdateAllSearchIndexesAsync();
                JobHelpers.LogFunctionEnd(runId, nameof(UpdateSearchAsync), startTime);
            }
            catch (Exception ex)
            {
                JobHelpers.LogFunctionError(runId, nameof(UpdateSearchAsync), startTime, ex);

                //Rethrow the error
                throw;
            }
        }

        private async Task UpdateAllSearchIndexesAsync(string userEmail = null, bool force = false)
        {
            CustomLogger.Information($"-- Started the updating of index {Global.SearchIndexName}");
            await UpdateSearchAsync(Global.SearchRepository, Global.SearchIndexName, userEmail, force);
            CustomLogger.Information($"-- Updating of index {Global.SearchIndexName} completed");
            CustomLogger.Information($"-- Started the updating of index {Global.SicCodesIndexName}");
            await UpdateSearchAsync(Global.SicCodeSearchRepository, Global.SicCodesIndexName, userEmail, force);
            CustomLogger.Information($"-- Updating of index {Global.SicCodesIndexName} completed");
        }

        public async Task UpdateSearchAsync(string userEmail = null, bool force = false)
        {
            await UpdateSearchAsync(Global.SearchRepository, Global.SearchIndexName, userEmail, force);
        }

        private async Task UpdateSearchAsync<T>(
            ISearchRepository<T> searchRepositoryToUpdate,
            string indexNameToUpdate,
            string userEmail = null,
            bool force = false)
        {
            if (RunningJobs.Contains(nameof(UpdateSearchAsync)))
            {
                CustomLogger.Information("The set of running jobs already contains 'UpdateSearch'");
                return;
            }

            try
            {
                await searchRepositoryToUpdate.CreateIndexIfNotExistsAsync(indexNameToUpdate);

                if (typeof(T) == typeof(EmployerSearchModel))
                {
                    await AddDataToIndexAsync();
                }
                else if (typeof(T) == typeof(SicCodeSearchModel))
                {
                    await AddDataToSicCodesIndexAsync();
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

        private async Task AddDataToSicCodesIndexAsync()
        {
            List<SicCodeSearchModel> listOfSicCodeRecords = await GetListOfSicCodeSearchModelsFromFileAsync();

            if (listOfSicCodeRecords == null || !listOfSicCodeRecords.Any())
            {
                CustomLogger.Information($"No records to be added to index {Global.SicCodesIndexName}");
                return;
            }

            await Global.SicCodeSearchRepository.RefreshIndexDataAsync(listOfSicCodeRecords);
        }

        private async Task AddDataToIndexAsync()
        {
            CustomLogger.Information("UpdateSearchAsync: Loading SIC codes from file");
            List<SicCodeSearchModel> listOfSicCodeRecords = await GetListOfSicCodeSearchModelsFromFileAsync();

            CustomLogger.Information("UpdateSearchAsync: Loading organisations");

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

            CustomLogger.Information("UpdateSearchAsync: Converting organisations to search results");
            List<EmployerSearchModel> selectionList =
                materialisedOrganisations.Select(o => o.ToEmployerSearchResult(false, listOfSicCodeRecords)).ToList();

            CustomLogger.Information("UpdateSearchAsync: Refreshing index");
            if (selectionList.Any())
            {
                await Global.SearchRepository.RefreshIndexDataAsync(selectionList);
            }

            CustomLogger.Information("UpdateSearchAsync: done with organisations");
        }

        private static async Task<List<SicCodeSearchModel>> GetListOfSicCodeSearchModelsFromFileAsync()
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
                    CustomLogger.Error(unableToFindPathMessage);
                    return null;
                }

                CustomLogger.Information($"Found {fileInPathMessage}");

                #endregion

                #region File exist

                bool fileExists = await Global.FileRepository.GetFileExistsAsync(sicSectorSynonymsFilePath);
                if (!fileExists)
                {
                    string fileDoesNotExistMessage = $"File does not exist {sicSectorSynonymsFilePath}.";
                    CustomLogger.Error(fileDoesNotExistMessage);
                    return null;
                }

                CustomLogger.Information($"File exists {sicSectorSynonymsFilePath}");

                #endregion

                List<SicCodeSearchModel> csv = await Global.FileRepository.ReadCSVAsync<SicCodeSearchModel>(sicSectorSynonymsFilePath);

                listOfSicCodeRecords = csv.OrderBy(o => o.SicCodeId).ToList();

                #region Records found

                if (!listOfSicCodeRecords.Any())
                {
                    string noRecordsFoundMessage = $"No records found in '{sicSectorSynonymsFilePath}'";
                    CustomLogger.Error(noRecordsFoundMessage);
                    return listOfSicCodeRecords;
                }

                CustomLogger.Information($"Number of records found {listOfSicCodeRecords.Count}");

                #endregion
            }
            catch (Exception ex)
            {
                CustomLogger.Error( $"An error occurred in GetListOfSicCodeSearchModelsFromFile function.", ex);
            }

            return listOfSicCodeRecords;
        }

    }
}
