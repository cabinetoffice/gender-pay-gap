using System;
using System.Collections.Generic;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{

    [Serializable]
    public class OrganisationPublicSectorType
    {

        public long OrganisationPublicSectorTypeId { get; set; }

        public int PublicSectorTypeId { get; set; }

        public long OrganisationId { get; set; }

        public string Source { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public DateTime? Retired { get; set; }

        public virtual PublicSectorType PublicSectorType { get; set; }

        public virtual ICollection<Organisation> Organisations { get; set; }

    }
}
