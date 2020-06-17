using System;
using GenderPayGap.Core;
using GenderPayGap.Extensions;

namespace GenderPayGap.BusinessLogic
{
    public interface ICommonBusinessLogic
    {
        DateTime GetAccountingStartDate(SectorTypes sector, int year = 0);
    }

    public class CommonBusinessLogic : ICommonBusinessLogic
    {

        /// <summary>
        ///     Returns the accounting start date for the specified sector and year
        /// </summary>
        /// <param name="sectorType">The sector type of the organisation</param>
        /// <param name="year">The starting year of the accounting period. If 0 then uses current accounting period</param>
        /// <returns></returns>
        public DateTime GetAccountingStartDate(SectorTypes sectorType, int year = 0)
        {
            var tempDay = 0;
            var tempMonth = 0;

            DateTime now = VirtualDateTime.Now;

            switch (sectorType)
            {
                case SectorTypes.Private:
                    tempDay = Global.PrivateAccountingDate.Day;
                    tempMonth = Global.PrivateAccountingDate.Month;
                    break;
                case SectorTypes.Public:
                    tempDay = Global.PublicAccountingDate.Day;
                    tempMonth = Global.PublicAccountingDate.Month;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(sectorType),
                        sectorType,
                        "Cannot calculate accounting date for this sector type");
            }

            if (year == 0)
            {
                year = now.Year;
            }

            var tempDate = new DateTime(year, tempMonth, tempDay);

            return now > tempDate ? tempDate : tempDate.AddYears(-1);
        }

    }

}
