using System;
using GenderPayGap.Core;
using GenderPayGap.WebUI.BusinessLogic.Classes;

namespace GenderPayGap.WebUI.BusinessLogic.Models.Organisation
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
