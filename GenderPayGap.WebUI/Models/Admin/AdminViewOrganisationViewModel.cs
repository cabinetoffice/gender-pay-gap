using System.Collections.Generic;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminViewOrganisationViewModel
    {
        public Database.Organisation Organisation { get; set; }
        public List<int> ReportingYears { get; set; }
    }
}
