using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminDashboardData
    {
        public AdminDashboardData(bool onDeadlineDay = false)
        {
            List<int> reportingYears = ReportingYearsHelper.GetReportingYears();
            foreach (int reportingYear in reportingYears)
            {
                var dataForYear = new DashboardDataForReportingYear {ReportingYear = reportingYear};

                if (onDeadlineDay)
                {
                    DateTime privateSectorDeadline = ReportingYearsHelper.GetDeadline(SectorTypes.Private, reportingYear);
                    dataForYear.DataForPrivateSector.Date = privateSectorDeadline < VirtualDateTime.Now ? privateSectorDeadline : (DateTime?)null;
                    
                    DateTime publicSectorDeadline = ReportingYearsHelper.GetDeadline(SectorTypes.Public, reportingYear);
                    dataForYear.DataForPublicSector.Date = publicSectorDeadline < VirtualDateTime.Now ? publicSectorDeadline : (DateTime?)null;
                }
                else
                {
                    dataForYear.DataForPrivateSector.Date = null;
                    dataForYear.DataForPublicSector.Date = null;
                }
                
                DashboardDataForReportingYears.Add(dataForYear);
            }
        }
        
        public AdminDashboardPage AdminDashboardPage { get; set; }
        public List<DashboardDataForReportingYear> DashboardDataForReportingYears { get; } = new List<DashboardDataForReportingYear>();
    }

    public enum AdminDashboardPage
    {
        Now,
        DeadlineDay
    }
    
        public class DashboardDataForReportingYear
    {
        public int ReportingYear { get; set; }

        public DashboardDataForReportingYearAndSector DataForPublicSector { get; } = new DashboardDataForReportingYearAndSector();
        public DashboardDataForReportingYearAndSector DataForPrivateSector { get; } = new DashboardDataForReportingYearAndSector();

        public DashboardDataForReportingYearAndSector DataFor(SectorTypes sector)
        {
            return sector == SectorTypes.Private ? DataForPrivateSector : DataForPublicSector;
        }

        public DashboardDataForReportingYearAndSector OverallData =>
            new DashboardDataForReportingYearAndSector
            {
                TotalNumberOfOrganisations = DataForPublicSector.TotalNumberOfOrganisations + DataForPrivateSector.TotalNumberOfOrganisations,
                
                OrganisationsNoScopeForYear = DataForPublicSector.OrganisationsNoScopeForYear + DataForPrivateSector.OrganisationsNoScopeForYear,
                OrganisationsOutOfScope = DataForPublicSector.OrganisationsOutOfScope + DataForPrivateSector.OrganisationsOutOfScope,
                OrganisationsPresumedOutOfScope = DataForPublicSector.OrganisationsPresumedOutOfScope + DataForPrivateSector.OrganisationsPresumedOutOfScope,
                OrganisationsPresumedInScope = DataForPublicSector.OrganisationsPresumedInScope + DataForPrivateSector.OrganisationsPresumedInScope,
                OrganisationsInScope = DataForPublicSector.OrganisationsInScope + DataForPrivateSector.OrganisationsInScope,
                
                OrganisationsOutOfScopeAndNotReported = DataForPublicSector.OrganisationsOutOfScopeAndNotReported + DataForPrivateSector.OrganisationsOutOfScopeAndNotReported,
                OrganisationsVoluntarilyReported = DataForPublicSector.OrganisationsVoluntarilyReported + DataForPrivateSector.OrganisationsVoluntarilyReported,
                OrganisationsInScopeAndReported = DataForPublicSector.OrganisationsInScopeAndReported + DataForPrivateSector.OrganisationsInScopeAndReported,
                OrganisationsFailedToReport = DataForPublicSector.OrganisationsFailedToReport + DataForPrivateSector.OrganisationsFailedToReport,
            };
    }

    public class DashboardDataForReportingYearAndSector
    {
        public DateTime? Date { get; set; }
        
        public int TotalNumberOfOrganisations { get; set; }


        public int OrganisationsNoScopeForYear { get; set; }
        public int OrganisationsOutOfScope { get; set; }
        public int OrganisationsPresumedOutOfScope { get; set; }
        public int OrganisationsPresumedInScope { get; set; }
        public int OrganisationsInScope { get; set; }
        public int OrganisationsInScopeAndPresumedInScope => OrganisationsInScope + OrganisationsPresumedInScope;

        public int OrganisationsOutOfScopeAndNotReported { get; set; }
        public int OrganisationsVoluntarilyReported { get; set; }
        public int OrganisationsInScopeAndReported { get; set; }
        public int OrganisationsFailedToReport { get; set; }

        public int TotalOrganisationsReported => OrganisationsVoluntarilyReported + OrganisationsInScopeAndReported;
        
        public double OrganisationsInScopeAndReportedPercent
        {
            get
            {
                if (OrganisationsInScopeAndPresumedInScope == 0)
                {
                    return -999; // We don't want to throw an error here, but we should show a number that's obviously wrong so someone investigates
                }
                return ((double) OrganisationsInScopeAndReported / (double) OrganisationsInScopeAndPresumedInScope) * 100;
            }
        }

        public double OrganisationsFailedToReportPercent
        {
            get
            {
                if (OrganisationsInScopeAndPresumedInScope == 0)
                {
                    return -999; // We don't want to throw an error here, but we should show a number that's obviously wrong so someone investigates
                }
                return ((double) OrganisationsFailedToReport / (double) OrganisationsInScopeAndPresumedInScope) * 100;
            }
        }
    }
    
    public class NotReportedViewModel
    {
        public AdminDashboardPage AdminDashboardPage { get; set; }
        public int ReportingYear { get; set; }
        public List<Database.Organisation> Organisations { get; set; }

    }
}
