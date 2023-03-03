using System;
using System.Data;
using CsvHelper.Configuration;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Extensions;

namespace GenderPayGap.Core.Classes
{
    public static class Extensions
    {

        /// <summary>
        ///     Returns the accounting start date for the specified sector and year
        /// </summary>
        /// <param name="sectorType">The sector type of the organisation</param>
        /// <param name="year">The starting year of the accounting period. If 0 then uses current accounting period</param>
        public static DateTime GetAccountingStartDate(this SectorTypes sectorType, int year = 0)
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

        public static string ToCSV(this DataTable datatable)
        {
            return CsvWriter.Write(
                (memoryStream, streamReader, streamWriter, csvWriter) =>
                {
                    csvWriter.Configuration.ShouldQuote = (s, context) => true;
                    csvWriter.Configuration.TrimOptions = TrimOptions.InsideQuotes;

                    // Write columns
                    foreach (DataColumn column in datatable.Columns) // copy datatable CHAIN to DT, or just use CHAIN
                    {
                        csvWriter.WriteField(column.ColumnName);
                    }

                    csvWriter.NextRecord();

                    // Write row values
                    foreach (DataRow row1 in datatable.Rows)
                    {
                        for (var i = 0; i < datatable.Columns.Count; i++)
                        {
                            csvWriter.WriteField(row1[i]);
                        }

                        csvWriter.NextRecord();
                    }

                    streamWriter.Flush();
                    memoryStream.Position = 0;
                    return streamReader.ReadToEnd();
                });
        }

    }
}
