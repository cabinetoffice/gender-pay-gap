using System.Collections.Generic;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminDatabaseIntegrityCheckOrganisationsToConsiderForDeScopingViewModel
    {
        public List<Database.Organisation> Organisations { get; set; }
        public int ReportingYear { get; set; }
    }
}
