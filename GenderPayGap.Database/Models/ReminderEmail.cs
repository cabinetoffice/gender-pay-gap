using System;
using System.ComponentModel.DataAnnotations.Schema;
using GenderPayGap.Core;
using Newtonsoft.Json;

namespace GenderPayGap.Database.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ReminderEmail
    {

        [JsonProperty]
        public long ReminderEmailId { get; set; }

        public virtual User User { get; set; }
        [ForeignKey("User")]
        [JsonProperty]
        public long UserId { get; set; }

        [JsonProperty]
        public SectorTypes SectorType { get; set; }

        [JsonProperty]
        public DateTime DateChecked { get; set; }

        [JsonProperty]
        public bool EmailSent { get; set; }

        [JsonProperty]
        public ReminderEmailStatus Status { get; set; }
        
        [JsonProperty]
        public DateTime? ReminderDate { get; set; }

    }
}
