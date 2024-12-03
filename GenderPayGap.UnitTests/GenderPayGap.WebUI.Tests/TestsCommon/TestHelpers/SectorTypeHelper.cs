using GenderPayGap.Core;

namespace GenderPayGap.Tests.Common.TestHelpers
{
    public static class SectorTypeHelper
    {

        public static DateTime GetSnapshotDateForSector(int year, SectorTypes sector)
        {
            switch (sector)
            {
                case SectorTypes.Unknown:
                    throw new ArgumentException("Unable to provide a snapshot date when the sector type is 'Unknown'", nameof(sector));
                case SectorTypes.Private:
                    return new DateTime(year, 4, 5);
                case SectorTypes.Public:
                    return new DateTime(year, 3, 31);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sector), sector, null);
            }
        }

    }
}
