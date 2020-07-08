using System;

namespace GenderPayGap.Database
{
    [Serializable]
    public partial class SicSection
    {

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
