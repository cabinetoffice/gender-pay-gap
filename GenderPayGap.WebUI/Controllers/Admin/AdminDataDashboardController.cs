using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.Controllers.Admin
{
    [Authorize(Roles = LoginRoles.GpgAdmin + "," + LoginRoles.GpgAdminReadOnly)]
    [Route("admin")]
    public class AdminDataDashboardController : Controller
    {
        private readonly IDataRepository dataRepository;

        public AdminDataDashboardController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("data-dashboard/now")]
        public IActionResult DataDashboardNow()
        {
            AdminDashboardData dashboardData = CalculateAdminDashboardDataForNow();
            dashboardData.AdminDashboardPage = AdminDashboardPage.Now;

            return View("DataDashboard", dashboardData);
        }

        [HttpGet("data-dashboard/deadline-day")]
        public IActionResult DataDashboardDeadlineDay()
        {
            AdminDashboardData dashboardData = CalculateAdminDashboardDataForDeadlineDay();
            dashboardData.AdminDashboardPage = AdminDashboardPage.DeadlineDay;

            return View("DataDashboard", dashboardData);
        }

        public class DataDashboardQueryRow
        {
            public int OrganisationId { get; set; }
            public SectorTypes SectorType { get; set; }
            public OrganisationStatuses OrganisationStatus { get; set; }
            public int ReportingYear { get; set; }
            public ScopeStatuses ScopeStatus { get; set; }
            public ReturnStatuses? ReturnStatus { get; set; }
        }

        private AdminDashboardData CalculateAdminDashboardDataForNow()
        {
            var dashboardData = new AdminDashboardData(onDeadlineDay: false);

            string tbl_OrganisationScopes = dataRepository.GetTable<OrganisationScope>().Name;
            string os_OrganisationId = dataRepository.GetTable<OrganisationScope>().GetColumnName(os => os.OrganisationId);
            string os_SnapshotDate = dataRepository.GetTable<OrganisationScope>().GetColumnName(os => os.SnapshotDate);
            string os_ScopeStatusId = dataRepository.GetTable<OrganisationScope>().GetColumnName(os => os.ScopeStatus);
            string os_StatusId = dataRepository.GetTable<OrganisationScope>().GetColumnName(os => os.Status);
            
            string tbl_Returns = dataRepository.GetTable<Return>().Name;
            string r_OrganisationId = dataRepository.GetTable<Return>().GetColumnName(r => r.OrganisationId);
            string r_AccountingDate = dataRepository.GetTable<Return>().GetColumnName(r => r.AccountingDate);
            string r_StatusId = dataRepository.GetTable<Return>().GetColumnName(r => r.Status);
            
            string tbl_Organisations = dataRepository.GetTable<Organisation>().Name;
            string o_OrganisationId = dataRepository.GetTable<Organisation>().GetColumnName(r => r.OrganisationId);
            string o_SectorTypeId = dataRepository.GetTable<Organisation>().GetColumnName(r => r.SectorType);
            string o_statusId = dataRepository.GetTable<Organisation>().GetColumnName(r => r.Status);
            
            string sql = @$"
                WITH
                ""LatestScopeStatus"" AS (
                    SELECT
                        ""{os_OrganisationId}"",
                        ""{os_SnapshotDate}"",
                        ""{os_ScopeStatusId}""
                    FROM
                        ""{tbl_OrganisationScopes}""
                    WHERE
                        ""{os_StatusId}"" = {(int)ScopeRowStatuses.Active}
                ),
                ""HasSubmittedReturn"" AS (
                    SELECT
                        ""{r_OrganisationId}"",
                        ""{r_AccountingDate}"",
                        MAX(""{r_StatusId}"") AS ""ReturnStatusId""
                    FROM
                        ""{tbl_Returns}""
                    WHERE
                        ""{r_StatusId}"" = {(int)ReturnStatuses.Submitted}
                    GROUP BY
                        ""{r_OrganisationId}"",
                        ""{r_AccountingDate}""
                )
                SELECT
                    Org.""{o_OrganisationId}"" AS ""{nameof(DataDashboardQueryRow.OrganisationId)}"",
                    Org.""{o_SectorTypeId}"" AS ""{nameof(DataDashboardQueryRow.SectorType)}"",
                    Org.""{o_statusId}"" AS ""{nameof(DataDashboardQueryRow.OrganisationStatus)}"",
                    CAST(date_part('year', ""LatestScopeStatus"".""{os_SnapshotDate}"") AS INTEGER) AS ""{nameof(DataDashboardQueryRow.ReportingYear)}"",
                    ""LatestScopeStatus"".""{os_ScopeStatusId}"" AS ""{nameof(DataDashboardQueryRow.ScopeStatus)}"",
                    ""HasSubmittedReturn"".""ReturnStatusId"" AS ""{nameof(DataDashboardQueryRow.ReturnStatus)}""
                FROM
                    ""{tbl_Organisations}"" Org
                LEFT JOIN ""LatestScopeStatus""
                    ON Org.""{o_OrganisationId}"" = ""LatestScopeStatus"".""{os_OrganisationId}""
                LEFT JOIN ""HasSubmittedReturn""
                    ON Org.""{o_OrganisationId}"" = ""HasSubmittedReturn"".""{r_OrganisationId}""
                    AND ""HasSubmittedReturn"".""{r_AccountingDate}"" = ""LatestScopeStatus"".""{os_SnapshotDate}""
                ORDER BY
                    Org.""{o_OrganisationId}"",
                    ""LatestScopeStatus"".""{os_SnapshotDate}""
            ";
            
            List<DataDashboardQueryRow> allRows = dataRepository.SqlQueryRaw<DataDashboardQueryRow>(sql).ToList();

            foreach (DataDashboardQueryRow row in allRows)
            {
                DashboardDataForReportingYear dataForYear = dashboardData.DashboardDataForReportingYears.Single(dataForYear => dataForYear.ReportingYear == row.ReportingYear);
                DashboardDataForReportingYearAndSector sectorData = dataForYear.DataFor(row.SectorType);
                
                OrganisationStatuses status = row.OrganisationStatus;
                ScopeStatuses scope = row.ScopeStatus;
                bool hasSubmittedReturn = row.ReturnStatus.HasValue;
                
                AddToSectorData(sectorData, status, scope, hasSubmittedReturn);
            }

            return dashboardData;
        }

        private AdminDashboardData CalculateAdminDashboardDataForDeadlineDay()
        {
            var dashboardData = new AdminDashboardData(onDeadlineDay: true);
            
            List<Organisation> allOrganisations = dataRepository.GetAll<Organisation>()
                .Include(org => org.OrganisationScopes)
                .Include(org => org.Returns)
                .ToList();

            foreach (DashboardDataForReportingYear dataForYear in dashboardData.DashboardDataForReportingYears)
            {
                foreach (Organisation organisation in allOrganisations)
                {
                    DashboardDataForReportingYearAndSector sectorData = dataForYear.DataFor(organisation.SectorType);

                    OrganisationStatuses status = sectorData.Date.HasValue
                        ? organisation.GetStatusAsOfDate(sectorData.Date.Value)
                        : organisation.Status;
                    
                    ScopeStatuses scope = sectorData.Date.HasValue
                        ? organisation.GetScopeStatusForYearAsOfDate(dataForYear.ReportingYear, sectorData.Date.Value)
                        : organisation.GetScopeStatusForYear(dataForYear.ReportingYear);

                    bool hasSubmittedReturn = sectorData.Date.HasValue
                        ? organisation.HadSubmittedReturnAsOfDate(dataForYear.ReportingYear, sectorData.Date.Value)
                        : organisation.HasSubmittedReturn(dataForYear.ReportingYear);

                    AddToSectorData(sectorData, status, scope, hasSubmittedReturn);
                }
            }

            return dashboardData;
        }
        
        private static void AddToSectorData(DashboardDataForReportingYearAndSector sectorData, OrganisationStatuses status, ScopeStatuses scope, bool hasSubmittedReturn)
        {
            if (status == OrganisationStatuses.Active || status == OrganisationStatuses.Retired)
            {
                sectorData.TotalNumberOfOrganisations++;

                switch (scope)
                {
                    case ScopeStatuses.OutOfScope:
                        sectorData.OrganisationsOutOfScope++;
                        break;
                    case ScopeStatuses.PresumedOutOfScope:
                        sectorData.OrganisationsPresumedOutOfScope++;
                        break;
                    case ScopeStatuses.PresumedInScope:
                        sectorData.OrganisationsPresumedInScope++;
                        break;
                    case ScopeStatuses.InScope:
                        sectorData.OrganisationsInScope++;
                        break;
                    case ScopeStatuses.Unknown:
                    default:
                        sectorData.OrganisationsNoScopeForYear++;
                        break;
                }

                if (!scope.IsInScopeVariant() && !hasSubmittedReturn)
                {
                    sectorData.OrganisationsOutOfScopeAndNotReported++;
                }
                else if (!scope.IsInScopeVariant() & hasSubmittedReturn)
                {
                    sectorData.OrganisationsVoluntarilyReported++;
                }
                else if (scope.IsInScopeVariant() && hasSubmittedReturn)
                {
                    sectorData.OrganisationsInScopeAndReported++;
                }
                else if (scope.IsInScopeVariant() && !hasSubmittedReturn)
                {
                    sectorData.OrganisationsFailedToReport++;
                }
            }
        }

        [HttpGet("data-dashboard/failed-to-report/{reportingYear}/now")]
        public IActionResult DataDashboardNotReportedNow(int reportingYear)
        {
            var viewModel = new NotReportedViewModel
            {
                AdminDashboardPage = AdminDashboardPage.Now,
                ReportingYear = reportingYear,
                Organisations = dataRepository.GetAll<Organisation>()
                    .Where(org => org.Status == OrganisationStatuses.Active)  // || org.Status == OrganisationStatuses.Retired
                    .OrderBy(org => org.OrganisationName)
                    .Include(org => org.OrganisationScopes)
                    .Include(org => org.Returns)
                    .AsEnumerable()
                    .Where(org => org.GetScopeStatusForYear(reportingYear).IsInScopeVariant())
                    .Where(org => !org.HasSubmittedReturn(reportingYear))
                    .ToList()
            };

            return View("NotReported", viewModel);
        }

        [HttpGet("data-dashboard/failed-to-report/{reportingYear}/deadline-date")]
        public IActionResult DataDashboardNotReportedDeadlineDate(int reportingYear)
        {
            var viewModel = new NotReportedViewModel
            {
                AdminDashboardPage = AdminDashboardPage.DeadlineDay,
                ReportingYear = reportingYear,
                Organisations = dataRepository.GetAll<Organisation>()
                    .Where(org => org.Status == OrganisationStatuses.Active)  // || org.Status == OrganisationStatuses.Retired
                    .OrderBy(org => org.OrganisationName)
                    .Include(org => org.OrganisationScopes)
                    .Include(org => org.Returns)
                    .AsEnumerable()
                    .Where(org => org.GetScopeStatusForYearAsOfDate(reportingYear, ReportingYearsHelper.GetDeadline(org.SectorType, reportingYear)).IsInScopeVariant())
                    .Where(org => !org.HadSubmittedReturnAsOfDate(reportingYear, ReportingYearsHelper.GetDeadline(org.SectorType, reportingYear)))
                    .ToList()
            };

            return View("NotReported", viewModel);
        }

    }
}
