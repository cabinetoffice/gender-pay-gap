using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Helpers
{
    public static class ReportingYearsHelper
    {
        public static List<int> GetReportingYears()
        {
            int firstReportingYear = Global.FirstReportingYear;
            int currentReportingYear = GetCurrentReportingYear();
            int numberOfYears = (currentReportingYear - firstReportingYear) + 1;

            // Use a manual List capacity allocation and a for-loop to reduce memory usage
            var reportingYears = new List<int>(numberOfYears);
            for (int year = firstReportingYear; year <= currentReportingYear; year++)
            {
                reportingYears.Add(year);
            }

            return reportingYears;
        }

        public static int GetCurrentReportingYear()
        {
            return SectorTypes.Public.GetAccountingStartDate().Year;
        }

        public static List<(int,string)> GetReportingYearWithCSVFileSize()
        {
            List<int> reportingYears = GetReportingYears().OrderByDescending(y => y).ToList<int>();
            List<(int, int)> yearsWithFileSizes = new List<(int, int)>()
            {
                (reportingYears[0], 55),
                (reportingYears[1], 4032),
                (reportingYears[2], 4129),
                (reportingYears[3], 2798),
                (reportingYears[4], 4266),
                (reportingYears[5], 4211),
            };

            List<(int, string)> result = new List<(int, string)>();
            foreach (var item in yearsWithFileSizes)
            {
                string sizeString = string.Concat(item.Item2.ToString(), "kb");
                if (item.Item2 > 1024)
                {
                    double asDecimal = Math.Round((double)item.Item2 / 1024, 2);
                    sizeString = string.Concat(asDecimal.ToString(), "mb");
                }
                result.Add((item.Item1, sizeString));
            }
            return result;
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

    }
}
