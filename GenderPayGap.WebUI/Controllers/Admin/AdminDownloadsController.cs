using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using CsvHelper;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Database.Models;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Models.Admin;
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

        [HttpGet("downloads")]
        public IActionResult Downloads()
        {
            int firstReportingYear = Global.FirstReportingYear;
            int currentReportingYear = SectorTypes.Public.GetAccountingStartDate().Year;
            int numberOfYears = currentReportingYear - firstReportingYear + 1;

            var viewModel = new AdminDownloadsViewModel
            {
                ReportingYears = Enumerable.Range(firstReportingYear, numberOfYears).Reverse().ToList()
            };

            return View("Downloads", viewModel);
        }
        
        [HttpGet("downloads/all-organisations")]
        public FileContentResult DownloadAllOrganisations()
        {
            List<Organisation> allOrganisations = dataRepository.GetAll<Organisation>()
                .Include(org => org.LatestAddress)
                .Include(org => org.OrganisationSicCodes)
                .ToList();

            var records = allOrganisations.Select(
                    org => new
                    {
                        org.OrganisationId,
                        org.OrganisationName,
                        org.CompanyNumber,
                        Sector = org.SectorType,
                        Status = org.Status,
                        Address = org.LatestAddress?.GetAddressString(),
                        SicCodes = org.GetSicCodeIdsString(),
                        Created = org.Created,
                    })
                .ToList();

            string fileDownloadName = $"Gpg-AllOrganisations-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/orphan-organisations")]
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

        [HttpGet("downloads/organisation-addresses")]
        public FileContentResult DownloadOrganisationAddresses()
        {
            DateTime pinExpiresDate = Global.PinExpiresDate;

            List<Organisation> organisationAddresses = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Where(org => org.LatestAddress != null)
                .Include(org => org.LatestAddress)
                .ToList();

            var records = organisationAddresses.Select(
                    org => new
                    {
                        org.OrganisationId,
                        org.OrganisationName,

                        org.LatestAddress.PoBox,
                        org.LatestAddress.Address1,
                        org.LatestAddress.Address2,
                        org.LatestAddress.Address3,
                        org.LatestAddress.TownCity,
                        org.LatestAddress.County,
                        org.LatestAddress.Country,
                        org.LatestAddress.PostCode,
                    })
                .ToList();

            string fileDownloadName = $"Gpg-OrganisationAddresses-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/organisation-scopes-for-{year}")]
        public FileContentResult DownloadOrganisationScopesForYear(int year)
        {
            List<Organisation> organisationsWithScopesForYear = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Where(org => org.OrganisationScopes.Any(scope => scope.SnapshotDate.Year == year && scope.Status == ScopeRowStatuses.Active))
                .Include(org => org.OrganisationScopes)
                .ToList();

            var records = organisationsWithScopesForYear.Select(
                    org => new
                    {
                        org.OrganisationId,
                        org.OrganisationName,
                        org.GetScopeForYear(year).ScopeStatus,
                        DateScopeLastChanged = org.GetScopeForYear(year).ScopeStatusDate,
                    })
                .ToList();

            string fileDownloadName = $"Gpg-OrganisationScopesForYear-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/all-submissions-for-{year}")]
        public FileContentResult DownloadAllSubmissionsForYear(int year)
        {
            List<Organisation> organisationsWithReturnsForYear = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Where(org =>
                    org.Returns.Any(ret => ret.Status == ReturnStatuses.Submitted
                                           && ret.AccountingDate.Year == year))
                .Include(org => org.OrganisationScopes)
                .Include(org => org.Returns)
                .ToList();

            var records = organisationsWithReturnsForYear.Select(
                    org =>
                    {
                        OrganisationScope scopeForYear = org.GetScopeForYear(year);
                        Return returnForYear = org.GetReturn(year);

                        return new
                        {
                            org.OrganisationId,
                            org.OrganisationName,
                            org.CompanyNumber,
                            org.SectorType,
                            ScopeStatus = scopeForYear?.ScopeStatus.ToString() ?? "(no active scope)",

                            SnapshotDate = returnForYear.AccountingDate,
                            DeadlineDate = returnForYear.AccountingDate.AddYears(1).AddDays(-1),
                            ModifiedDate = returnForYear.Modified,
                            returnForYear.IsLateSubmission,

                            returnForYear.DiffMeanHourlyPayPercent,
                            returnForYear.DiffMedianHourlyPercent,

                            LowerQuartileFemalePercent = returnForYear.FemaleLowerPayBand,
                            LowerQuartileMalePercent = returnForYear.MaleLowerPayBand,
                            LowerMiddleQuartileFemalePercent = returnForYear.FemaleMiddlePayBand,
                            LowerMiddleQuartileMalePercent = returnForYear.MaleMiddlePayBand,
                            UpperMiddleQuartileFemalePercent = returnForYear.FemaleUpperPayBand,
                            UpperMiddleQuartileMalePercent = returnForYear.MaleUpperPayBand,
                            UpperQuartileFemalePercent = returnForYear.FemaleUpperQuartilePayBand,
                            UpperQuartileMalePercent = returnForYear.MaleUpperQuartilePayBand,

                            PercentPaidBonusFemale = returnForYear.FemaleMedianBonusPayPercent,
                            PercentPaidBonusMale = returnForYear.MaleMedianBonusPayPercent,
                            returnForYear.DiffMeanBonusPercent,
                            returnForYear.DiffMedianBonusPercent,

                            returnForYear.CompanyLinkToGPGInfo,
                            returnForYear.ResponsiblePerson,
                            returnForYear.OrganisationSize.GetAttribute<DisplayAttribute>().Name,
                        };
                    })
                .ToList();

            string fileDownloadName = $"Gpg-AllSubmissionsForYear-{year}--{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/late-submissions")]
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
        
        [HttpGet("downloads/all-users")]
        public FileContentResult DownloadAllUsers()
        {
            List<User> users = dataRepository.GetAll<User>().ToList();

            var records = users.Select(
                    u => new
                    {
                        u.UserId,
                        u.Firstname,
                        u.Lastname,
                        u.JobTitle,
                        u.EmailAddress,
                        u.EmailVerifySendDate,
                        u.EmailVerifiedDate,
                        u.Status,
                        u.StatusDate,
                        u.StatusDetails,
                        u.Created
                    })
                .ToList();

            string fileDownloadName = $"Gpg-AllUsers-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }
        
        [HttpGet("downloads/user-organisation-registrations")]
        public FileContentResult DownloadUserOrganisationRegistrations()
        {
            List<UserOrganisation> userOrganisations = dataRepository.GetAll<UserOrganisation>()
                .Include(uo => uo.Organisation.LatestScope)
                .Include(uo => uo.User)
                .ToList();

            var records = userOrganisations.Select(
                    uo => new
                    {
                        uo.OrganisationId,
                        uo.UserId,
                        uo.Organisation.OrganisationName,
                        uo.Organisation.CompanyNumber,
                        uo.Organisation.SectorType,
                        uo.Method,
                        uo.User.Firstname,
                        uo.User.Lastname,
                        uo.User.JobTitle,
                        uo.User.EmailAddress,
                        uo.PINSentDate,
                        uo.PINConfirmedDate,
                        uo.Created
                    })
                .ToList();

            string fileDownloadName = $"Gpg-UserOrganisationRegistrations-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }
        
        [HttpGet("downloads/unverified-user-organisation-registrations")]
        public FileContentResult DownloadUnverifiedUserOrganisationRegistrations()
        {
            List<UserOrganisation> userOrganisations = dataRepository.GetEntities<UserOrganisation>() 
                .Where(uo => uo.PINConfirmedDate == null) 
                .Include(uo => uo.Organisation.LatestScope) 
                .Include(uo => uo.User) 
                .ToList(); 
            var records = userOrganisations.Select(
                    uo => new
                    {
                        uo.OrganisationId,
                        uo.UserId,
                        uo.Organisation.OrganisationName,
                        uo.Organisation.CompanyNumber,
                        uo.Organisation.SectorType,
                        uo.Method,
                        uo.User.Firstname,
                        uo.User.Lastname,
                        uo.User.JobTitle,
                        uo.User.EmailAddress,
                        uo.PINSentDate,
                        uo.PINConfirmedDate,
                        uo.Created
                    })
                .ToList();

            string fileDownloadName = $"Gpg-UnverifiedUserOrganisationRegistrations-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/user-consent-send-updates")]
        public FileContentResult DownloadUserConsentSendUpdates()
        {
            List<User> users = dataRepository.GetAll<User>()
                .Where(user => user.Status == UserStatuses.Active)
                .Where(user => user.UserSettings.Any(us => us.Key == UserSettingKeys.SendUpdates && us.Value.ToLower() == "true"))
                .ToList();

            var records = users.Select(
                    u => new
                    {
                        u.UserId,
                        u.Firstname,
                        u.Lastname,
                        u.JobTitle,
                        u.EmailAddress,
                    })
                .ToList();

            string fileDownloadName = $"Gpg-UserConsent-SendUpdates-{DateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/user-consent-allow-contact-for-feedback")]
        public FileContentResult DownloadUserConsentAllowContactForFeedback()
        {
            List<User> users = dataRepository.GetAll<User>()
                .Where(user => user.Status == UserStatuses.Active)
                .Where(user => user.UserSettings.Any(us => us.Key == UserSettingKeys.AllowContact && us.Value.ToLower() == "true"))
                .ToList();

            var records = users.Select(
                    u => new
                    {
                        u.UserId,
                        u.Firstname,
                        u.Lastname,
                        u.JobTitle,
                        u.EmailAddress,
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
