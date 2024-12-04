namespace GenderPayGap.WebUI.Tests.TestHelpers
{
    public static class TestIdGenerator
    {
        private static long nextIdSuffix = 1;
        private static readonly object nextIdSuffixLock = new object();

        public static long GenerateIdFor<T>()
        {
            long idPrefix = CalculateIdPrefixForType<T>();
            long idSuffix = GetNextIdSuffix();

            long id = (idPrefix * 1000000) + idSuffix;

            return id;
        }

        private static long CalculateIdPrefixForType<T>()
        {
            int hashCode = typeof(T).Name.GetHashCode();
            int restrictedRangeHashCode = Math.Abs(hashCode) % 1000;
            return restrictedRangeHashCode;
        }

        private static long GetNextIdSuffix()
        {
            lock (nextIdSuffixLock)
            {
                long idSuffixToReturn = nextIdSuffix;
                nextIdSuffix++;
                return idSuffixToReturn;
            }
        }

    }
}
