using GenderPayGap.Core;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.Scope
{
    public class ScopeDeclaredViewModel
    {

        public Organisation Organisation { get; set; }
        public int ReportingYear { get; set; }
        public ScopeStatuses ScopeStatus { get; set; }

    }
}
