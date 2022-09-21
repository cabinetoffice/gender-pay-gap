using System.Collections.Generic;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GovUkDesignSystem;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminViewAuditLogsViewModel 
    {
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public IEnumerable<AuditLog> AuditLogs { get; set; }

        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public Database.Organisation Organisation { get; set; }
        
        [BindNever /* Output Only - only used for sending data from the Controller to the View */]
        public User User { get; set; }

    }
}
