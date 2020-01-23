namespace GenderPayGap.WebUI.Models
{
    public class ReportingStepsViewModel
    {

        /// <summary>
        ///     The current step of the step-by-step navigation.
        /// </summary>
        public int? CurrentStep { get; set; } = null;

        /// <summary>
        ///     The current task of the step-by-step navigation.
        /// </summary>
        public int? CurrentTask { get; set; } = null;

    }
}
