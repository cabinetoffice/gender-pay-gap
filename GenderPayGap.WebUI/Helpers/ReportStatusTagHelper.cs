using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Helpers
{
    public static class ReportStatusTagHelper
    {

        public static ReportStatusTag GetReportStatusTag(Organisation organisation, int reportingYear)
        {
            Return returnForYear = organisation.GetReturn(reportingYear);
            bool reportIsSubmitted = returnForYear != null;

            if (reportIsSubmitted)
            {   // Report has been submitted

                if (returnForYear.IsVoluntarySubmission())
                {
                    return ReportStatusTag.SubmittedVoluntarily;
                }

                if (returnForYear.IsRequired() && returnForYear.IsLateSubmission)
                {
                    return ReportStatusTag.SubmittedLate;
                }

                return ReportStatusTag.Submitted;
            }
            else
            {   // Report has not been submitted

                if (Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(reportingYear))
                {
                    return ReportStatusTag.NotRequiredDueToCovid;
                }

                if (!organisation.GetScopeStatusForYear(reportingYear).IsInScopeVariant())
                {
                    return ReportStatusTag.NotRequired;
                }

                if (ReportingYearsHelper.DeadlineHasPassedForYearAndSector(reportingYear, organisation.SectorType))
                {
                    return ReportStatusTag.Overdue;
                }

                return ReportStatusTag.Due;
            }
        }

    }
}
