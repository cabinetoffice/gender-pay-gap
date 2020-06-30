using System.Collections.Generic;

namespace GenderPayGap.WebUI.BusinessLogic.Models.Scope
{
    public class OrganisationMissingScope
    {

        public Database.Organisation Organisation { get; set; }

        public List<int> MissingSnapshotYears { get; set; }

    }
}
