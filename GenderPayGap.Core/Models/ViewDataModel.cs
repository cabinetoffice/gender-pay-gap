using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Models
{
    [Serializable]
    public class ViewDataModel
    {

        public long OrganisationId { get; set; }
        public SectorTypes SectorType { get; set; }
        public string OrganisationName { get; set; }
        public string OrganisationAbbr { get; set; }
        public string Address { get; set; }
        public string SicCodes { get; set; }
        public string SicSectors { get; set; }
        public long ReturnId { get; set; }
        public DateTime AccountingDate { get; set; }
        public int OrganisationSize { get; set; }

        public SortedSet<int> SicCodeIds
        {
            get { return new SortedSet<int>(SicCodes.SplitI("," + Environment.NewLine).Select(s => s.ToInt32())); }
        }

        public string SicSectionIds { get; set; }
        
    }

}
