using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Classes;

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
                ? "You are required to report."
                : "You are not required to report.";
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
        
        public string GetReportTagClassName()
        {
            switch (GetReportTag())
            {
                case ReportTag.Due:
                    return "govuk-tag--blue";
                case ReportTag.Overdue:
                    return "govuk-tag--red";
                case ReportTag.Submitted:
                case ReportTag.SubmittedLate:
                    return "govuk-tag--green";
                default:
                    return "govuk-tag--grey";
            }
        }

        public string GetReportTagDescription()
        {
            ReportTag tag = GetReportTag();
            Return returnForYear = organisation.GetReturn(ReportingYear);

            switch (tag)
            {
                case ReportTag.Overdue:
                    return "This report was due on " + GetReportingDeadline().ToString("d MMMM yyyy");
                case ReportTag.Submitted:
                case ReportTag.SubmittedLate:
                    return "Reported on " + returnForYear.Modified.ToString("d MMMM yyyy");
                default:
                    return null;
            }
        }

        public string GetModifiedDateText()
        {
            if (hasDraftReturnForYear && draftReturnForYear.Modified != DateTime.MinValue)
            {
                return "Last edited on " + draftReturnForYear.Modified.ToString("d MMMM yyyy");
            }

            return null;

        }

        public bool DoesReturnOrDraftReturnExistForYear()
        {
            Return returnForYear = organisation.GetReturn(ReportingYear);
            bool hasReturnForYear = returnForYear != null;

            return hasReturnForYear || hasDraftReturnForYear;
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
