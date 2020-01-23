using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Models.Compare;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.BusinessLogic
{
    public interface IOrganisationBusinessLogic
    {

        // Organisation repo
        Organisation GetOrganisationById(long organisationId);
        Task<List<OrganisationsFileModel>> GetOrganisationsFileModelByYearAsync(int year);

        string GenerateEmployerReference();
        Task SetUniqueEmployerReferencesAsync();
        Task SetUniqueEmployerReferenceAsync(Organisation organisation);

        string GeneratePINCode(bool isTestUser);

        Task<CustomResult<OrganisationScope>> SetAsScopeAsync(string employerRef,
            int changeScopeToSnapshotYear,
            string changeScopeToComment,
            User currentUser,
            ScopeStatuses scopeStatus,
            bool saveToDatabase);

        CustomResult<Organisation> LoadInfoFromEmployerIdentifier(string employerIdentifier);

        CustomResult<Organisation> LoadInfoFromActiveEmployerIdentifier(string employerIdentifier);
        Task<CustomResult<Organisation>> GetOrganisationByEncryptedReturnIdAsync(string encryptedReturnId);

        Task<IEnumerable<CompareReportModel>> GetCompareDataAsync(IEnumerable<string> comparedOrganisationIds,
            int year,
            string sortColumn,
            bool sortAscending);

        Task<CustomResult<Organisation>> CreateSecurityCodeAsync(string employerRef, DateTime securityCodeExpiryDateTime);
        Task<CustomBulkResult<Organisation>> CreateSecurityCodesInBulkAsync(DateTime securityCodeExpiryDateTime);
        Task<CustomResult<Organisation>> ExtendSecurityCodeAsync(string employerRef, DateTime securityCodeExpiryDateTime);
        Task<CustomBulkResult<Organisation>> ExtendSecurityCodesInBulkAsync(DateTime securityCodeExpiryDateTime);
        Task<CustomResult<Organisation>> ExpireSecurityCodeAsync(string employerRef);
        Task<CustomBulkResult<Organisation>> ExpireSecurityCodesInBulkAsync();

        DataTable GetCompareDatatable(IEnumerable<CompareReportModel> data);
        Task<Organisation> GetOrganisationByEmployerReferenceAndSecurityCodeAsync(string employerReference, string securityCode);

    }

    public class OrganisationBusinessLogic : IOrganisationBusinessLogic
    {

        private ICommonBusinessLogic _commonBusinessLogic;
        private readonly IEncryptionHandler _encryptionHandler;
        private readonly IObfuscator _obfuscator;
        private readonly IScopeBusinessLogic _scopeLogic;
        private readonly ISecurityCodeBusinessLogic _securityCodeLogic;
        private readonly ISubmissionBusinessLogic _submissionLogic;

        public OrganisationBusinessLogic(ICommonBusinessLogic commonBusinessLogic,
            IDataRepository dataRepo,
            ISubmissionBusinessLogic submissionLogic,
            IScopeBusinessLogic scopeLogic,
            IEncryptionHandler encryptionHandler,
            ISecurityCodeBusinessLogic securityCodeLogic,
            IObfuscator obfuscator = null)
        {
            _commonBusinessLogic = commonBusinessLogic;
            _DataRepository = dataRepo;
            _submissionLogic = submissionLogic;
            _scopeLogic = scopeLogic;
            _obfuscator = obfuscator;
            _securityCodeLogic = securityCodeLogic;
            _encryptionHandler = encryptionHandler;
        }

        private IDataRepository _DataRepository { get; }

        /// <summary>
        ///     Gets a list of organisations with latest returns and scopes for Organisations download file
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public virtual async Task<List<OrganisationsFileModel>> GetOrganisationsFileModelByYearAsync(int year)
        {
#if DEBUG
            IQueryable<Organisation> orgs = Debugger.IsAttached
                ? _DataRepository.GetAll<Organisation>().Take(100)
                : _DataRepository.GetAll<Organisation>();
#else
            var orgs = _DataRepository.GetAll<Organisation>();
#endif
            var records = new List<OrganisationsFileModel>();

            foreach (Organisation o in await orgs.ToListAsync())
            {
                var record = new OrganisationsFileModel {
                    OrganisationId = o.OrganisationId,
                    DUNSNumber = o.DUNSNumber,
                    EmployerReference = o.EmployerReference,
                    OrganisationName = o.OrganisationName,
                    CompanyNo = o.CompanyNumber,
                    Sector = o.SectorType,
                    Status = o.Status,
                    StatusDate = o.StatusDate,
                    StatusDetails = o.StatusDetails,
                    Address = o.LatestAddress?.GetAddressString(),
                    SicCodes = o.GetSicCodeIdsString(),
                    LatestRegistrationDate = o.LatestRegistration?.PINConfirmedDate,
                    LatestRegistrationMethod = o.LatestRegistration?.Method,
                    Created = o.Created,
                    SecurityCode = o.SecurityCode,
                    SecurityCodeExpiryDateTime = o.SecurityCodeExpiryDateTime,
                    SecurityCodeCreatedDateTime = o.SecurityCodeCreatedDateTime
                };

                Return latestReturn = await _submissionLogic.GetLatestSubmissionBySnapshotYearAsync(o.OrganisationId, year);
                OrganisationScope latestScope = await _scopeLogic.GetLatestScopeBySnapshotYearAsync(o.OrganisationId, year);

                record.LatestReturn = latestReturn?.Modified;
                record.ScopeStatus = latestScope?.ScopeStatus;
                record.ScopeDate = latestScope?.ScopeStatusDate;
                records.Add(record);
            }


            return records;
        }

        public virtual async Task SetUniqueEmployerReferencesAsync()
        {
            List<Organisation> orgs = await _DataRepository.GetAll<Organisation>().Where(o => o.EmployerReference == null).ToListAsync();
            foreach (Organisation org in orgs)
            {
                await SetUniqueEmployerReferenceAsync(org);
            }
        }

        public virtual async Task SetUniqueEmployerReferenceAsync(Organisation organisation)
        {
            //Get the unique reference
            retry:
            do
            {
                organisation.EmployerReference = GenerateEmployerReference();
            } while (await _DataRepository.GetAll<Organisation>()
                .AnyAsync(o => o.OrganisationId != organisation.OrganisationId && o.EmployerReference == organisation.EmployerReference));

            try
            {
                //Save the organisation
                await _DataRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                var bex = ex.GetBaseException() as SqlException;
                if (bex != null)
                {
                    switch (bex.Number)
                    {
                        case 2601: // Duplicated key row error
                            goto retry;
                        case 2627: // Unique constraint error
                        case 547: // Constraint check violation
                        default:
                            break;
                    }
                }

                throw;
            }
        }

        public virtual string GenerateEmployerReference()
        {
            return Crypto.GeneratePasscode(Global.EmployerCodeChars.ToCharArray(), Global.EmployerCodeLength);
        }

        public virtual string GeneratePINCode(bool isTestUser)
        {
            if (isTestUser)
            {
                return "ABCDEFG";
            }

            return Crypto.GeneratePasscode(Global.PINChars.ToCharArray(), Global.PINLength);
        }

        public virtual async Task<CustomResult<OrganisationScope>> SetAsScopeAsync(string employerRef,
            int changeScopeToSnapshotYear,
            string changeScopeToComment,
            User currentUser,
            ScopeStatuses scopeStatus,
            bool saveToDatabase)
        {
            Organisation org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return await _scopeLogic.AddScopeAsync(
                org,
                scopeStatus,
                currentUser,
                changeScopeToSnapshotYear,
                changeScopeToComment,
                saveToDatabase);
        }

        public CustomResult<Organisation> LoadInfoFromEmployerIdentifier(string employerIdentifier)
        {
            int organisationId = _obfuscator.DeObfuscate(employerIdentifier);

            if (organisationId == 0)
            {
                return new CustomResult<Organisation>(InternalMessages.HttpBadRequestCausedByInvalidEmployerIdentifier(employerIdentifier));
            }

            Organisation organisation = GetOrganisationById(organisationId);

            if (organisation == null)
            {
                return new CustomResult<Organisation>(InternalMessages.HttpNotFoundCausedByOrganisationIdNotInDatabase(employerIdentifier));
            }

            return new CustomResult<Organisation>(organisation);
        }


        public virtual CustomResult<Organisation> LoadInfoFromActiveEmployerIdentifier(string employerIdentifier)
        {
            CustomResult<Organisation> result = LoadInfoFromEmployerIdentifier(employerIdentifier);

            if (!result.Failed && !result.Result.IsActive())
            {
                return new CustomResult<Organisation>(InternalMessages.HttpGoneCausedByOrganisationBeingInactive(result.Result.Status));
            }

            return result;
        }


        public async Task<CustomResult<Organisation>> GetOrganisationByEncryptedReturnIdAsync(string encryptedReturnId)
        {
            string decryptedReturnId = _encryptionHandler.DecryptAndDecode(encryptedReturnId);

            Return result = await _submissionLogic.GetSubmissionByReturnIdAsync(decryptedReturnId.ToInt64());

            if (result == null)
            {
                return new CustomResult<Organisation>(InternalMessages.HttpNotFoundCausedByReturnIdNotInDatabase(encryptedReturnId));
            }

            Organisation organisation = GetOrganisationById(result.OrganisationId);

            return new CustomResult<Organisation>(organisation);
        }

        public virtual async Task<IEnumerable<CompareReportModel>> GetCompareDataAsync(IEnumerable<string> encBasketOrgIds,
            int year,
            string sortColumn,
            bool sortAscending)
        {
            // decrypt all ids for query
            List<long> basketOrgIds = encBasketOrgIds.Select(x => _obfuscator.DeObfuscate(x).ToInt64()).ToList();

            // query against scopes and filter by basket ids
            IQueryable<OrganisationScope> dbScopesQuery = _DataRepository.GetAll<OrganisationScope>()
                .Where(os => os.Status == ScopeRowStatuses.Active && os.SnapshotDate.Year == year)
                .Where(r => basketOrgIds.Contains(r.OrganisationId));

            // query submitted returns for current year
            IQueryable<Return> dbReturnsQuery = _DataRepository.GetAll<Return>()
                .Where(r => r.Status == ReturnStatuses.Submitted && r.AccountingDate.Year == year);

            // finally, generate the left join sql statement between scopes and returns
            var dbResults = await dbScopesQuery.GroupJoin(
                    // join
                    dbReturnsQuery,
                    // on
                    // inner
                    Scope => Scope.OrganisationId,
                    // outer
                    Return => Return.OrganisationId,
                    // into
                    (Scope, Return) => new {Scope, Return = Return.LastOrDefault()})
                // execute on sql server and return results into memory
                .ToListAsync();

            // build the compare reports list
            List<CompareReportModel> compareReports = dbResults.Select(
                    r => {
                        return new CompareReportModel {
                            EncOrganisationId = _obfuscator.Obfuscate(r.Scope.OrganisationId.ToString()),
                            OrganisationName = r.Scope.Organisation.OrganisationName,
                            ScopeStatus = r.Scope.ScopeStatus,
                            HasReported = r.Return != null,
                            OrganisationSize = r.Return?.OrganisationSize,
                            DiffMeanBonusPercent = r.Return?.DiffMeanBonusPercent,
                            DiffMeanHourlyPayPercent = r.Return?.DiffMeanHourlyPayPercent,
                            DiffMedianBonusPercent = r.Return?.DiffMedianBonusPercent,
                            DiffMedianHourlyPercent = r.Return?.DiffMedianHourlyPercent,
                            FemaleLowerPayBand = r.Return?.FemaleLowerPayBand,
                            FemaleMedianBonusPayPercent = r.Return?.FemaleMedianBonusPayPercent,
                            FemaleMiddlePayBand = r.Return?.FemaleMiddlePayBand,
                            FemaleUpperPayBand = r.Return?.FemaleUpperPayBand,
                            FemaleUpperQuartilePayBand = r.Return?.FemaleUpperQuartilePayBand,
                            MaleMedianBonusPayPercent = r.Return?.MaleMedianBonusPayPercent,
                            HasBonusesPaid = r.Return?.HasBonusesPaid()
                        };
                    })
                .ToList();

            // Sort the results if required
            if (!string.IsNullOrWhiteSpace(sortColumn))
            {
                return SortCompareReports(compareReports, sortColumn, sortAscending);
            }

            // Return the results
            return compareReports;
        }

        public virtual DataTable GetCompareDatatable(IEnumerable<CompareReportModel> data)
        {
            DataTable datatable = data.Select(
                    r => new {
                        r.OrganisationName,
                        r.OrganisationSizeName,
                        DiffMeanHourlyPayPercent = r.HasReported == false ? null : r.DiffMeanHourlyPayPercent.FormatDecimal("0.0"),
                        DiffMedianHourlyPercent = r.HasReported == false ? null : r.DiffMedianHourlyPercent.FormatDecimal("0.0"),
                        FemaleLowerPayBand = r.HasReported == false ? null : r.FemaleLowerPayBand.FormatDecimal("0.0"),
                        FemaleMiddlePayBand = r.HasReported == false ? null : r.FemaleMiddlePayBand.FormatDecimal("0.0"),
                        FemaleUpperPayBand = r.HasReported == false ? null : r.FemaleUpperPayBand.FormatDecimal("0.0"),
                        FemaleUpperQuartilePayBand = r.HasReported == false ? null : r.FemaleUpperQuartilePayBand.FormatDecimal("0.0"),
                        FemaleMedianBonusPayPercent = r.HasReported == false ? null : r.FemaleMedianBonusPayPercent.FormatDecimal("0.0"),
                        MaleMedianBonusPayPercent = r.HasReported == false ? null : r.MaleMedianBonusPayPercent.FormatDecimal("0.0"),
                        DiffMeanBonusPercent = r.HasReported == false ? null : r.DiffMeanBonusPercent.FormatDecimal("0.0"),
                        DiffMedianBonusPercent = r.HasReported == false ? null : r.DiffMedianBonusPercent.FormatDecimal("0.0")
                    })
                .ToDataTable();

            datatable.Columns[nameof(CompareReportModel.OrganisationName)].ColumnName = "Employer";
            datatable.Columns[nameof(CompareReportModel.OrganisationSizeName)].ColumnName = "Employer Size";
            datatable.Columns[nameof(CompareReportModel.DiffMeanHourlyPayPercent)].ColumnName = "% Difference in hourly rate (Mean)";
            datatable.Columns[nameof(CompareReportModel.DiffMedianHourlyPercent)].ColumnName = "% Difference in hourly rate (Median)";
            datatable.Columns[nameof(CompareReportModel.FemaleLowerPayBand)].ColumnName = "% Women in lower pay quartile";
            datatable.Columns[nameof(CompareReportModel.FemaleMiddlePayBand)].ColumnName = "% Women in lower middle pay quartile";
            datatable.Columns[nameof(CompareReportModel.FemaleUpperPayBand)].ColumnName = "% Women in upper middle pay quartile";
            datatable.Columns[nameof(CompareReportModel.FemaleUpperQuartilePayBand)].ColumnName = "% Women in top pay quartile";
            datatable.Columns[nameof(CompareReportModel.FemaleMedianBonusPayPercent)].ColumnName = "% Who received bonus pay (Women)";
            datatable.Columns[nameof(CompareReportModel.MaleMedianBonusPayPercent)].ColumnName = "% Who received bonus pay (Men)";
            datatable.Columns[nameof(CompareReportModel.DiffMeanBonusPercent)].ColumnName = "% Difference in bonus pay (Mean)";
            datatable.Columns[nameof(CompareReportModel.DiffMedianBonusPercent)].ColumnName = "% Difference in bonus pay (Median)";

            return datatable;
        }


        private async Task<IEnumerable<Organisation>> GetAllActiveOrPendingOrganisationsOrThrowAsync()
        {
            IQueryable<Organisation> orgList = _DataRepository.GetAll<Organisation>()
                .Where(o => o.Status == OrganisationStatuses.Active || o.Status == OrganisationStatuses.Pending);

            if (!await orgList.AnyAsync())
            {
                throw new Exception("Unable to find organisations with statuses 'Active' or 'Pending' in the database");
            }

            return orgList;
        }

        private IEnumerable<CompareReportModel> SortCompareReports(IEnumerable<CompareReportModel> originalList,
            string sortColumn,
            bool sortAscending)
        {
            IEnumerable<CompareReportModel> listOfNulls;
            IEnumerable<CompareReportModel> listOfNotNulls;

            listOfNulls = originalList.Where(x => x.HasReported == false);
            listOfNotNulls = originalList.Where(x => x.HasReported);

            if (isBonusColumn(sortColumn))
            {
                listOfNulls = originalList.Where(x => x.HasBonusesPaid == false);
                listOfNotNulls = originalList.Where(x => x.HasBonusesPaid == true);
            }

            listOfNotNulls = listOfNotNulls.AsQueryable().OrderBy($"{sortColumn} {(sortAscending ? "ASC" : "DESC")}");

            return listOfNotNulls.Union(listOfNulls);
        }

        private bool isBonusColumn(string columnToCheck)
        {
            switch (columnToCheck)
            {
                case "FemaleMedianBonusPayPercent":
                case "MaleMedianBonusPayPercent":
                case "DiffMeanBonusPercent":
                case "DiffMedianBonusPercent":
                    return true;
            }

            return false;
        }

        #region Repo

        public virtual Organisation GetOrganisationById(long organisationId)
        {
            return _DataRepository.Get<Organisation>(organisationId);
        }

        public virtual async Task<Organisation> GetOrganisationByEmployerReferenceAsync(string employerReference)
        {
            return await _DataRepository.GetAll<Organisation>()
                .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerReference.ToUpper());
        }

        public virtual async Task<Organisation> GetOrganisationByEmployerReferenceAndSecurityCodeAsync(string employerReference,
            string securityCode)
        {
            return await _DataRepository.GetAll<Organisation>()
                .FirstOrDefaultAsync(o => o.EmployerReference.ToUpper() == employerReference.ToUpper() && o.SecurityCode == securityCode);
        }

        public virtual async Task<Organisation> GetOrganisationByEmployerReferenceOrThrowAsync(string employerReference)
        {
            Organisation org = await GetOrganisationByEmployerReferenceAsync(employerReference);

            if (org == null)
            {
                throw new ArgumentException(
                    $"Cannot find organisation with employerReference {employerReference}",
                    nameof(employerReference));
            }

            return org;
        }

        #endregion

        #region Security Codes

        public delegate CustomResult<Organisation> ActionSecurityCodeDelegate(Organisation organisation,
            DateTime securityCodeExpiryDateTime);

        private async Task<CustomBulkResult<Organisation>> ActionSecurityCodesInBulkAsync(DateTime securityCodeExpiryDateTime,
            ActionSecurityCodeDelegate actionSecurityCodeDelegate)
        {
            IEnumerable<Organisation> listOfOrganisations = await GetAllActiveOrPendingOrganisationsOrThrowAsync();

            var concurrentBagOfProcessedOrganisations = new ConcurrentBag<Organisation>();
            var concurrentBagOfErrors = new ConcurrentBag<CustomResult<Organisation>>();

            Parallel.ForEach(
                listOfOrganisations,
                organisation => {
                    CustomResult<Organisation> securityCodeCreationResult =
                        actionSecurityCodeDelegate(organisation, securityCodeExpiryDateTime);

                    if (securityCodeCreationResult.Failed)
                    {
                        concurrentBagOfErrors.Add(securityCodeCreationResult);
                    }
                    else
                    {
                        concurrentBagOfProcessedOrganisations.Add(securityCodeCreationResult.Result);
                    }
                });

            return new CustomBulkResult<Organisation> {
                ConcurrentBagOfSuccesses = concurrentBagOfProcessedOrganisations, ConcurrentBagOfErrors = concurrentBagOfErrors
            };
        }

        public async Task<CustomResult<Organisation>> CreateSecurityCodeAsync(string employerRef, DateTime securityCodeExpiryDateTime)
        {
            Organisation org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return _securityCodeLogic.CreateSecurityCode(org, securityCodeExpiryDateTime);
        }

        public async Task<CustomBulkResult<Organisation>> CreateSecurityCodesInBulkAsync(DateTime securityCodeExpiryDateTime)
        {
            return await ActionSecurityCodesInBulkAsync(securityCodeExpiryDateTime, _securityCodeLogic.CreateSecurityCode);
        }

        public async Task<CustomResult<Organisation>> ExtendSecurityCodeAsync(string employerRef, DateTime securityCodeExpiryDateTime)
        {
            Organisation org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return _securityCodeLogic.ExtendSecurityCode(org, securityCodeExpiryDateTime);
        }

        public async Task<CustomBulkResult<Organisation>> ExtendSecurityCodesInBulkAsync(DateTime securityCodeExpiryDateTime)
        {
            return await ActionSecurityCodesInBulkAsync(securityCodeExpiryDateTime, _securityCodeLogic.ExtendSecurityCode);
        }

        public async Task<CustomResult<Organisation>> ExpireSecurityCodeAsync(string employerRef)
        {
            Organisation org = await GetOrganisationByEmployerReferenceOrThrowAsync(employerRef);
            return _securityCodeLogic.ExpireSecurityCode(org);
        }

        public async Task<CustomBulkResult<Organisation>> ExpireSecurityCodesInBulkAsync()
        {
            IEnumerable<Organisation> listOfOrganisations = await GetAllActiveOrPendingOrganisationsOrThrowAsync();

            var concurrentBagOfProcessedOrganisations = new ConcurrentBag<Organisation>();
            var concurrentBagOfErrors = new ConcurrentBag<CustomResult<Organisation>>();

            Parallel.ForEach(
                listOfOrganisations,
                organisation => {
                    CustomResult<Organisation> securityCodeCreationResult = _securityCodeLogic.ExpireSecurityCode(organisation);

                    if (securityCodeCreationResult.Failed)
                    {
                        concurrentBagOfErrors.Add(securityCodeCreationResult);
                    }
                    else
                    {
                        concurrentBagOfProcessedOrganisations.Add(securityCodeCreationResult.Result);
                    }
                });

            return new CustomBulkResult<Organisation> {
                ConcurrentBagOfSuccesses = concurrentBagOfProcessedOrganisations, ConcurrentBagOfErrors = concurrentBagOfErrors
            };
        }

        #endregion

    }
}
