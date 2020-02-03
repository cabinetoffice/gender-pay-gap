using System.Collections.Generic;
using GenderPayGap.Database;
using GovUkDesignSystem;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class DatabaseIntegrityChecksViewModel : GovUkViewModel
    {

        public List<Database.Organisation> OrganisationsWithMultipleActiveAddresses { get; set; } = new List<Database.Organisation>();

        public List<Database.Organisation> OrganisationsWhereLatestAddressIsNotActive { get; } = new List<Database.Organisation>();

        public List<Database.Organisation> OrganisationsWithMultipleActiveScopesForASingleYear { get; set; } = new List<Database.Organisation>();
        
        public List<Database.Organisation> OrganisationsWithNoActiveScopeForEveryYear { get; set; } = new List<Database.Organisation>();
        
        public List<Database.Organisation> OrganisationsWithMultipleActiveReturnsForASingleYear { get; set; } = new List<Database.Organisation>();
        
        public List<Database.Organisation> PublicSectorOrganisationsWithoutAPublicSectorType { get; set; } = new List<Database.Organisation>();
    }
}
