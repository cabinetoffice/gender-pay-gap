namespace GenderPayGap.WebUI.Helpers
{
    public static class IntegrationChecksHelper
    {
        public static Boolean HasMoreThanOneDecimalPlace(decimal? number)
        {
            if (!number.HasValue)
            {
                return false;
            }

            return Decimal.Round(number.Value, 1) != number.Value;
        }
    }
}
