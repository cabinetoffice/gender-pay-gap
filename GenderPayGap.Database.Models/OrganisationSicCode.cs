using System;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{
    public class OrganisationSicCode
    {

        public int SicCodeId { get; set; }
        public long OrganisationId { get; set; }
        public DateTime Created { get; set; } = VirtualDateTime.Now;
        public long OrganisationSicCodeId { get; set; }
        public string Source { get; set; }
        public DateTime? Retired { get; set; }

        public virtual Organisation Organisation { get; set; }
        public virtual SicCode SicCode { get; set; }

    }
}
