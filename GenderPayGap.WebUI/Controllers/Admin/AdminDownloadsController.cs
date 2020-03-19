using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;
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

        [HttpGet("downloads-new")]
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
        
        [HttpGet("downloads-new/all-organisations")]
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

            string fileDownloadName = $"Gpg-AllOrganisations-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
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

            string fileDownloadName = $"Gpg-OrphanOrganisations-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads-new/organisation-addresses")]
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

            string fileDownloadName = $"Gpg-OrganisationAddresses-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads-new/organisation-scopes-for-{year}")]
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

            string fileDownloadName = $"Gpg-OrganisationScopesForYear-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads-new/all-submissions-for-{year}")]
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

            List<object> records = organisationsWithReturnsForYear.Select(
                    org =>
                    {
                        Return returnForYear = org.GetReturn(year);

                        return ConvertReturnToDownloadFormat(returnForYear);
                    })
                .ToList();

            string fileDownloadName = $"Gpg-AllSubmissionsForYear-{year}--{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads-new/full-submission-history-for-{year}")]
        public FileContentResult DownloadFullSubmissionHistoryForYear(int year)
        {
            List<Organisation> organisationsWithReturnsForYear = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Include(org => org.OrganisationScopes)
                .Include(org => org.Returns)
                .ToList();

            List<Return> returnsForYear = organisationsWithReturnsForYear
                .SelectMany(organisation => organisation.Returns)
                .Where(ret => ret.AccountingDate.Year == year)
                .ToList();

            List<object> records = returnsForYear
                .Select(ConvertReturnToDownloadFormat)
                .ToList();

            string fileDownloadName = $"Gpg-FullSubmissionHistoryForYear-{year}--{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        private static object ConvertReturnToDownloadFormat(Return returnForYear)
        {
            Organisation organisation = returnForYear.Organisation;

            int year = returnForYear.AccountingDate.Year;
            OrganisationScope scopeForYear = organisation.GetScopeForYear(year);

            return new
            {
                OrganisationId = organisation.OrganisationId,
                ReturnId = returnForYear.ReturnId,
                ReturnStatus = returnForYear.Status,

                OrganisationName = organisation.OrganisationName,
                CompanyNumber = organisation.CompanyNumber,
                SectorType = organisation.SectorType,
                ScopeStatus = scopeForYear?.ScopeStatus.ToString() ?? "(no active scope)",

                SnapshotDate = returnForYear.AccountingDate,
                DeadlineDate = returnForYear.AccountingDate.AddYears(1).AddDays(-1),
                ModifiedDate = returnForYear.Modified,
                IsLateSubmission = returnForYear.IsLateSubmission,

                DiffMeanHourlyPayPercent = returnForYear.DiffMeanHourlyPayPercent,
                DiffMedianHourlyPercent = returnForYear.DiffMedianHourlyPercent,

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
                DiffMeanBonusPercent = returnForYear.DiffMeanBonusPercent,
                DiffMedianBonusPercent = returnForYear.DiffMedianBonusPercent,

                CompanyLinkToGPGInfo = returnForYear.CompanyLinkToGPGInfo,
                ResponsiblePerson = returnForYear.ResponsiblePerson,
                OrganisationSize = returnForYear.OrganisationSize.GetAttribute<DisplayAttribute>().Name,
            };
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

            string fileDownloadName = $"Gpg-LateSubmissions-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }
        
        [HttpGet("downloads-new/all-users")]
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

            string fileDownloadName = $"Gpg-AllUsers-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }
        
        [HttpGet("downloads-new/user-organisation-registrations")]
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

            string fileDownloadName = $"Gpg-UserOrganisationRegistrations-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }
        
        [HttpGet("downloads-new/unverified-user-organisation-registrations")]
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

            string fileDownloadName = $"Gpg-UnverifiedUserOrganisationRegistrations-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

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
                        u.UserId,
                        u.Firstname,
                        u.Lastname,
                        u.JobTitle,
                        u.EmailAddress,
                    })
                .ToList();

            string fileDownloadName = $"Gpg-UserConsent-SendUpdates-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

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
                        u.UserId,
                        u.Firstname,
                        u.Lastname,
                        u.JobTitle,
                        u.EmailAddress,
                    })
                .ToList();

            string fileDownloadName = $"Gpg-UserConsent-AllowContactForFeedback-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

    }
}
