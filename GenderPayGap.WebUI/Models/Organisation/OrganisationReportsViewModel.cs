using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes.Formatters;
using GenderPayGap.WebUI.Helpers;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Organisation
{
    public class OrganisationReportsViewModel
    {

        [BindNever]
        public long OrganisationId { get; set; }

        [BindNever]
        public Dictionary<int, OrganisationScopeForYear> OrganisationScopesByYear { get; set; }

        public OrganisationReportsViewModel(Database.Organisation organisation, List<int> yearsWithDraftReturns)
        {
            this.OrganisationId = organisation.OrganisationId;
            this.OrganisationScopesByYear = GetOrganisationScopes(organisation, yearsWithDraftReturns);
        }

        private Dictionary<int, OrganisationScopeForYear> GetOrganisationScopes(Database.Organisation organisation, List<int> yearsWithDraftReturns)
        {
            List<int> reportingYears = ReportingYearsHelper.GetReportingYears();
            
            Dictionary<int, OrganisationScopeForYear> scopesByYear = new Dictionary<int, OrganisationScopeForYear>();
            
            foreach (int reportingYear in reportingYears)
            {
                // Get organisation's scope for given reporting year
                var scopeForYear = organisation.GetScopeForYear(reportingYear);
                
                if (scopeForYear != null)
                {
                    OrganisationScopeForYear orgScopeForYear = new OrganisationScopeForYear(
                        reportingYear, 
                        organisation.SectorType.GetAccountingStartDate(reportingYear),
                        scopeForYear, 
                        organisation.GetReturn(reportingYear),
                        yearsWithDraftReturns.Contains(reportingYear)
                    );
                    scopesByYear.Add(reportingYear, orgScopeForYear);
                }
            }

            return scopesByYear;
        }

    }

    public class OrganisationScopeForYear
    {

        private readonly int reportingYear;

        public DateTime SnapshotDateForYear;

        private readonly OrganisationScope scopeForYear;

        private readonly Return returnForYear;

        private readonly bool isDraftReturnAvailable;

        public OrganisationScopeForYear(int reportingYear, DateTime snapshotDateForYear, OrganisationScope scopeForYear, Return returnForYear, bool isDraftReturnAvailable)
        {
            this.reportingYear = reportingYear;
            this.SnapshotDateForYear = snapshotDateForYear;
            this.scopeForYear = scopeForYear;
            this.returnForYear = returnForYear;
            this.isDraftReturnAvailable = isDraftReturnAvailable;
        }

        public bool CanChangeScope()
        {
            return Math.Abs(reportingYear - VirtualDateTime.Now.Year) <= Global.EditableScopeCount;
        }

        public string GetScopeVariantText()
        {
            if (!scopeForYear.IsInScopeVariant())
            {
                return "NOT REQUIRED TO REPORT";
            }

            return "REQUIRED TO REPORT";
        }

        public string GetByReportingDeadlineText(DateTime deadline)
        {
            if (scopeForYear.IsInScopeVariant())
            {
                return "by " + deadline.ToString("d MMM yyyy");
            }

            return null;
        }

        public string GetReportStatusText()
        {
            if (returnForYear == null)
            {
                return "Your organisation has not reported";
            } 
            
            if (returnForYear.IsVoluntarySubmission())
            {
                return "Reported voluntarily on " + new GDSDateFormatter(returnForYear.Modified).FullStartDate;
            }

            return "Reported on " + new GDSDateFormatter(returnForYear.Modified).FullStartDate;
        }

        public bool IsAnyReportAvailable()
        {
            return returnForYear != null || isDraftReturnAvailable;
        }
        
        public string GetReportButtonText()
        {
            var isReportAvailable = returnForYear != null;

            if (!isReportAvailable && !isDraftReturnAvailable)
            {
                return "Draft report";
            }

            if (!isReportAvailable && isDraftReturnAvailable)
            {
                return "Edit draft";
            }

            if (isReportAvailable && !isDraftReturnAvailable)
            {
                return "Edit report";
            }

            return "Edit draft report";
        }

    }
}
