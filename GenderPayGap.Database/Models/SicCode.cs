using System.Collections.Generic;
using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SicCode
    {

        public SicCode()
        {
            OrganisationSicCodes = new HashSet<OrganisationSicCode>();
        }

        [JsonProperty]
        public int SicCodeId { get; set; }
        [JsonProperty]
        public string SicSectionId { get; set; }
        [JsonProperty]
        public string Description { get; set; }
        [JsonProperty]
        public string Synonyms { get; set; }

        public virtual SicSection SicSection { get; set; }
        public virtual ICollection<OrganisationSicCode> OrganisationSicCodes { get; set; }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var target = (SicCode) obj;
            return SicCodeId == target.SicCodeId;
        }

    }
}
