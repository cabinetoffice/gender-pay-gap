using System;
using System.ComponentModel.DataAnnotations.Schema;
using GenderPayGap.Core;

namespace GenderPayGap.Database.Models
{
    public class ReminderEmail
    {
        public long ReminderEmailId { get; set; }

        public virtual User User { get; set; }
        [ForeignKey("User")]
        public long UserId { get; set; }

        public SectorTypes SectorType { get; set; }

        public DateTime DateChecked { get; set; }

        public bool EmailSent { get; set; }
    }
}
