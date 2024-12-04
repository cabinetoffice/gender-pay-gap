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

        public static string FormatYearAsReportingPeriodLongFormat(int reportingPeriodStartYear)
        {
            int fourDigitStartYear = reportingPeriodStartYear;

            int fourDigitEndYear = reportingPeriodStartYear + 1;

            string formattedYear = $"{fourDigitStartYear} to {fourDigitEndYear}";
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
                orderby year descending
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

        public static bool CanChangeScope(SectorTypes sectorType, int reportingYear)
        {
            int currentReportingYear = sectorType.GetAccountingStartDate().Year;
            int earliestAllowedReportingYear = currentReportingYear - (Global.EditableScopeCount - 1);
            return reportingYear >= earliestAllowedReportingYear;
        }

        /// <summary>
        ///     Returns the accounting start date for the specified sector and year
        /// </summary>
        /// <param name="sectorType">The sector type of the organisation</param>
        /// <param name="year">The starting year of the accounting period. If 0 then uses current accounting period</param>
        public static DateTime GetAccountingStartDate(this SectorTypes sectorType, int year = 0)
        {
            if (year == 0)
            {
                return GetSnapshotDateForDateWithinReportingYear(sectorType, VirtualDateTime.Now);
            }
            else
            {
                return GetSnapshotDateForDateWithinReportingYear(sectorType, new DateTime(year, 12, 31));
            }
        }

        public static DateTime GetSnapshotDateForDateWithinReportingYear(this SectorTypes sectorType, DateTime dateWithinReportingYear)
        {
            var snapshotDateDay = 0;
            var snapshotDateMonth = 0;

            switch (sectorType)
            {
                case SectorTypes.Private:
                    snapshotDateDay = Global.PrivateAccountingDate.Day;
                    snapshotDateMonth = Global.PrivateAccountingDate.Month;
                    break;
                case SectorTypes.Public:
                    snapshotDateDay = Global.PublicAccountingDate.Day;
                    snapshotDateMonth = Global.PublicAccountingDate.Month;
                    break;
                case SectorTypes.Unknown:
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(sectorType),
                        sectorType,
                        "Cannot calculate accounting date for this sector type");
            }

            var snapshotDate = new DateTime(dateWithinReportingYear.Year, snapshotDateMonth, snapshotDateDay);

            // The "Date Within Reporting Year" should not be before the "Snapshot Date" (the start of the reporting year)
            // If it is, then subtract 1 year from the Snapshot Date
            // This will happen when the "Date Within Reporting Year" is from January to end of March / early April
            if (dateWithinReportingYear < snapshotDate)
            {
                snapshotDate = snapshotDate.AddYears(-1);
            }
            
            return snapshotDate;
        }

    }
}
