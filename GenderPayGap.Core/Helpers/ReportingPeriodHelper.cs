namespace GenderPayGap.Core.Helpers
{
    public static class ReportingPeriodHelper
    {

        public static string FormatReportingPeriod(int reportingPeriodStartYear)
        {
            int fourDigitStartYear = reportingPeriodStartYear;

            int fourDigitEndYear = reportingPeriodStartYear + 1;
            int twoDigitEndYear = fourDigitEndYear % 100;

            string formattedYear = $"{fourDigitStartYear}-{twoDigitEndYear}";
            return formattedYear;
        }

    }
}
