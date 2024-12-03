using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminDatabaseIntegrityCheckOrganisationsToConsiderForDeScopingViewModel
    {
        public List<Organisation> Organisations { get; set; }
        public int ReportingYear { get; set; }
    }
}
