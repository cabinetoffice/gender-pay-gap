using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;
using GenderPayGap.WebUI.Classes.Formatters;

namespace GenderPayGap.WebUI.Models.ManageOrganisations
{
    public class ManageOrganisationViewModel
    {

        public Database.Organisation Organisation { get; }
        public User User { get; }

        private readonly List<int> yearsWithDraftReturns;

        public ManageOrganisationViewModel(Database.Organisation organisation, User user, List<int> yearsWithDraftReturns)
        {
            Organisation = organisation;
            User = user;
            this.yearsWithDraftReturns = yearsWithDraftReturns;
        }

        public List<User> GetFullyRegisteredUsersForOrganisationWithCurrentUserFirst()
        {
            List<User> users = Organisation.UserOrganisations
                .Where(uo => uo.PINConfirmedDate.HasValue)
                .Select(uo => uo.User)
                .ToList();
            
            // The current user must be in this list (otherwise we wouldn't be able to visit this page)
            // So, remove the user from wherever they are n the list
            // And insert them at the start of the list
            users.Remove(User);
            users.Insert(0, User);
            
            return users;
        }

        public List<ManageOrganisationDetailsForYearViewModel> GetOrganisationDetailsForYears()
        {
            List<int> reportingYears = ReportingYearsHelper.GetReportingYears();
            var detailsForYears = new List<ManageOrganisationDetailsForYearViewModel>();

            foreach (int reportingYear in reportingYears)
            {
                // Get organisation's scope for given reporting year
                var scopeForYear = Organisation.GetScopeForYear(reportingYear);

                if (scopeForYear != null)
                {
                    detailsForYears.Add(new ManageOrganisationDetailsForYearViewModel(
                        Organisation,
                        reportingYear,
                        yearsWithDraftReturns.Contains(reportingYear)
                    ));
                }
            }

            return detailsForYears;
        }

    }

    public class ManageOrganisationDetailsForYearViewModel
    {
        public int ReportingYear { get; }

        private readonly Database.Organisation organisation;
        private readonly bool hasDraftReturnForYear;

        public ManageOrganisationDetailsForYearViewModel(Database.Organisation organisation, int reportingYear, bool hasDraftReturnForYear)
        {
            this.organisation = organisation;
            ReportingYear = reportingYear;
            this.hasDraftReturnForYear = hasDraftReturnForYear;
        }


        public object GetFormattedYearText()
        {
            return ReportingYearsHelper.FormatYearAsReportingPeriod(ReportingYear);
        }

        public bool CanChangeScope()
        {
            int currentReportingYear = organisation.SectorType.GetAccountingStartDate().Year;
            int earliestAllowedReportingYear = currentReportingYear - (Global.EditableScopeCount - 1);
            return ReportingYear >= earliestAllowedReportingYear;
        }

        public string GetRequiredToReportOrNotText()
        {
            OrganisationScope scopeForYear = organisation.GetScopeForYear(ReportingYear);

            return scopeForYear.IsInScopeVariant()
                ? "REQUIRED TO REPORT"
                : "NOT REQUIRED TO REPORT";
        }

        public string GetByReportingDeadlineText()
        {
            OrganisationScope scopeForYear = organisation.GetScopeForYear(ReportingYear);

            if (scopeForYear.IsInScopeVariant())
            {
                DateTime snapshotDate = organisation.SectorType.GetAccountingStartDate(ReportingYear);
                DateTime deadline = ReportingYearsHelper.GetDeadlineForAccountingDate(snapshotDate);

                return "by " + deadline.ToString("d MMM yyyy");
            }

            return null;
        }

        public string GetReportStatusText()
        {
            Return returnForYear = organisation.GetReturn(ReportingYear);

            if (returnForYear == null)
            {
                return "Your employer has not reported";
            }

            if (returnForYear.IsVoluntarySubmission())
            {
                return "Reported voluntarily on " + new GDSDateFormatter(returnForYear.Modified).FullStartDate;
            }

            return "Reported on " + new GDSDateFormatter(returnForYear.Modified).FullStartDate;
        }

        public bool DoesReturnOrDraftReturnExistForYear()
        {
            Return returnForYear = organisation.GetReturn(ReportingYear);
            bool hasReturnForYear = returnForYear != null;

            return hasReturnForYear || hasDraftReturnForYear;
        }

        public string GetReportButtonText()
        {
            Return returnForYear = organisation.GetReturn(ReportingYear);
            bool hasReturnForYear = returnForYear != null;

            if (!hasReturnForYear && !hasDraftReturnForYear)
            {
                return "Draft report";
            }

            if (!hasReturnForYear && hasDraftReturnForYear)
            {
                return "Edit draft";
            }

            if (hasReturnForYear && !hasDraftReturnForYear)
            {
                return "Edit report";
            }

            return "Edit draft report";
        }

    }
}
