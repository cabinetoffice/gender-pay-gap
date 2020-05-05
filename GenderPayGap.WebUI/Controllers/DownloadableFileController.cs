using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes.Logger;
using GenderPayGap.Core.Filters;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Interfaces.Downloadable;
using GenderPayGap.Core.Models.Downloadable;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Classes;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers.Administration
{
    [Route("~/")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class DownloadableFileController : BaseController
    {

        private readonly IDownloadableFileBusinessLogic _downloadableFileBusinessLogic;

        #region Constructors

        public DownloadableFileController(IHttpCache cache,
            IHttpSession session,
            IDataRepository dataRepository,
            IWebTracker webTracker,
            IDownloadableFileBusinessLogic downloadableFileBusinessLogic) : base(cache, session, dataRepository, webTracker)
        {
            _downloadableFileBusinessLogic = downloadableFileBusinessLogic;
        }

        #endregion

        #region File Download Action

        [Route("download")]
        [AllowOnlyTrustedIps(AllowOnlyTrustedIps.IpRangeTypes.EhrcIPRange)]
        [ResponseCache(CacheProfileName = "Download")]
        public async Task<IActionResult> DownloadFile(string p)
        {
            IActionResult result;

            try
            {
                DownloadableFileModel downloadableFile = await _downloadableFileBusinessLogic.GetFileRemovingSensitiveInformationAsync(p);
                result = File(downloadableFile.ByteArrayContent, downloadableFile.ContentType, downloadableFile.Filename);
            }
            catch (ArgumentException argumentException)
            {
                result = BadRequest();
                CustomLogger.Error(argumentException.Message, argumentException);
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                result = NotFound();
                CustomLogger.Error(fileNotFoundException.Message, fileNotFoundException);
            }
            catch (DirectoryNotFoundException directoryNotFoundException)
            {
                result = NotFound();
                CustomLogger.Error(directoryNotFoundException.Message, directoryNotFoundException);
            }

            return result;
        }

        #endregion

        [HttpGet("admin/WebsiteLogs")]
        public async Task<IActionResult> WebsiteLogs(string fp)
        {
            IEnumerable<IDownloadableItem> downloadViewModelToReturn = await FetchDownloadablesFromSubfolderAsync(fp, "GenderPayGap.WebUI");
            return View("WebsiteLogs", downloadViewModelToReturn);
        }

        [HttpGet("admin/WebjobLogs")]
        public async Task<IActionResult> WebjobLogs(string fp)
        {
            IEnumerable<IDownloadableItem> downloadViewModelToReturn =
                await FetchDownloadablesFromSubfolderAsync(fp, "GenderPayGap.WebJob");
            return View("WebjobLogs", downloadViewModelToReturn);
        }

        [HttpGet("admin/IdentityLogs")]
        public async Task<IActionResult> IdentityLogs(string fp)
        {
            IEnumerable<IDownloadableItem> downloadViewModelToReturn =
                await FetchDownloadablesFromSubfolderAsync(fp, "GenderPayGap.IdentityServer4");
            return View("IdentityLogs", downloadViewModelToReturn);
        }

        private async Task<IEnumerable<IDownloadableItem>> FetchDownloadablesFromSubfolderAsync(string fp, string subfolderName)
        {
            //var logsPathToProcess = string.IsNullOrEmpty(fp)
            //    ? Path.Combine(Global.LogPath, subfolderName)
            //    : fp;

            // Storage explorer, we DO want to change
            string logsPathToProcess = string.IsNullOrWhiteSpace(fp)
                ? Path.Combine(Global.LogPath, subfolderName).Replace("\\", "/")
                : fp;

            IEnumerable<IDownloadableItem> listOfDownloadableItems =
                await _downloadableFileBusinessLogic.GetListOfDownloadableItemsFromPathAsync(logsPathToProcess);
            return listOfDownloadableItems;
        }

    }
}
