﻿using System;
using GenderPayGap.Core;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{
    public class ReturnStatus
    {

        public long ReturnStatusId { get; set; }
        public long ReturnId { get; set; }
        public ReturnStatuses Status { get; set; }

        public DateTime StatusDate { get; set; } = VirtualDateTime.Now;
        public string StatusDetails { get; set; }
        public long ByUserId { get; set; }

        public virtual User ByUser { get; set; }
        public virtual Return Return { get; set; }

    }
}
