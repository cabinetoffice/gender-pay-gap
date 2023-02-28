using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Models.Scope
{
    public class ScopeDeclaredViewModel
    {

        public Database.Organisation Organisation { get; set; }
        public int ReportingYear { get; set; }
        public ScopeStatuses ScopeStatus { get; set; }

    }
}
