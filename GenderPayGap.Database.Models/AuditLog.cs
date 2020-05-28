using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using GenderPayGap.Core;
using Newtonsoft.Json;

namespace GenderPayGap.Database.Models
{
    public class AuditLog
    {

        public long AuditLogId { get; set; }
        public AuditedAction Action { get; set; }
        public DateTime CreatedDate { get; set; }
        public virtual Organisation Organisation { get; set; }

        [ForeignKey("OriginalUserId")]
        public virtual User OriginalUser { get; set; }

        [ForeignKey("ImpersonatedUserId")]
        public virtual User ImpersonatedUser { get; set; }

        public string DetailsString { get; set; }

        [NotMapped]
        public Dictionary<string, string> Details
        {
            get => JsonConvert.DeserializeObject<Dictionary<string, string>>(string.IsNullOrEmpty(DetailsString) ? "{}" : DetailsString);
            set => DetailsString = JsonConvert.SerializeObject(value);
        }

    }
}
