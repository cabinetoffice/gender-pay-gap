using System.Collections.Generic;
using GenderPayGap.Database;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Organisation
{
    public class UsersRegisteredToReportForOrganisationViewModel : GovUkViewModel
    {

        [BindNever]
        public List<User> UsersRegisteredToReportForOrganisation { get; set; }
        
        [BindNever]
        public long LoggedInUserId { get; set; }
        
        [BindNever]
        public long OrganisationId { get; set; }

    }
}
