using System;

namespace GenderPayGap.WebUI.Models.ScopeNew
{
    [Serializable]
    public class OutOfScopeViewModel
    {

        public Database.Organisation Organisation { get; set; }
        
        public int ReportingYear { get; set; }

    }
}
