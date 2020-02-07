using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using GenderPayGap.Core;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GenderPayGap.WebUI.Controllers
{
    [Authorize(Roles = "GPGadmin")]
    [Route("admin")]
    public class AdminDownloadsController : Controller
    {

        private readonly IDataRepository dataRepository;

        public AdminDownloadsController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("downloads-new")]
        public IActionResult Downloads()
        {
            return View("Downloads");
        }
        
        [HttpGet("downloads-new/user-consent-send-updates")]
        public FileContentResult DownloadUserConsentSendUpdates()
        {
            List<User> users = dataRepository.GetAll<User>()
                .Where(user => user.Status == UserStatuses.Active)
                .Where(user => user.UserSettings.Any(us => us.Key == UserSettingKeys.SendUpdates && us.Value.ToLower() == "true"))
                .ToList();

            var records = users.Select(
                    u => new {
                        u.Firstname,
                        u.Lastname,
                        u.JobTitle,
                        u.EmailAddress,
                        u.ContactFirstName,
                        u.ContactLastName,
                        u.ContactJobTitle,
                        u.ContactEmailAddress,
                        u.ContactPhoneNumber,
                        u.ContactOrganisation
                    })
                .ToList();

            string fileDownloadName = $"Gpg-UserConsent-SendUpdates-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads-new/user-consent-allow-contact-for-feedback")]
        public FileContentResult DownloadUserConsentAllowContactForFeedback()
        {
            List<User> users = dataRepository.GetAll<User>()
                .Where(user => user.Status == UserStatuses.Active)
                .Where(user => user.UserSettings.Any(us => us.Key == UserSettingKeys.AllowContact && us.Value.ToLower() == "true"))
                .ToList();

            var records = users.Select(
                    u => new {
                        u.Firstname,
                        u.Lastname,
                        u.JobTitle,
                        u.EmailAddress,
                        u.ContactFirstName,
                        u.ContactLastName,
                        u.ContactJobTitle,
                        u.ContactEmailAddress,
                        u.ContactPhoneNumber,
                        u.ContactOrganisation
                    })
                .ToList();

            string fileDownloadName = $"Gpg-UserConsent-AllowContactForFeedback-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        private static FileContentResult CreateCsvDownload(IEnumerable rows, string fileDownloadName)
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
