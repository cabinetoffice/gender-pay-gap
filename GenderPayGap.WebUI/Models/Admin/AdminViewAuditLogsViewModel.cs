using System.Collections.Generic;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GovUkDesignSystem;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminViewAuditLogsViewModel : GovUkViewModel
    {

        public IEnumerable<AuditLog> AuditLogs { get; set; }
        public Database.Organisation Organisation { get; set; }
        public User User { get; set; }

    }
}
