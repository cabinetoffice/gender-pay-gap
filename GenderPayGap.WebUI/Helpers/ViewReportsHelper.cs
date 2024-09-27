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

    }
}
