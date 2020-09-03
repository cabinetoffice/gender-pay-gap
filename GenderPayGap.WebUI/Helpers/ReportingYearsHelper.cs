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
            int currentReportingYear = OrganisationSectors.Public.GetAccountingStartDate().Year;
            int numberOfYears = currentReportingYear - firstReportingYear + 1;

            List<int> reportingYears = Enumerable.Range(firstReportingYear, numberOfYears).Reverse().ToList();
            return reportingYears;
        }

    }
}
