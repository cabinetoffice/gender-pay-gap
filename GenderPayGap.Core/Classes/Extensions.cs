﻿using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using GenderPayGap.Core.Interfaces;
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

        #region FileSystem


        
        /// <summary>
        ///     Save records to remote CSV via temporary local storage
        /// </summary>
        /// <param name="records">collection of records to write</param>
        /// <param name="filePath">the remote location of the file to save overwrite</param>
        /// <param name="oldfilePath">the previous file (if any) to be deleted on successful copy</param>
        public static async Task SaveCSVAsync(this IFileRepository fileRepository,
            IEnumerable records,
            string filePath,
            string oldfilePath = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            var tempfile = new FileInfo(Path.GetTempFileName());
            try
            {
                using (StreamWriter textWriter = tempfile.CreateText())
                {
                    var config = new CsvConfiguration {QuoteAllFields = true, TrimFields = true, TrimHeaders = true};
                    using (var writer = new CsvWriter(textWriter, config))
                    {
                        writer.WriteRecords(records);
                    }
                }

                //Save CSV to storage
                await fileRepository.WriteAsync(filePath, tempfile);


                //Delete the old file if it exists
                if (!string.IsNullOrWhiteSpace(oldfilePath)
                    && await fileRepository.GetFileExistsAsync(oldfilePath)
                    && !filePath.EqualsI(oldfilePath))
                {
                    await fileRepository.DeleteFileAsync(oldfilePath);
                }
            }
            finally
            {
                File.Delete(tempfile.FullName);
            }
        }

        public static string ToCSV(this DataTable datatable)
        {
            using (var stream = new MemoryStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    using (var textWriter = new StreamWriter(stream))
                    {
                        var config = new CsvConfiguration {QuoteAllFields = true, TrimFields = true, TrimHeaders = true};
                        using (var writer = new CsvWriter(textWriter, config))
                        {
                            // Write columns
                            foreach (DataColumn column in datatable.Columns) //copy datatable CHAIN to DT, or just use CHAIN
                            {
                                writer.WriteField(column.ColumnName);
                            }

                            writer.NextRecord();

                            // Write row values
                            foreach (DataRow row1 in datatable.Rows)
                            {
                                for (var i = 0; i < datatable.Columns.Count; i++)
                                {
                                    writer.WriteField(row1[i]);
                                }

                                writer.NextRecord();
                            }

                            textWriter.Flush();
                            stream.Position = 0;
                            return reader.ReadToEnd();
                        }
                    }
                }
            }
        }

        #endregion

    }
}
