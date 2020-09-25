using System;
using System.Collections.Generic;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.Organisation
{

    [Serializable]
    public class ManageOrganisationModel
    {

        public UserOrganisation CurrentUserOrg { get; set; }

        public List<UserOrganisation> AssociatedUserOrgs { get; set; }

        public string EncCurrentOrgId { get; set; }

        public Database.Organisation Organisation => CurrentUserOrg.Organisation;

        public List<int> ReportingYearsWithDraftReturns { get; set; }

    }

}
