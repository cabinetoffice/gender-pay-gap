using System;
using GenderPayGap.BusinessLogic.Models.Organisation;

namespace GenderPayGap.WebUI.Models.Organisation
{

    [Serializable]
    public class DisplayReportModel
    {

        public long OrganisationId { get; set; }

        public string EncCurrentOrgId { get; set; }

        public ReportInfoModel Report { get; set; }

        public bool CanChangeScope { get; set; }

    }

}
