﻿using System;
using GenderPayGap.BusinessLogic.Classes;
using GenderPayGap.Core;

namespace GenderPayGap.BusinessLogic.Models.Organisation
{
    /// <summary>
    /// </summary>
    [Serializable]
    public class ReportInfoModel
    {

        public DateTime ReportingStartDate { get; set; }

        public DateTime? ReportModifiedDate { get; set; }

        public ScopeStatuses ReportingRequirement { get; set; }

        public bool NotRequiredToReport =>
            ReportingRequirement == ScopeStatuses.OutOfScope || ReportingRequirement == ScopeStatuses.PresumedOutOfScope;

        public Draft Draft { get; set; }

        public bool HasDraftContent()
        {
            if (Draft == null)
            {
                return false;
            }

            return Draft.HasContent();
        }

    }
}
