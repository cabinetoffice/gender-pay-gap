namespace GenderPayGap.Extensions
{
    public static class VirtualDateTime
    {

        private static TimeSpan offsetCurrentDateTimeForSite = TimeSpan.Zero;

        // This uses the system timezone
        public static DateTime Now => DateTime.Now.Add(offsetCurrentDateTimeForSite);

        public static void Initialise(TimeSpan initialisationTimeSpan)
        {
            offsetCurrentDateTimeForSite = initialisationTimeSpan;
        }

    }
}
