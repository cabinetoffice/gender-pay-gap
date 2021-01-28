using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Helpers
{
    public static class DownloadHelper
    {

        public static FileContentResult CreateCsvDownload(IEnumerable rows, string fileDownloadName)
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture))
                {
                    var options = new TypeConverterOptions {Formats = new[] {"yyyy/MM/dd hh:mm:ss"}};
                    csv.Configuration.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                    csv.Configuration.TypeConverterOptionsCache.AddOptions<DateTime?>(options);
                    csv.WriteRecords(rows);
                }
            }

            var fileContentResult = new FileContentResult(memoryStream.GetBuffer(), "text/csv")
            {
                FileDownloadName = fileDownloadName
            };
            return fileContentResult;
        }

        public static FileContentResult CreateCsvDownload(string csvFileContents, string fileDownloadName)
        {
            byte[] fileContentsBytes = Encoding.UTF8.GetBytes(csvFileContents);

            var fileContentResult = new FileContentResult(fileContentsBytes, "text/csv")
            {
                FileDownloadName = fileDownloadName
            };
            return fileContentResult;
        }

    }
}
