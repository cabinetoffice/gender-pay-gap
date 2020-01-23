using System;

namespace GenderPayGap.WebUI.Models.Viewing
{
    public class VerticalBarChartModel
    {

        public string Id { get; set; }
        public decimal Male { get; set; }
        public decimal Female { get; set; }
        public decimal Delta => FemaleIsLower ? 100 - Female : 100 - Male;

        public string DeltaMonetisation
        {
            get
            {
                decimal deltaRounded = Math.Round(Delta);
                if (Female == Male)
                {
                    return "0p";
                }

                if (deltaRounded < 100)
                {
                    return $"{deltaRounded}p";
                }

                return $"£{string.Format("{0:#.00}", deltaRounded / 100)}";
            }
        }

        public bool FemaleIsLower { get; set; }
        public string MaleMonetisation { get; set; }
        public string FemaleMonetisation { get; set; }

    }
}
