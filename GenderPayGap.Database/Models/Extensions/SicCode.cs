using System;

namespace GenderPayGap.Database
{
    [Serializable]
    public partial class SicCode
    {

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
