using System;

namespace GenderPayGap.Extensions
{
    public static class VirtualDateTime
    {

        private static TimeSpan offsetCurrentDateTimeForSite = TimeSpan.Zero;

        public static DateTime Now => DateTime.Now.Add(offsetCurrentDateTimeForSite);

        public static DateTime UtcNow => DateTime.UtcNow.Add(offsetCurrentDateTimeForSite);

        public static void Initialise(TimeSpan initialisationTimeSpan)
        {
            offsetCurrentDateTimeForSite = initialisationTimeSpan;
        }

    }
}
