using System;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class BadSicLogModel
    {

        public DateTime Date { get; set; } = VirtualDateTime.Now;
        public long OrganisationId { get; set; }
        public string OrganisationName { get; set; }
        public int SicCode { get; set; }
        public string Source { get; set; }

    }
}
