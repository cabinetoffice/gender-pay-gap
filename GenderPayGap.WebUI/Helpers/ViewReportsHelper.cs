using System;

namespace GenderPayGap.WebUI.Helpers
{
    public static class ViewReportsHelper
    {

        public static string FormatNumberAsPoundsOrPence(decimal number)
        {
            if (number >= 1)
            {
                return $"£{number:#.00}";
            }
            else
            {
                return $"{Math.Round(number * 100)}p";
            }
        }

        public static decimal CalculateBarChartHeightForWomen(decimal payGapPercent)
        {
            return
                payGapPercent >= 0
                    ? 100 - payGapPercent
                    : 100;
        }

        public static decimal CalculateBarChartHeightForMen(decimal payGapPercent)
        {
            return
                payGapPercent >= 0
                    ? 100
                    : 10000 / (100 - payGapPercent);
        }

    }
}
