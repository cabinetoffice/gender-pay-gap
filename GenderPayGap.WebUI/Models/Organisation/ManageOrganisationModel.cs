using System;
using System.Collections.Generic;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Models.Organisation;

namespace GenderPayGap.WebUI.Models.Organisation
{

    [Serializable]
    public class ManageOrganisationModel
    {

        public List<ReportInfoModel> ReportInfoModels;
        public UserOrganisation CurrentUserOrg { get; set; }

        public List<UserOrganisation> AssociatedUserOrgs { get; set; }

        public string EncCurrentOrgId { get; set; }

        public Database.Organisation Organisation => CurrentUserOrg.Organisation;

    }

}
