using GenderPayGap.Core;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminChangeSectorAuditLogDetails
    {

        public OrganisationSectors OldSector { get; set; }
        public OrganisationSectors NewSector { get; set; }
        public string Reason { get; set; }

    }
}
