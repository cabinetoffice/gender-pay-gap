using System.Collections.Generic;
using System.Linq;
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
    [Authorize(Roles = LoginRoles.GpgAdmin)]
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
            AdminDashboardData dashboardData = CalculateAdminDashboardData();
            dashboardData.AdminDashboardPage = AdminDashboardPage.Now;

            return View("DataDashboard", dashboardData);
        }

        [HttpGet("data-dashboard/deadline-day")]
        public IActionResult DataDashboardDeadlineDay()
        {
            AdminDashboardData dashboardData = CalculateAdminDashboardData(onDeadlineDay: true);
            dashboardData.AdminDashboardPage = AdminDashboardPage.DeadlineDay;

            return View("DataDashboard", dashboardData);
        }

        private AdminDashboardData CalculateAdminDashboardData(bool onDeadlineDay = false)
        {
            var dashboardData = new AdminDashboardData(onDeadlineDay);
            
            List<Organisation> allOrganisations = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active || org.Status == OrganisationStatuses.Retired)
                .Include(org => org.OrganisationScopes)
                .Include(org => org.Returns)
                // .AsEnumerable()
                .ToList();

            foreach (DashboardDataForReportingYear dataForYear in dashboardData.DashboardDataForReportingYears)
            {
                foreach (Organisation organisation in allOrganisations)
                {
                    DashboardDataForReportingYearAndSector sectorData = dataForYear.DataFor(organisation.SectorType);

                    sectorData.TotalNumberOfOrganisations++;

                    ScopeStatuses scope = sectorData.Date.HasValue
                        ? organisation.GetScopeStatusForYearAsOfDate(dataForYear.ReportingYear, sectorData.Date.Value)
                        : organisation.GetScopeStatusForYear(dataForYear.ReportingYear);

                    bool hasSubmittedReturn = sectorData.Date.HasValue
                        ? organisation.HadSubmittedReturnAsOfDate(dataForYear.ReportingYear, sectorData.Date.Value)
                        : organisation.HasSubmittedReturn(dataForYear.ReportingYear);

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

            return dashboardData;
        }
        
        [HttpGet("data-dashboard/failed-to-report/{reportingYear}/now")]
        public IActionResult DataDashboardNotReportedNow(int reportingYear)
        {
            var viewModel = new NotReportedViewModel
            {
                AdminDashboardPage = AdminDashboardPage.Now,
                ReportingYear = reportingYear,
                Organisations = dataRepository.GetAll<Organisation>()
                    .Where(org => org.Status == OrganisationStatuses.Active || org.Status == OrganisationStatuses.Retired)
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
                    .Where(org => org.Status == OrganisationStatuses.Active || org.Status == OrganisationStatuses.Retired)
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
