using System.Collections.Generic;
using GenderPayGap.Database.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GenderPayGap.WebUI.Models.Admin
{
    public class AdminSectorHistoryViewModel
    {

        [BindNever]
        public Database.Organisation Organisation { get; set; }
        
        [BindNever]
        public List<AuditLog> SectorHistory { get; set; }

    }
}
