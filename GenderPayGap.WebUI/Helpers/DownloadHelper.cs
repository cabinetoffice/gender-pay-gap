using System.Collections;
using System.Text;
using CsvHelper.TypeConversion;
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
                    csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime>(options);
                    csvWriter.Context.TypeConverterOptionsCache.AddOptions<DateTime?>(options);
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

        public static string GetFormattedFileSize(long fileSizeInBytes)
        {
            double oneGigabyte = 1024 * 1024 * 1024;
            if (fileSizeInBytes > oneGigabyte)
            {
                return $"{To3SignificantFigures(fileSizeInBytes / oneGigabyte)}GB";
            }
            
            double oneMegaByte = 1024 * 1024;
            if (fileSizeInBytes > oneMegaByte)
            {
                return $"{To3SignificantFigures(fileSizeInBytes / oneMegaByte)}MB";
            }
            
            double oneKiloByte = 1024;
            if (fileSizeInBytes > oneKiloByte)
            {
                return $"{To3SignificantFigures(fileSizeInBytes / oneKiloByte)}KB";
            }
            
            return $"{To3SignificantFigures(fileSizeInBytes)} bytes";
        }

        private static string To3SignificantFigures(double fileSizeInUnits)
        {
            if (fileSizeInUnits > 100)
            {
                return fileSizeInUnits.ToString("##");
            }
            else if (fileSizeInUnits > 10)
            {
                return fileSizeInUnits.ToString("##.#");
            }
            else
            {
                return fileSizeInUnits.ToString("#.##");
            }
        }

    }
}
