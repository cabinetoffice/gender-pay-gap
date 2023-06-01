using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.WebUI.Classes;

namespace GenderPayGap.WebUI.Models.ManageOrganisations
{
    public class AllOrganisationReportsViewModel
    {

        public Database.Organisation Organisation { get; }
        public User User { get; }
        
        private readonly List<DraftReturn> allDraftReturns;
        public int? CurrentPage { get; set; }
        public int? TotalPages { get; set; }
        public int? EntriesPerPage { get; set; }

        public AllOrganisationReportsViewModel(Database.Organisation organisation, User user, List<DraftReturn> allDraftReturns, int? currentPage, int? totalPages, int? entriesPerPage)
        {
            Organisation = organisation;
            User = user;
            this.allDraftReturns = allDraftReturns;
            CurrentPage = currentPage;
            TotalPages = totalPages;
            EntriesPerPage = entriesPerPage;
        }

        public List<AllOrganisationReportsForYearViewModel> GetOrganisationDetailsForYears()
        {
            List<int> reportingYears = ReportingYearsHelper.GetReportingYears();
            var detailsForYears = new List<AllOrganisationReportsForYearViewModel>();

            foreach (int reportingYear in reportingYears)
            {
                // Get organisation's scope for given reporting year
                var scopeForYear = Organisation.GetScopeForYear(reportingYear);

                if (scopeForYear != null)
                {
                    detailsForYears.Add(
                        new AllOrganisationReportsForYearViewModel(
                            Organisation,
                            reportingYear,
                            allDraftReturns.Where(d => d.SnapshotYear == reportingYear)
                                .OrderByDescending(d => d.Modified)
                                .FirstOrDefault()
                        )
                    );
                }
            }

            return detailsForYears;
        }
    }
    
    public class AllOrganisationReportsForYearViewModel
    {

        public int ReportingYear { get; }

        private readonly Database.Organisation organisation;
        private readonly bool hasDraftReturnForYear;

        public AllOrganisationReportsForYearViewModel(Database.Organisation organisation, int reportingYear, DraftReturn draftReturnForYear)
        {
            this.organisation = organisation;
            ReportingYear = reportingYear;
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
                ? "You are required to report."
                : "You are not required to report.";
        }

        private DateTime GetAccountingDate()
        {
            return organisation.SectorType.GetAccountingStartDate(ReportingYear);
        }

        public DateTime GetReportingDeadline()
        {
            DateTime snapshotDate = GetAccountingDate();
            return ReportingYearsHelper.GetDeadlineForAccountingDate(snapshotDate);
        }

        private bool DeadlineHasPassed()
        {
            return ReportingYearsHelper.DeadlineForAccountingDateHasPassed(GetAccountingDate());
        }

        private bool OrganisationIsRequiredToSubmit()
        {
            return organisation.GetScopeForYear(ReportingYear).IsInScopeVariant()
                   && !Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(GetAccountingDate().Year);
        }

        public ReportTag GetReportTag()
        {
            Return returnForYear = organisation.GetReturn(ReportingYear);
            bool reportIsSubmitted = returnForYear != null;
            
            return reportIsSubmitted ? GetSubmittedReportTag(returnForYear) : GetNotSubmittedReportTag();
        }

        public ReportStatusBadgeType GetReportBadge()
        {
            var tag = GetReportTag();
            switch (tag)
            {
                case ReportTag.Submitted:
                    return ReportStatusBadgeType.Reported;
                case ReportTag.SubmittedLate:
                    return ReportStatusBadgeType.SubmittedLate;
                case ReportTag.Due:
                    return ReportStatusBadgeType.Due;
                case ReportTag.Overdue:
                    return ReportStatusBadgeType.Overdue;
                case ReportTag.NotRequired:
                    return ReportStatusBadgeType.NotRequired;
                default:
                    return ReportStatusBadgeType.Due;
            }
        }

        private ReportTag GetSubmittedReportTag(Return returnForYear)
        {
            bool returnIsRequired = returnForYear.IsRequired();
            
            if (returnIsRequired && returnForYear.IsLateSubmission)
            {
                return ReportTag.SubmittedLate;
            }

            return ReportTag.Submitted;
        }

        private ReportTag GetNotSubmittedReportTag()
        {
            if (!OrganisationIsRequiredToSubmit())
            {
                return ReportTag.NotRequired;
            }

            return DeadlineHasPassed() ? ReportTag.Overdue : ReportTag.Due;
        }

        public string GetReportTagText()
        {
            switch (GetReportTag())
            {
                case ReportTag.Due:
                    return "Report due by " + GetReportingDeadline().ToString("d MMMM yyyy");
                case ReportTag.Overdue:
                    return "Report overdue";
                case ReportTag.Submitted:
                    return "Report submitted";
                case ReportTag.SubmittedLate:
                    return "Report submitted late";
                default:
                    return "Report not required";
            }
        }

        public string GetReportLinkText()
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
