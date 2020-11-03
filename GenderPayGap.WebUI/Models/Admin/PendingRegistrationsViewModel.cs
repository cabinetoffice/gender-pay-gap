using System.Collections.Generic;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class PendingRegistrationsViewModel
    {

        public List<UserOrganisation> PublicSectorUserOrganisations { get; set; }
        public List<UserOrganisation> NonUkAddressUserOrganisations { get; set; }
        public List<UserOrganisation> ManuallyRegisteredUserOrganisations { get; set; }
        
        public List<UserOrganisation> NewUsersForExistingOrganisations { get; set; }
        
        public List<UserOrganisation> NewCompaniesHouseOrganisations { get; set; }
        
        public List<UserOrganisation> NewManuallyRegisteredOrganisations { get; set; }

    }
}
