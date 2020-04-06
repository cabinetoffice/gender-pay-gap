namespace GenderPayGap.WebUI.Models.Guidance
{
    public class MenWomenBarChartViewModel
    {

        public string Id { get; set; }
        public string Title { get; set; }
        public string SecondaryTitle { get; set; }
        public decimal Male { get; set; }
        public decimal Female { get; set; }
        public string MaleFormatted => string.Format("{0:0.#}", Male);
        public string FemaleFormatted => string.Format("{0:0.#}", Female);
    }
}
