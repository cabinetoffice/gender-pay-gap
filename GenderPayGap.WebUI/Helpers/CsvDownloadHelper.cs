using System.Collections;
using System.IO;
using CsvHelper;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Helpers
{
    public static class CsvDownloadHelper
    {

        public static FileContentResult CreateCsvDownload(IEnumerable rows, string fileDownloadName)
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream))
            {
                using (var csv = new CsvWriter(writer))
                {
                    csv.WriteRecords(rows);
                }
            }

            var fileContentResult = new FileContentResult(memoryStream.GetBuffer(), "text/csv")
            {
                FileDownloadName = fileDownloadName
            };
            return fileContentResult;
        }

    }
}
