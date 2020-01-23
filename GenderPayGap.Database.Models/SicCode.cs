using System;
using System.Collections.Generic;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{
    public partial class SicCode
    {

        public SicCode()
        {
            OrganisationSicCodes = new HashSet<OrganisationSicCode>();
        }

        public int SicCodeId { get; set; }
        public string SicSectionId { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public virtual SicSection SicSection { get; set; }
        public virtual ICollection<OrganisationSicCode> OrganisationSicCodes { get; set; }

    }
}
