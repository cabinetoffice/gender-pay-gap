using System;

namespace GenderPayGap.WebUI.Models.Admin
{
    [Serializable]
    public class ManualChangesViewModel
    {

        public string LastTestedInput { get; set; }
        public string LastTestedCommand { get; set; }
        public string Command { get; set; }
        public string Parameters { get; set; }
        public string Results { get; set; }
        public string Comment { get; set; }
        public bool Tested { get; set; }
        public string SuccessMessage { get; set; }

    }
}
