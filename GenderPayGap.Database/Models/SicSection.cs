using Newtonsoft.Json;

namespace GenderPayGap.Database
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SicSection
    {

        public SicSection()
        {
            SicCodes = new HashSet<SicCode>();
        }

        [JsonProperty]
        public string SicSectionId { get; set; }
        [JsonProperty]
        public string Description { get; set; }

        public virtual ICollection<SicCode> SicCodes { get; set; }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var target = (SicSection) obj;
            return SicSectionId == target.SicSectionId;
        }

    }
}
