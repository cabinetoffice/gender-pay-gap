using System;
using System.Collections.Generic;
using GovUkDesignSystem;

namespace GenderPayGap.WebUI.Models.Organisation
{
    [Serializable]
    public class RemoveOrganisationModel
    {

        public string EncOrganisationId { get; set; }
        public string EncUserId { get; set; }

        public string OrganisationName { get; set; }
        public List<string> OrganisationAddress { get; set; }
        public string UserName { get; set; }

    }
}
