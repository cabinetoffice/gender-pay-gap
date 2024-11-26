using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Models.Viewing.Download;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    public class DownloadController : Controller
    {

        private readonly IFileRepository fileRepository;
        private readonly GoogleAnalyticsTracker webTracker;

        public DownloadController(
            IFileRepository fileRepository,
            GoogleAnalyticsTracker webTracker)
        {
            this.fileRepository = fileRepository;
            this.webTracker = webTracker;
        }
        
        [HttpGet("viewing/download")]
        public IActionResult Download()
        {
            List<DownloadCsvFile> downloadCsvFiles = GetListOfCsvFilesToDownload();
            
            return View("Download", downloadCsvFiles);
        }

        private List<DownloadCsvFile> GetListOfCsvFilesToDownload()
        {
            List<int> reportingYears = ReportingYearsHelper.GetReportingYears().OrderByDescending(y => y).ToList();

            var csvFiles = new List<DownloadCsvFile>();

            foreach (int year in reportingYears)
            {
                string filePath = UpdatePublicFacingDownloadFilesJob.GetDownloadFileLocationForYear(year);

                bool fileIsAvailable = fileRepository.FileExists(filePath);
                long? fileSize = fileIsAvailable ? fileRepository.GetFileSize(filePath) : null;

                csvFiles.Add(new DownloadCsvFile
                {
                    ReportingYear = year,
                    FileIsAvailable = fileIsAvailable,
                    FileSize = fileSize
                });
            }

            return csvFiles;
        }

        [HttpGet("viewing/download-data")]
        [HttpGet("viewing/download-data/{year}")]
        public IActionResult DownloadData(int? year = null)
        {
            if (year == null)
            {
                year = SectorTypes.Private.GetAccountingStartDate().Year;
            }

            if (!ReportingYearsHelper.GetReportingYears().Contains(year.Value))
            {
                throw new PageNotFoundException();
            }

            string filePath = UpdatePublicFacingDownloadFilesJob.GetDownloadFileLocationForYear(year.Value);

            if (!fileRepository.FileExists(filePath))
            {
                throw new PageNotFoundException();
            }
            
            string fileContents = fileRepository.Read(filePath);

            string userFacingDownloadFileName = $"UK Gender Pay Gap Data - {year.Value} to {year.Value + 1}.csv";

            //Track the download 
            webTracker.TrackPageView(this, userFacingDownloadFileName);

            return DownloadHelper.CreateCsvDownload(fileContents, userFacingDownloadFileName);
        }

    }
}
