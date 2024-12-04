using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminViewOrganisationViewModel
    {
        public Organisation Organisation { get; set; }
        public List<int> ReportingYears { get; set; }
    }
}
