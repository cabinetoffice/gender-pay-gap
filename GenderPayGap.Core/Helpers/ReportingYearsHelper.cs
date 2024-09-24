using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Helpers
{
    public static class ReportingYearsHelper
    {
        public static List<int> GetReportingYears(SectorTypes sector = SectorTypes.Public)
        {
            int firstReportingYear = Global.FirstReportingYear;
            int currentReportingYear = GetCurrentReportingYear(sector);
            int numberOfYears = (currentReportingYear - firstReportingYear) + 1;

            // Use a manual List capacity allocation and a for-loop to reduce memory usage
            var reportingYears = new List<int>(numberOfYears);
            for (int year = firstReportingYear; year <= currentReportingYear; year++)
            {
                reportingYears.Add(year);
            }

            return reportingYears;
        }

        public static int GetCurrentReportingYear(SectorTypes sector = SectorTypes.Public)
        {
            return sector.GetAccountingStartDate().Year;
        }

        public static int GetCurrentReportingYearForSector(SectorTypes sector)
        {
            return sector.GetAccountingStartDate().Year;
        }

        public static string FormatYearAsReportingPeriod(int reportingPeriodStartYear, string separator = "-")
        {
            int fourDigitStartYear = reportingPeriodStartYear;

            int fourDigitEndYear = reportingPeriodStartYear + 1;
            int twoDigitEndYear = fourDigitEndYear % 100;

            string formattedYear = $"{fourDigitStartYear}{separator}{twoDigitEndYear}";
            return formattedYear;
        }

        //The deadline date is the final date on which returns are not considered late
        public static DateTime GetDeadline(SectorTypes sector, int reportingYear)
        {
            DateTime snapshotDate = sector.GetAccountingStartDate(reportingYear);
            DateTime deadlineDate = GetDeadlineForAccountingDate(snapshotDate);
            return deadlineDate;
        }
        
        public static DateTime GetDeadlineForAccountingDate(DateTime accountingDate)
        {
            int reportingYear = accountingDate.Year;
            DateTime deadline = accountingDate.AddYears(1).AddDays(-1);
            if (reportingYear == 2020)
            {
                // Reporting deadline for 2020 was changed to 2021/10/05 for both public and private
                deadline = new DateTime(2021,10,5);
            }

            return deadline;
        }

        public static int GetTheMostRecentCompletedReportingYear()
        {
            int mostRecentReportingYear = Global.FirstReportingYear;
            foreach (int year in from year in GetReportingYears()
                let accountingDate = SectorTypes.Private.GetAccountingStartDate(year)
                where DeadlineForAccountingDateHasPassed(accountingDate)
                select year)
            {
                mostRecentReportingYear = year;
                break;
            }

            return mostRecentReportingYear;
        }

        public static bool DeadlineHasPassedForYearAndSector(int reportingYear, SectorTypes sector)
        {
            DateTime snapshotDate = sector.GetAccountingStartDate(reportingYear);
            return DeadlineForAccountingDateHasPassed(snapshotDate);
        }

        public static bool DeadlineForAccountingDateHasPassed(DateTime accountingDate)
        {
            return GetDeadlineForAccountingDate(accountingDate).AddDays(1) < VirtualDateTime.Now;
        }

        public static bool IsReportingYearWithFurloughScheme(DateTime accountingDate)
        {
            return IsReportingYearWithFurloughScheme(accountingDate.Year);
        }
        
        public static bool IsReportingYearWithFurloughScheme(int reportingYear)
        {
            return Global.ReportingStartYearsWithFurloughScheme.Contains(reportingYear);
        }

        public static bool IsReportingYearExcludedFromLateFlagEnforcement(int reportingYear)
        {
            return Global.ReportingStartYearsToExcludeFromLateFlagEnforcement.Contains(reportingYear);
        }

    }
}
