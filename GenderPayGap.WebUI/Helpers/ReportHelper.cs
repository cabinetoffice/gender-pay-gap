using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Helpers
{
    public static class ReportHelper
    {

        public static bool HasSubmittedReturn(Organisation organisation, int reportingYear)
        {
            return organisation.GetReturn(reportingYear) != null;
        }

    }
}
