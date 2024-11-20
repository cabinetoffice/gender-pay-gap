using System.Collections.Generic;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.Compare
{
    public class CompareEmployersForYearViewModel
    {

        public List<Organisation> Organisations { get; set; } = new List<Organisation>();
        public int ReportingYear { get; set; }
        public bool CameFromShareLink { get; set; }

    }
}
