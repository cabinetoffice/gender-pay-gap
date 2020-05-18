using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Filters;
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
                .Include(org => org.OrganisationAddresses)
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
                        Address = org.GetLatestAddress()?.GetAddressString(),
                        SicCodes = org.GetSicCodeIdsString(),
                        Created = org.Created,
                    })
                .ToList();

            string fileDownloadName = $"Gpg-AllOrganisations-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/orphan-organisations")]
        public FileContentResult DownloadOrphanOrganisations()
        {
            DateTime pinExpiresDate = Global.PinExpiresDate;

            List<Organisation> orphanOrganisations = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Where(org =>
                    org.OrganisationScopes.OrderByDescending(s => s.SnapshotDate).FirstOrDefault(s => s.Status == ScopeRowStatuses.Active) == null ||
                    org.OrganisationScopes.OrderByDescending(s => s.SnapshotDate).FirstOrDefault(s => s.Status == ScopeRowStatuses.Active).ScopeStatus == ScopeStatuses.InScope ||
                    org.OrganisationScopes.OrderByDescending(s => s.SnapshotDate).FirstOrDefault(s => s.Status == ScopeRowStatuses.Active).ScopeStatus == ScopeStatuses.PresumedInScope)
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
                        Address = org.GetLatestAddress()?.GetAddressString(),
                        Sector = org.SectorType,
                        ReportingDeadline = org.SectorType.GetAccountingStartDate().AddYears(1).AddDays(-1),
                    })
                .ToList();

            string fileDownloadName = $"Gpg-OrphanOrganisations-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/organisation-addresses")]
        public FileContentResult DownloadOrganisationAddresses()
        {
            List<Organisation> organisationAddresses = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Include(org => org.OrganisationAddresses)
                // This .ToList() materialises the collection (i.e. runs the SQL query)
                .ToList()
                // We only want organisations with valid addresses
                // The following filter only works in code (cannot be converted to SQL) so must be done after the first .ToList()
                .Where(org => org.GetLatestAddress() != null)
                .ToList();

            var records = organisationAddresses.Select(
                    org =>
                    {
                        OrganisationAddress address = org.GetLatestAddress();
                        return new
                        {
                            org.OrganisationId,
                            org.OrganisationName,
                            address.PoBox,
                            address.Address1,
                            address.Address2,
                            address.Address3,
                            address.TownCity,
                            address.County,
                            address.Country,
                            address.PostCode,
                        };
                    })
                .ToList();

            string fileDownloadName = $"Gpg-OrganisationAddresses-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

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

            string fileDownloadName = $"Gpg-OrganisationScopesForYear-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

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

        [HttpGet("downloads/full-submission-history-for-{year}")]
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
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

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

        [HttpGet("downloads/late-submissions-for-{year}")]
        public FileContentResult DownloadLateSubmissions(int year)
        {
            List<Organisation> organisationsWithLateReturns = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Where(
                    org => !org.OrganisationScopes.Any(s => s.SnapshotDate.Year == year) ||
                           org.OrganisationScopes.Any(
                               s => s.SnapshotDate.Year == year &&
                                    (s.ScopeStatus == ScopeStatuses.InScope ||
                                     s.ScopeStatus == ScopeStatuses.PresumedInScope)))

                // There might not be a Return for any given year
                // So we search for organisations that do NOT have a non-late submission
                .Where(
                    org => !org.Returns
                        .Any(
                            r => r.AccountingDate.Year == year
                                 && r.Status == ReturnStatuses.Submitted
                                 && !r.IsLateSubmission))
                .Include(org => org.Returns)
                .ToList();

            var records = organisationsWithLateReturns.Select(
                    org => new
                    {
                        org.OrganisationId,
                        org.OrganisationName,
                        org.SectorType,
                        Submitted = org.GetReturn(year) != null,
                        ReportingDeadline = org.SectorType.GetAccountingStartDate().AddYears(1).AddDays(-1).ToString("d MMMM yyyy"),
                        SubmittedDate = org.GetReturn(year)?.Created,
                        ModifiedDate = org.GetReturn(year)?.Modified,
                        org.GetReturn(year)?.LateReason

                    })
                .ToList();

            string fileDownloadName = $"Gpg-LateSubmissions-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

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

            string fileDownloadName = $"Gpg-AllUsers-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }
        
        [HttpGet("downloads/user-organisation-registrations")]
        public FileContentResult DownloadUserOrganisationRegistrations()
        {
            List<UserOrganisation> userOrganisations = dataRepository.GetAll<UserOrganisation>()
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
        
        [HttpGet("downloads/unverified-user-organisation-registrations")]
        public FileContentResult DownloadUnverifiedUserOrganisationRegistrations()
        {
            List<UserOrganisation> userOrganisations = dataRepository.GetEntities<UserOrganisation>() 
                .Where(uo => uo.PINConfirmedDate == null) 
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

            string fileDownloadName = $"Gpg-UserConsent-SendUpdates-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

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

            string fileDownloadName = $"Gpg-UserConsent-AllowContactForFeedback-{VirtualDateTime.Now:yyyy-MM-dd HH:mm}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/erhc-all-organisations")]
        public FileContentResult EhrcAllOrganisationsForYear_AdminPage(int year)
        {
            return GenerateEhrcAllOrganisationsForYearFile(year);
        }

        [Route("/download"
             /* This is "/download" rather than "download" because the URLs we've shared with the EHRC are of the format:
              https://gender-pay-gap.service.gov.uk/download?p=App_Data%5CDownloads%5CGPG-Organisations_2017-18.csv
              just "download" would make the URLs "/admin/download" */
        )]
        [AllowOnlyTrustedIps(AllowOnlyTrustedIps.IpRangeTypes.EhrcIPRange)]
        public IActionResult EhrcAllOrganisationsForYear_EhrcIpProtectedLink(string p)
        {
            // Get year from query string
            Match match = Regex.Match(p, @"^App_Data\\Downloads\\GPG-Organisations_(?<year4digits>\d\d\d\d)-(?<year2digits>\d\d)\.csv$");
            if (match.Success)
            {
                string year4digitsString = match.Groups["year4digits"].Value;
                string year2digitsString = match.Groups["year2digits"].Value;

                if (int.TryParse(year4digitsString, out int year4digits) &&
                    int.TryParse(year2digitsString, out int year2digits) &&
                    (year4digits + 1) % 100 == year2digits)
                {
                    return GenerateEhrcAllOrganisationsForYearFile(year4digits);
                }
            }

            return NotFound();
        }

        private FileContentResult GenerateEhrcAllOrganisationsForYearFile(int year)
        {
            List<Organisation> organisations = dataRepository
                .GetAll<Organisation>()
                .Include(org => org.OrganisationAddresses)
                .Include(org => org.OrganisationSicCodes)
                .Include(org => org.OrganisationScopes)
                .Include(org => org.Returns)
                .Include(org => org.UserOrganisations)
                .ToList();

            var records = organisations.Select(
                    o => new
                    {
                        OrganisationId = o.OrganisationId,
                        EmployerReference = o.EmployerReference,
                        OrganisationName = o.OrganisationName,
                        CompanyNo = o.CompanyNumber,
                        Sector = o.SectorType,
                        Status = o.Status,
                        StatusDate = o.StatusDate,
                        StatusDetails = o.StatusDetails,
                        Address = o.GetLatestAddress()?.GetAddressString(),
                        SicCodes = o.GetSicCodeIdsString(),
                        LatestRegistrationDate = o.UserOrganisations.OrderByDescending(uo => uo.Created).FirstOrDefault()?.Created,
                        LatestRegistrationMethod = o.UserOrganisations.OrderByDescending(uo => uo.Created).FirstOrDefault()?.Method.ToString(),
                        LatestReturn = o.GetLatestReturn()?.Modified,
                        ScopeStatus = o.GetLatestScopeForSnapshotYear(year)?.ScopeStatus,
                        ScopeDate = o.GetLatestScopeForSnapshotYear(year)?.ScopeStatusDate,
                        Created = o.Created,
                    })
                .ToList();

            string fileDownloadName = $"GPG-Organisations_{year}-{(year + 1) % 100}.csv";
            FileContentResult fileContentResult = CsvDownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

    }
}
