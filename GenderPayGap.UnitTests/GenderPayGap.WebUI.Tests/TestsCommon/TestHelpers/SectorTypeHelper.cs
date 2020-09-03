using System;
using GenderPayGap.Core;

namespace GenderPayGap.Tests.Common.TestHelpers
{
    public static class SectorTypeHelper
    {

        public static DateTime GetSnapshotDateForSector(int year, OrganisationSectors sector)
        {
            switch (sector)
            {
                case OrganisationSectors.Unknown:
                    throw new ArgumentException("Unable to provide a snapshot date when the sector type is 'Unknown'", nameof(sector));
                case OrganisationSectors.Private:
                    return new DateTime(year, 4, 5);
                case OrganisationSectors.Public:
                    return new DateTime(year, 3, 31);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sector), sector, null);
            }
        }

    }
}
