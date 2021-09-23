using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes.Formatters;

namespace GenderPayGap.WebUI.Models.ManageOrganisations
{
    public class ManageOrganisationViewModel
    {

        public Database.Organisation Organisation { get; }
        public User User { get; }

        private readonly List<DraftReturn> allDraftReturns;

        public ManageOrganisationViewModel(Database.Organisation organisation, User user, List<DraftReturn> allDraftReturns)
        {
            Organisation = organisation;
            User = user;
            this.allDraftReturns = allDraftReturns;
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
                    detailsForYears.Add(
                        new ManageOrganisationDetailsForYearViewModel(
                            Organisation,
                            reportingYear,
                            allDraftReturns.FirstOrDefault(d => d.SnapshotYear == reportingYear)));
                }
            }

            return detailsForYears;
        }

    }

    public class ManageOrganisationDetailsForYearViewModel
    {

        public int ReportingYear { get; }

        private readonly Database.Organisation organisation;
        private readonly DraftReturn draftReturnForYear;
        private readonly bool hasDraftReturnForYear;

        public ManageOrganisationDetailsForYearViewModel(Database.Organisation organisation, int reportingYear, DraftReturn draftReturnForYear)
        {
            this.organisation = organisation;
            ReportingYear = reportingYear;
            this.draftReturnForYear = draftReturnForYear;
            hasDraftReturnForYear = draftReturnForYear != null;

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

        private DateTime GetAccountingDate()
        {
            return organisation.SectorType.GetAccountingStartDate(ReportingYear);
        }

        private DateTime GetReportingDeadline()
        {
            DateTime snapshotDate = GetAccountingDate();
            return ReportingYearsHelper.GetDeadlineForAccountingDate(snapshotDate);
        }

        private bool DeadlineHasPassed()
        {
            return VirtualDateTime.Now > GetReportingDeadline();
        }

        private bool OrganisationIsRequiredToSubmit()
        {
            return organisation.GetScopeForYear(ReportingYear).IsInScopeVariant()
                   && !Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(GetAccountingDate().Year);
        }

        private ReportStatus GetReportStatus()
        {
            Return returnForYear = organisation.GetReturn(ReportingYear);
            bool reportIsNotSubmitted = returnForYear == null;

            if (OrganisationIsRequiredToSubmit() && reportIsNotSubmitted && !DeadlineHasPassed()) // Required-NotSubmitted-DeadlineNotPassed
            {
                return ReportStatus.Due;
            }

            if (OrganisationIsRequiredToSubmit() && reportIsNotSubmitted && DeadlineHasPassed()) // Required-NotSubmitted-DeadlinePassed
            {
                // Report overdue: RED
                return ReportStatus.Overdue;
            }

            // if (!OrganisationIsRequiredToSubmit() && reportIsNotSubmitted) // NotRequired-NotSubmitted
            // {
            //     // Report not required: GREY
            //     return "Report not required";
            // }

            if (!reportIsNotSubmitted
                && (returnForYear.IsRequired() && returnForYear.IsSubmittedOnTime()
                    || !returnForYear.IsRequired() && returnForYear.IsSubmitted())) // Required-Submitted OR NotRequired-Submitted
            {
                // Report submitted: GREEN
                return ReportStatus.Submitted;
            }

            if (!reportIsNotSubmitted && returnForYear.IsRequired() && returnForYear.IsLateSubmission) // Required-SubmittedLate
            {
                // Report submitted late: GREEN
                return ReportStatus.SubmittedLate;
            }
            
            // Report not required: GREY
            return ReportStatus.NotRequired;

        }

        public string GetReportStatusText()
        {
            switch (GetReportStatus())
            {
                case ReportStatus.Due:
                    return "Report due by " + GetReportingDeadline().ToString("d MMM yyyy");
                case ReportStatus.Overdue:
                    return "Report overdue";
                case ReportStatus.Submitted:
                    return "Report submitted";
                case ReportStatus.SubmittedLate:
                    return "Report submitted late";
                default:
                    return "Report not required";
            }
        }
        
        public string GetReportStatusColour()
        {
            switch (GetReportStatus())
            {
                case ReportStatus.Due:
                    return "blue";
                case ReportStatus.Overdue:
                    return "red";
                case ReportStatus.Submitted:
                case ReportStatus.SubmittedLate:
                    return "green";
                default:
                    return "grey";
            }
        }

        public string GetReportStatusDescription()
        {
            ReportStatus status = GetReportStatus();
            Return returnForYear = organisation.GetReturn(ReportingYear);

            switch (status)
            {
                case ReportStatus.Overdue:
                    return "This report was due on " + GetReportingDeadline().ToString("d MMM yyyy");
                case ReportStatus.Submitted:
                case ReportStatus.SubmittedLate:
                    return "Reported on " + returnForYear.Modified.ToString("d MMM yyyy");
                default:
                    return null;
            }
        }

        public string GetModifiedDateText()
        {
            Return returnForYear = organisation.GetReturn(ReportingYear);
            bool hasReturnForYear = returnForYear != null;

            if (hasReturnForYear)
            {
                return "Last edited on " + returnForYear.Modified.ToString("d MMM yyyy");
            } 
            if (hasDraftReturnForYear)
            {
                return "Last edited on " + draftReturnForYear.Modified.ToString("d MMM yyyy");
            }

            return null;

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
