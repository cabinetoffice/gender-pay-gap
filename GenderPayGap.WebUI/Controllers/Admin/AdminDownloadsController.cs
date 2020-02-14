using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        
        [HttpGet("downloads-new/orphan-organisations")]
        public FileContentResult DownloadOrphanOrganisations()
        {
            DateTime pinExpiresDate = Global.PinExpiresDate;

            List<Organisation> orphanOrganisations = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Where(org => org.LatestScope == null ||
                              org.LatestScope.ScopeStatus == ScopeStatuses.InScope ||
                              org.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope)
                .Where(org => org.UserOrganisations == null ||
                              !org.UserOrganisations.Any(uo => uo.PINConfirmedDate != null // Registration complete
                                                               || uo.Method == RegistrationMethods.Manual // Manual registration
                                                               || (uo.Method == RegistrationMethods.PinInPost // PITP registration in progress
                                                               && uo.PINSentDate.HasValue
                                                               && uo.PINSentDate.Value > pinExpiresDate)))
                .ToList();

            var records = orphanOrganisations.Select(
                    org => new
                    {
                        org.OrganisationId,
                        org.OrganisationName,
                        Address = org.GetAddressString(),
                        Sector = org.SectorType,
                        ReportingDeadline = org.SectorType.GetAccountingStartDate().AddYears(1).AddDays(-1),
                        ScopeStatus = org.GetCurrentScope()?.ScopeStatus,
                        HasSubmitted = org.LatestReturn?.Status == ReturnStatuses.Submitted
                    })
                .ToList();

            string fileDownloadName = $"Gpg-OrphanOrganisations-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads-new/late-submissions")]
        public FileContentResult DownloadLateSubmissions()
        {
            List<Organisation> organisationsWithLateReturns = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Where(
                    org => org.LatestScope == null
                           || org.LatestScope.ScopeStatus == ScopeStatuses.InScope
                           || org.LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope)
                .Where(org => org.LatestReturn == null || org.LatestReturn.IsLateSubmission)
                .Include(org => org.Returns)
                .ToList();

            var records = organisationsWithLateReturns.Select(
                    org => new
                    {
                        org.OrganisationId,
                        org.OrganisationName,
                        org.SectorType,
                        Submitted = org.LatestReturn != null,
                        ReportingDeadline = org.SectorType.GetAccountingStartDate().AddYears(1).AddDays(-1).ToString("d MMMM yyyy"),
                        SubmittedDate = org.LatestReturn?.Created,
                        ModifiedDate = org.LatestReturn?.Modified,
                        org.LatestReturn?.LateReason

                    })
                .ToList();

            string fileDownloadName = $"Gpg-LateSubmissions-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads-new/user-consent-send-updates")]
        public FileContentResult DownloadUserConsentSendUpdates()
        {
            List<User> users = dataRepository.GetAll<User>()
                .Where(user => user.Status == UserStatuses.Active)
                .Where(user => user.UserSettings.Any(us => us.Key == UserSettingKeys.SendUpdates && us.Value.ToLower() == "true"))
                .ToList();

            var records = users.Select(
                    u => new
                    {
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
                    u => new
                    {
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
