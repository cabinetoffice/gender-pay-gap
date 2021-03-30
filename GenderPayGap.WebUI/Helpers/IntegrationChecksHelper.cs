using System;

namespace GenderPayGap.WebUI.Helpers
{
    public static class IntegrationChecksHelper
    {
        public static Boolean SumNotEqualTo100(decimal a, decimal b)
        {
            return a + b != 100;
        }

        public static Boolean FigureGreaterThan100OrLessThan(decimal? a, decimal minValue)
        {
            return a > 100 || a < minValue;
        }

        
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
