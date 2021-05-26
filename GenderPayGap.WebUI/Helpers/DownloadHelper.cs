using System;
using System.Collections;
using System.IO;
using System.Text;
using CsvHelper.TypeConversion;
using GenderPayGap.Core.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Helpers
{
    public static class DownloadHelper
    {

        public static FileContentResult CreateCsvDownload(IEnumerable rows, string fileDownloadName)
        {
            MemoryStream stream = CsvWriter.Write(
                (memoryStream, streamReader, streamWriter, csvWriter) =>
                {
                    var options = new TypeConverterOptions {Formats = new[] {"yyyy/MM/dd hh:mm:ss"}};
                    csvWriter.Configuration.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                    csvWriter.Configuration.TypeConverterOptionsCache.AddOptions<DateTime?>(options);
                    csvWriter.WriteRecords(rows);
                    return memoryStream;
                });

            return CreateCsvFile(stream.GetBuffer(), fileDownloadName);
        }

        public static FileContentResult CreateCsvDownload(string csvFileContents, string fileDownloadName)
        {
            byte[] fileContentsBytes = Encoding.UTF8.GetBytes(csvFileContents);

            return CreateCsvFile(fileContentsBytes, fileDownloadName);
        }

        private static FileContentResult CreateCsvFile(byte[] fileContentsBytes, string fileDownloadName)
        {
            var fileContentResult = new FileContentResult(fileContentsBytes, "text/csv")
            {
                FileDownloadName = fileDownloadName
            };
            return fileContentResult;
        }

    }
}
