namespace GenderPayGap.WebUI.Models.Viewing.Download
{
    public class DownloadCsvFile
    {

        public int ReportingYear { get; set; }
        public bool FileIsAvailable { get; set; }
        public long? FileSize { get; set; }

    }
}
