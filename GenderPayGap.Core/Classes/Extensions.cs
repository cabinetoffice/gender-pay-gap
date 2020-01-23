using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
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

        public static async Task<List<T>> ReadCSVAsync<T>(this IFileRepository fileRepository, string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            string content = await fileRepository.ReadAsync(filePath);
            return ReadCSV<T>(content);
        }

        public static List<T> ReadCSV<T>(string content)
        {
            using (var codeStream = new MemoryStream(Encoding.UTF8.GetBytes(content)))
            using (var reader = new StreamReader(codeStream, Encoding.UTF8))
            {
                var csvReader = new CsvReader(reader);
                csvReader.Configuration.WillThrowOnMissingField = false;
                csvReader.Configuration.TrimFields = true;
                csvReader.Configuration.IgnoreQuotes = false;

                try
                {
                    return csvReader.GetRecords<T>().ToList();
                }
                catch (CsvTypeConverterException ex)
                {
                    if (ex.Data.Count > 0)
                    {
                        throw new CsvBadDataException(ex.Data.Values.ToList<string>()[0]);
                    }

                    //ex.Data.Values has more info...
                    throw;
                }
            }
        }

        public static DataTable ToDataTable(this string csvContent)
        {
            var table = new DataTable();

            using (TextReader sr = new StringReader(csvContent))
            {
                using (var csv = new CsvReader(sr))
                {
                    csv.ReadHeader();
                    foreach (string header in csv.FieldHeaders)
                    {
                        table.Columns.Add(header);
                    }

                    while (csv.Read())
                    {
                        DataRow row = table.NewRow();
                        foreach (DataColumn col in table.Columns)
                        {
                            row[col.ColumnName] = csv.GetField(col.DataType, col.ColumnName);
                        }

                        table.Rows.Add(row);
                    }
                }
            }

            return table;
        }

        /// <summary>
        ///     Save records to remote CSV via temporary local storage
        /// </summary>
        /// <param name="records">collection of records to write</param>
        /// <param name="filePath">the remote location of the file to save overwrite</param>
        /// <param name="oldfilePath">the previous file (if any) to be deleted on successful copy</param>
        public static async Task<long> SaveCSVAsync(this IFileRepository fileRepository,
            IEnumerable records,
            string filePath,
            string oldfilePath = null)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            long size = 0;
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

                size = await fileRepository.GetFileSizeAsync(filePath);


                //Set the count in the metadata file
                var count = 0;
                foreach (object item in records)
                {
                    count++;
                }

                await fileRepository.SetMetaDataAsync(filePath, "RecordCount", count.ToString());

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

            return size;
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

        public static async Task AppendCsvRecordsAsync<T>(this IFileRepository fileRepository, string filePath, IEnumerable<T> records)
        {
            if (records == null || !records.Any())
            {
                return;
            }

            DataTable table = records.ToDataTable();

            using (var textWriter = new StringWriter())
            {
                var config = new CsvConfiguration {QuoteAllFields = true, TrimFields = true, TrimHeaders = true};

                using (var writer = new CsvWriter(textWriter, config))
                {
                    if (!await fileRepository.GetFileExistsAsync(filePath))
                    {
                        for (var c = 0; c < table.Columns.Count; c++)
                        {
                            writer.WriteField(table.Columns[c].ColumnName);
                        }

                        writer.NextRecord();
                    }
                    else
                    {
                        writer.Configuration.HasHeaderRecord = false;
                    }

                    foreach (DataRow row in table.Rows)
                    {
                        for (var c = 0; c < table.Columns.Count; c++)
                        {
                            writer.WriteField(row[c].ToString());
                        }

                        writer.NextRecord();
                    }
                }

                string appendString = textWriter.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(appendString))
                {
                    await fileRepository.AppendAsync(filePath, appendString + Environment.NewLine);

                    //Increase the count in the metadata file
                    string metaData = await fileRepository.GetMetaDataAsync(filePath, "RecordCount");
                    int count = metaData.ToInt32();
                    count += records.Count();
                    await fileRepository.SetMetaDataAsync(filePath, "RecordCount", count.ToString());
                }
            }
        }

        public static async Task AppendCsvRecordAsync<T>(this IFileRepository fileRepository, string filePath, T record)
        {
            await AppendCsvRecordsAsync(fileRepository, filePath, new[] {record});
        }

        /// <summary>
        ///     pushes a local file to remote if it doesnt already exist
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="remotePath">The path to the remote file</param>
        /// <param name="localPath">The path to the local file (default=</param>
        public static async Task PushRemoteFileAsync(IFileRepository fileRepository,
            string fileName,
            string remotePath,
            string localPath = null)
        {
            if (string.IsNullOrWhiteSpace(remotePath))
            {
                throw new ArgumentNullException(nameof(remotePath));
            }

            if (!await fileRepository.GetDirectoryExistsAsync(remotePath))
            {
                await fileRepository.CreateDirectoryAsync(remotePath);
            }

            remotePath = Path.Combine(remotePath, fileName);
            bool remoteExists = await fileRepository.GetFileExistsAsync(remotePath);
            if (remoteExists)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(localPath))
            {
                localPath = "App_Data";
            }

            localPath = FileSystem.ExpandLocalPath(localPath);
            localPath = Path.Combine(localPath, fileName);
            bool localExists = File.Exists(localPath);
            byte[] localContent = localExists ? File.ReadAllBytes(localPath) : null;


            //Overwrite remote 
            if (localContent != null && localContent.Length > 0)
            {
                await fileRepository.WriteAsync(remotePath, localContent);
            }
        }

        #endregion

    }
}
