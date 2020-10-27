using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.ErrorMessages;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Models.Compare;

namespace GenderPayGap.WebUI.BusinessLogic.Services
{
    public interface IOrganisationBusinessLogic
    {
        CustomResult<Organisation> LoadInfoFromEmployerIdentifier(string employerIdentifier);

        CustomResult<Organisation> LoadInfoFromActiveEmployerIdentifier(string employerIdentifier);

        CustomResult<Organisation> GetOrganisationByEncryptedReturnId(string encryptedReturnId);

        IEnumerable<CompareReportModel> GetCompareData(IEnumerable<string> comparedOrganisationIds,
            int year,
            string sortColumn,
            bool sortAscending);

        DataTable GetCompareDatatable(IEnumerable<CompareReportModel> data);

    }

    public class OrganisationBusinessLogic : IOrganisationBusinessLogic
    {

        private readonly IEncryptionHandler _encryptionHandler;
        private readonly IObfuscator _obfuscator;
        private readonly ISubmissionBusinessLogic _submissionLogic;

        public OrganisationBusinessLogic(IDataRepository dataRepo,
            ISubmissionBusinessLogic submissionLogic,
            IEncryptionHandler encryptionHandler,
            IObfuscator obfuscator = null)
        {
            _DataRepository = dataRepo;
            _submissionLogic = submissionLogic;
            _obfuscator = obfuscator;
            _encryptionHandler = encryptionHandler;
        }

        private IDataRepository _DataRepository { get; }

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

            if (!result.Failed && !result.Result.IsSearchable())
            {
                return new CustomResult<Organisation>(InternalMessages.HttpGoneCausedByOrganisationBeingInactive(result.Result.Status));
            }

            return result;
        }


        public CustomResult<Organisation> GetOrganisationByEncryptedReturnId(string encryptedReturnId)
        {
            string decryptedReturnId = _encryptionHandler.DecryptAndDecode(encryptedReturnId);

            Return result = _submissionLogic.GetSubmissionByReturnId(decryptedReturnId.ToInt64());

            if (result == null)
            {
                return new CustomResult<Organisation>(InternalMessages.HttpNotFoundCausedByReturnIdNotInDatabase(encryptedReturnId));
            }

            Organisation organisation = GetOrganisationById(result.OrganisationId);

            return new CustomResult<Organisation>(organisation);
        }

        public virtual IEnumerable<CompareReportModel> GetCompareData(IEnumerable<string> encBasketOrgIds,
            int year,
            string sortColumn,
            bool sortAscending)
        {
            // decrypt all ids for query
            List<long> basketOrgIds = encBasketOrgIds.Select(x => _obfuscator.DeObfuscate(x).ToInt64()).ToList();

            // query against scopes and filter by basket ids
            List<OrganisationScope> dbScopesQuery = _DataRepository.GetAll<OrganisationScope>()
                .Where(os => os.Status == ScopeRowStatuses.Active)
                .Where(os => os.SnapshotDate.Year == year)
                .Where(os => basketOrgIds.Contains(os.OrganisationId))
                .ToList();

            // query submitted returns for current year
            List<Return> dbReturnsQuery = _DataRepository.GetAll<Return>()
                .Where(r => r.Status == ReturnStatuses.Submitted)
                .Where(r => r.AccountingDate.Year == year)
                .Where(r => basketOrgIds.Contains(r.OrganisationId))
                .ToList();

            // finally, generate the left join sql statement between scopes and returns
            var dbResults = dbScopesQuery.GroupJoin(
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
                .ToList();

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

        public virtual Organisation GetOrganisationById(long organisationId)
        {
            return _DataRepository.Get<Organisation>(organisationId);
        }

    }
}
