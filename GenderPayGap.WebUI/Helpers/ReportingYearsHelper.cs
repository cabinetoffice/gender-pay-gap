using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;

namespace GenderPayGap.WebUI.Helpers
{
    public static class ReportingYearsHelper
    {

        public static List<int> GetReportingYears()
        {
            int firstReportingYear = Global.FirstReportingYear;
            int currentReportingYear = SectorTypes.Public.GetAccountingStartDate().Year;
            int numberOfYears = currentReportingYear - firstReportingYear + 1;

            List<int> reportingYears = Enumerable.Range(firstReportingYear, numberOfYears).Reverse().ToList();
            return reportingYears;
        }

        public static string FormatYearAsReportingPeriod(int reportingPeriodStartYear)
        {
            int fourDigitStartYear = reportingPeriodStartYear;

            int fourDigitEndYear = reportingPeriodStartYear + 1;
            int twoDigitEndYear = fourDigitEndYear % 100;

            string formattedYear = $"{fourDigitStartYear}-{twoDigitEndYear}";
            return formattedYear;
        }

        public static DateTime GetDeadlineForAccountingDate(DateTime accountingDate)
        {
            const int deadlineExtensionFor2020InMonths = 6;
            int reportingYear = accountingDate.Year;
            DateTime deadline = accountingDate.AddYears(1).AddDays(-1);
            if (reportingYear == 2020)
            {
                deadline = deadline.AddMonths(deadlineExtensionFor2020InMonths);
            }

            return deadline;
        }

    }
}
