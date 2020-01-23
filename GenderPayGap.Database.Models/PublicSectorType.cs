using System;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{

    [Serializable]
    public class PublicSectorType
    {

        public int PublicSectorTypeId { get; set; }

        public string Description { get; set; }

        public DateTime Created { get; set; } = VirtualDateTime.Now;

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var target = (PublicSectorType) obj;
            return PublicSectorTypeId == target.PublicSectorTypeId;
        }

    }

}
