using System;
using GenderPayGap.Core;

namespace GenderPayGap.Database.Models
{
    public class ReminderEmail
    {
        public long ReminderEmailId { get; set; }
        
        public long UserId { get; set; }
        
        public SectorTypes SectorType { get; set; }
        
        public DateTime DateSent { get; set; }
    }
}
