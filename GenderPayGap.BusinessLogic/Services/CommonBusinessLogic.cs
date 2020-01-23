using System;
using GenderPayGap.Core;
using GenderPayGap.Extensions;
using Microsoft.Extensions.Configuration;

namespace GenderPayGap.BusinessLogic
{
    public interface ICommonBusinessLogic
    {

        DateTime PrivateAccountingDate { get; }
        DateTime PublicAccountingDate { get; }

        DateTime GetAccountingStartDate(SectorTypes sector, int year = 0);

    }

    public class CommonBusinessLogic : ICommonBusinessLogic
    {

        private readonly IConfiguration _configuration;

        public CommonBusinessLogic(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public DateTime PrivateAccountingDate => _configuration["PrivateAccountingDate"].ToDateTime();
        public DateTime PublicAccountingDate => _configuration["PublicAccountingDate"].ToDateTime();

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
                    tempDay = PrivateAccountingDate.Day;
                    tempMonth = PrivateAccountingDate.Month;
                    break;
                case SectorTypes.Public:
                    tempDay = PublicAccountingDate.Day;
                    tempMonth = PublicAccountingDate.Month;
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
