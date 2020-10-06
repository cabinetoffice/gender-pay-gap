using GovUkDesignSystem;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminDataMigrationViewModel : GovUkViewModel
    {

        public string Hostname { get; set; }

        public string Password { get; set; }

        public string BasicAuthUsername { get; set; }
        public string BasicAuthPassword { get; set; }

    }
}
