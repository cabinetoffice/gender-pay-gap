using System;

namespace GenderPayGap.WebUI.Classes.Formatters
{

    [Serializable]
    public class GDSDateFormatter
    {

        public GDSDateFormatter(DateTime startDate)
        {
            StartDate = startDate;
        }

        public DateTime StartDate { get; }

        public int StartDay => StartDate.Day;

        public int EndDay => StartDate.Day - 1;

        public string Month => StartDate.ToString("MMMM");

        public int StartYear => StartDate.Year;

        public int EndYear => StartDate.Year + 1;

        public string FullStartDate => $"{StartDay} {Month} {StartYear}";

        public string FullEndDate => $"{EndDay} {Month} {EndYear}";

        public string FullDateRange => $"{FullStartDate} to {FullEndDate}";

        public string FullYearRange => $"{StartYear} to {EndYear}";

        public string FullStartDateTime => StartDate.ToString("yyyy-MM-dd HH:mm");

    }

}
