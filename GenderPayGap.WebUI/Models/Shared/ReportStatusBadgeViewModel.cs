﻿using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Models.Shared
{

    public class ReportStatusBadgeViewModel
    {
        public ReportStatusBadgeType ReportStatus { get; set; }
        public string DateText { get; set; }
        public bool Desktop { get; set; }
        public string Classes { get; set; }
    }
}
