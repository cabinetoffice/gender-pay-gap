using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeSectorAuditLogDetails
    {

        public SectorTypes OldSector { get; set; }
        public SectorTypes NewSector { get; set; }
        public string Reason { get; set; }

    }
}
