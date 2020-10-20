using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes.Formatters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Organisation
{
    public class OrganisationReportsViewModel
    {

        [BindNever]
        public Database.Organisation Organisation { get; set; }
        
        [BindNever]
        public List<int> YearsWithDraftReturns { get; set; }
        
        [BindNever]
        public Dictionary<int, OrganisationScopeForYear> OrganisationScopesByYear { get; set; }

    }

    public class OrganisationScopeForYear
    {

        private readonly int scopeYear;

        private readonly OrganisationScope scope;

        private readonly Return returnForYear;

        private readonly bool isDraftReturnAvailable;

        public OrganisationScopeForYear(int scopeYear, OrganisationScope scope, Return returnForYear, bool isDraftReturnAvailable)
        {
            this.scopeYear = scopeYear;
            this.scope = scope;
            this.returnForYear = returnForYear;
            this.isDraftReturnAvailable = isDraftReturnAvailable;
        }

        public bool CanChangeScope()
        {
            return Math.Abs(scopeYear - VirtualDateTime.Now.Year) <= Global.EditableScopeCount;
        }

        public string GetScopeVariantText()
        {
            if (!scope.IsInScopeVariant())
            {
                return "NOT REQUIRED TO REPORT";
            }

            return "REQUIRED TO REPORT";
        }

        public string GetByReportingDeadlineText(DateTime deadline)
        {
            if (scope.IsInScopeVariant())
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
