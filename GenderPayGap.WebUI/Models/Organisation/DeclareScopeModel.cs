using System;
using System.ComponentModel.DataAnnotations;
using GenderPayGap.Core;

namespace GenderPayGap.WebUI.Models.Organisation
{
    [Serializable]
    public class DeclareScopeModel
    {
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public DateTime SnapshotDate { get; set; }

        [Required(AllowEmptyStrings = false)]
        public ScopeStatuses? ScopeStatus { get; set; }
    }
}
