using System.Dynamic;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
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
    [Authorize(Roles = LoginRoles.GpgAdmin + "," + LoginRoles.GpgAdminReadOnly)]
    [Route("admin")]
    public class AdminDownloadsController : Controller
    {

        private const string DateFormatForFileName = "yyyy-MM-dd HH:mm";

        private readonly IDataRepository dataRepository;

        public AdminDownloadsController(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }

        [HttpGet("downloads")]
        public IActionResult Downloads()
        {
            var viewModel = new AdminDownloadsViewModel
            {
                ReportingYears = ReportingYearsHelper.GetReportingYears()
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

            string fileDownloadName = $"Gpg-AllOrganisations-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/orphan-organisations")]
        public FileContentResult DownloadOrphanOrganisations()
        {
            DateTime pinExpiresDate = VirtualDateTime.Now.AddDays(0 - Global.PinInPostExpiryDays);

            List<Organisation> orphanOrganisations = dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Where(org =>
                    org.OrganisationScopes.OrderByDescending(s => s.SnapshotDate).FirstOrDefault(s => s.Status == ScopeRowStatuses.Active) == null ||
                    org.OrganisationScopes.OrderByDescending(s => s.SnapshotDate).FirstOrDefault(s => s.Status == ScopeRowStatuses.Active).ScopeStatus == ScopeStatuses.InScope ||
                    org.OrganisationScopes.OrderByDescending(s => s.SnapshotDate).FirstOrDefault(s => s.Status == ScopeRowStatuses.Active).ScopeStatus == ScopeStatuses.PresumedInScope)
                // We need the AsEnumerable here because EF gets upset about method calls - so we get the list at this point and then can filter it using a method call
                .AsEnumerable()
                .Where(org => org.UserOrganisations == null ||
                    !org.UserOrganisations.Any(uo => uo.HasBeenActivated() // Registration complete
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
                        ReportingDeadline = ReportingYearsHelper.GetDeadlineForAccountingDate(org.SectorType.GetAccountingStartDate()),
                    })
                .ToList();

            string fileDownloadName = $"Gpg-OrphanOrganisations-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

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
                        string postCode = address.GetPostCodeInAllCaps();
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
                            postCode,
                        };
                    })
                .ToList();

            string fileDownloadName = $"Gpg-OrganisationAddresses-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

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

            string fileDownloadName = $"Gpg-OrganisationScopesForYear-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

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

            string fileDownloadName = $"Gpg-AllSubmissionsForYear-{year}--{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/full-submission-history-for-{year}")]
        public FileContentResult DownloadFullSubmissionHistoryForYear(int year)
        {
            return GenerateFullSubmissionHistoryForYear(dataRepository, year);
        }

        public static FileContentResult GenerateFullSubmissionHistoryForYear(IDataRepository dataRepository, int year)
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

            string fileDownloadName = $"Gpg-FullSubmissionHistoryForYear-{year}--{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

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
                DeadlineDate = ReportingYearsHelper.GetDeadlineForAccountingDate(returnForYear.AccountingDate),
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
                OrganisationSize = returnForYear.OrganisationSize.GetDisplayName(),
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
                        ReportingDeadline = ReportingYearsHelper.GetDeadlineForAccountingDate(org.SectorType.GetAccountingStartDate(year)).ToString("d MMMM yyyy"),
                        ModifiedDate = org.GetReturn(year)?.Modified,
                        org.GetReturn(year)?.LateReason

                    })
                .ToList();

            string fileDownloadName = $"Gpg-LateSubmissions-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

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

            string fileDownloadName = $"Gpg-AllUsers-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

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

            string fileDownloadName = $"Gpg-UserOrganisationRegistrations-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

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

            string fileDownloadName = $"Gpg-UnverifiedUserOrganisationRegistrations-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/user-consent-send-updates")]
        public FileContentResult DownloadUserConsentSendUpdates()
        {
            List<User> users = dataRepository.GetAll<User>()
                .Where(user => user.Status == UserStatuses.Active)
                .Where(user => user.SendUpdates)
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

            string fileDownloadName = $"Gpg-UserConsent-SendUpdates-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/user-consent-allow-contact-for-feedback")]
        public FileContentResult DownloadUserConsentAllowContactForFeedback()
        {
            List<User> users = dataRepository.GetAll<User>()
                .Where(user => user.Status == UserStatuses.Active)
                .Where(user => user.AllowContact)
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

            string fileDownloadName = $"Gpg-UserConsent-AllowContactForFeedback-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        [HttpGet("downloads/erhc-all-organisations")]
        public FileContentResult EhrcAllOrganisationsForYear_AdminPage(int year)
        {
            return GenerateEhrcAllOrganisationsForYearFile(dataRepository, year);
        }

        [HttpGet("downloads/organisations-without-submitted-returns")]
        public FileContentResult DownloadOrganisationsWithNoSubmittedReturns(int year)
        {
            return GenerateOrganisationsWithNoSubmittedReturnsForYear(dataRepository, year, false);
        }

        [HttpGet("downloads/organisations-without-submitted-returns-with-login-data")]
        public FileContentResult DownloadOrganisationsWithNoSubmittedReturnsAndRecentLoginData(int year)
        {
            return GenerateOrganisationsWithNoSubmittedReturnsForYear(dataRepository, year, true);
        }

        public static FileContentResult GenerateEhrcAllOrganisationsForYearFile(IDataRepository dataRepository, int year)
        {
            // IMPORTANT: This variable isn't used, but running this query makes the next query much faster
            var allOrgsWithAddresses = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationAddresses)
                .ToList();

            // IMPORTANT: This variable isn't used, but running this query makes the next query much faster
            var allOrgsWithScopes = dataRepository.GetAll<Organisation>()
                .Include(o => o.OrganisationScopes)
                .ToList();

            // IMPORTANT: This variable isn't used, but running this query makes the next query much faster
            var allOrgsWithReturns = dataRepository.GetAll<Organisation>()
                .Include(o => o.Returns)
                .ToList();

            // IMPORTANT: This variable isn't used, but running this query makes the next query much faster
            var allOrgsWithUserOrgs = dataRepository.GetAll<Organisation>()
                .Include(o => o.UserOrganisations)
                .ToList();

            List<Organisation> organisations = dataRepository
                .GetAll<Organisation>()
                //.Include(org => org.OrganisationAddresses) // Moved into separate pre-load query
                .Include(org => org.OrganisationSicCodes)
                //.Include(org => org.OrganisationScopes) // Moved into separate pre-load query
                //.Include(org => org.Returns) // Moved into separate pre-load query
                //.Include(org => org.UserOrganisations) // Moved into separate pre-load query
                .ToList();

            var records = organisations.Select(
                    o => new
                    {
                        OrganisationId = o.OrganisationId,
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
                        ScopeStatus = o.GetScopeForYear(year)?.ScopeStatus,
                        ScopeDate = o.GetScopeForYear(year)?.ScopeStatusDate,
                        Created = o.Created,
                    })
                .ToList();

            string fileDownloadName = $"GPG-Organisations_{ReportingYearsHelper.FormatYearAsReportingPeriod(year)}.csv";
            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        public static FileContentResult GenerateOrganisationsWithNoSubmittedReturnsForYear(IDataRepository dataRepository, int year, bool includeMostRecentLoginData)
        {
            IEnumerable<Organisation> organisationsWithNoSubmittedReturns = GetOrganisationsWithNoSubmittedReturns(dataRepository, year);

            IEnumerable<dynamic> records = BuildOrganisationsWithNoSubmittedReturnsRecords(organisationsWithNoSubmittedReturns, includeMostRecentLoginData);

            string filenameWithoutRecentLoginDetails = $"Gpg-NoSubmissionsFor{year}-{VirtualDateTime.Now.ToString(DateFormatForFileName)}.csv";
            string filenameWithRecentLoginDetails = $"Gpg-NoSubmissionsFor{year}-{VirtualDateTime.Now.ToString(DateFormatForFileName)}-(recentLogins).csv";
            string fileDownloadName = includeMostRecentLoginData ? filenameWithRecentLoginDetails : filenameWithoutRecentLoginDetails;

            FileContentResult fileContentResult = DownloadHelper.CreateCsvDownload(records, fileDownloadName);

            return fileContentResult;
        }

        private static IEnumerable<Organisation> GetOrganisationsWithNoSubmittedReturns(IDataRepository dataRepository, int year)
        {
            return dataRepository.GetAll<Organisation>()
                .Where(org => org.Status == OrganisationStatuses.Active)
                .Where(
                    org => !org.OrganisationScopes.Any(s => s.SnapshotDate.Year == year)
                           || org.OrganisationScopes.Any(
                               s => s.SnapshotDate.Year == year
                                    && (s.ScopeStatus == ScopeStatuses.InScope || s.ScopeStatus == ScopeStatuses.PresumedInScope)
                                    && s.Status != ScopeRowStatuses.Retired))
                .Where(org => !org.Returns.Any(r => r.AccountingDate.Year == year && r.Status == ReturnStatuses.Submitted))
                .Include(o => o.OrganisationAddresses)
                .Include(o => o.UserOrganisations)
                .ThenInclude(uo => uo.User)
                .Include(o => o.Returns);
        }

        private static IEnumerable<dynamic> BuildOrganisationsWithNoSubmittedReturnsRecords(IEnumerable<Organisation> organisationsWithNoSubmittedReturns, bool includeMostRecentLoginData)
        {
            return organisationsWithNoSubmittedReturns.Select(
                    org =>
                    {
                        dynamic record = new ExpandoObject();
                        record.OrganisationId = org.OrganisationId;
                        record.OrganisationName = org.OrganisationName;
                        record.CompanyNumber = org.CompanyNumber;
                        record.SectorType = org.SectorType;
                        record.Address = org.GetLatestAddress()?.GetAddressString();

                        UserOrganisation latestUserOrg = org.UserOrganisations
                            .OrderByDescending(uo => uo.Created)
                            .FirstOrDefault(
                                uo => uo.HasBeenActivated()
                                      && uo.User.Status == UserStatuses.Active);

                        Return latestReturn = org.Returns
                            .OrderByDescending(r => r.StatusDate)
                            .FirstOrDefault(r => r.Status == ReturnStatuses.Submitted);

                        record.Size = latestReturn?.OrganisationSize.GetDisplayName();
                        record.FirstName = latestUserOrg?.User.Firstname;
                        record.LastName = latestUserOrg?.User.Lastname;
                        record.JobTitle = latestUserOrg?.User.JobTitle;
                        record.PhoneNumber = latestUserOrg?.User.ContactPhoneNumber;
                        record.EmailAddress = latestUserOrg?.User.EmailAddress;

                        foreach (int repYear in ReportingYearsHelper.GetReportingYears(org.SectorType))
                        {
                            ((IDictionary<string, object>) record)["ReportDateTimeFor" + repYear] = org.GetReturn(repYear)?.StatusDate;
                        }

                        if (includeMostRecentLoginData)
                        {
                            UserOrganisation lastUserLogin = org.UserOrganisations
                                .OrderByDescending(uo => uo.User.LoginDate)
                                .FirstOrDefault(
                                    uo => uo.HasBeenActivated()
                                        && uo.User.Status == UserStatuses.Active);

                            record.LastLoginFirstName = lastUserLogin?.User.Firstname;
                            record.LastLoginLastname = lastUserLogin?.User.Lastname;
                            record.LastLoginEmail = lastUserLogin?.User.EmailAddress;
                            record.LastLoginTime = lastUserLogin?.User.LoginDate;
                        }
                        
                        return record;
                    })
                .ToList();
        }
    }
}
