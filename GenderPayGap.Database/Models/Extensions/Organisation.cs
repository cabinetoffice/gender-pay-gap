using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{

    [Serializable]
    [DebuggerDisplay("{OrganisationName},{Status}")]
    public partial class Organisation
    {

        private IObfuscator _obfuscator;

        public Organisation(IObfuscator obfuscator)
        {
            _obfuscator = obfuscator;
        }

        private IObfuscator obfuscator
        {
            get
            {
                if (_obfuscator == null)
                {
                    _obfuscator = new InternalObfuscator();
                }

                return _obfuscator;
            }
        }

        public void SetStatus(OrganisationStatuses status, long? byUserId = null, string details = null)
        {
            if (status == Status && details == StatusDetails)
            {
                return;
            }

            OrganisationStatuses.Add(
                new OrganisationStatus {
                    OrganisationId = OrganisationId,
                    Status = status,
                    StatusDate = VirtualDateTime.Now,
                    StatusDetails = details,
                    ByUserId = byUserId
                });
            Status = status;
            StatusDate = VirtualDateTime.Now;
            StatusDetails = details;
        }

        public EmployerRecord ToEmployerRecord(long userId = 0)
        {
            OrganisationAddress address = null;
            if (userId > 0)
            {
                address = UserOrganisations.FirstOrDefault(uo => uo.UserId == userId)?.Address;
            }

            if (address == null)
            {
                address = GetLatestAddress();
            }

            if (address == null)
            {
                return new EmployerRecord {
                    OrganisationId = OrganisationId,
                    SectorType = SectorType,
                    OrganisationName = OrganisationName,
                    NameSource = GetName()?.Source,
                    EmployerReference = EmployerReference,
                    DateOfCessation = DateOfCessation,
                    CompanyNumber = CompanyNumber,
                    SicSectors = GetSicSectorsString(null, ",<br/>"),
                    SicCodeIds = GetSicCodeIdsString(),
                    SicSource = GetSicSource(),
                    RegistrationStatus = GetRegistrationStatus(),
                    References = OrganisationReferences.ToDictionary(
                        r => r.ReferenceName,
                        r => r.ReferenceValue,
                        StringComparer.OrdinalIgnoreCase)
                };
            }

            return new EmployerRecord {
                OrganisationId = OrganisationId,
                SectorType = SectorType,
                OrganisationName = OrganisationName,
                NameSource = GetName()?.Source,
                EmployerReference = EmployerReference,
                DateOfCessation = DateOfCessation,
                CompanyNumber = CompanyNumber,
                SicSectors = GetSicSectorsString(null, ",<br/>"),
                SicCodeIds = GetSicCodeIdsString(),
                SicSource = GetSicSource(),
                ActiveAddressId = address.AddressId,
                AddressSource = address.Source,
                Address1 = address.Address1,
                Address2 = address.Address2,
                Address3 = address.Address3,
                City = address.TownCity,
                County = address.County,
                Country = address.Country,
                PostCode = address.GetPostCodeInAllCaps(),
                PoBox = address.PoBox,
                IsUkAddress = address.IsUkAddress,
                RegistrationStatus = GetRegistrationStatus(),
                References = OrganisationReferences.ToDictionary(
                    r => r.ReferenceName,
                    r => r.ReferenceValue,
                    StringComparer.OrdinalIgnoreCase)
            };
        }

        /// <summary>
        ///     Returns true if organisation has been made an orphan and is in scope
        /// </summary>
        public bool GetIsOrphan()
        {
            DateTime pinExpiresDate = VirtualDateTime.Now.AddDays(0 - Global.PinInPostExpiryDays);
            return Status == Core.OrganisationStatuses.Active
                   && (GetCurrentScope().ScopeStatus == ScopeStatuses.InScope || GetCurrentScope().ScopeStatus == ScopeStatuses.PresumedInScope)
                   && (UserOrganisations == null
                       || !UserOrganisations.Any(
                           uo => uo.HasBeenActivated()
                                 || uo.Method == RegistrationMethods.Manual
                                 || uo.Method == RegistrationMethods.PinInPost
                                 && uo.PINSentDate.HasValue
                                 && uo.PINSentDate.Value > pinExpiresDate));
        }

        public EmployerSearchModel ToEmployerSearchResult(bool keyOnly = false, List<SicCodeSearchModel> listOfSicCodeSearchModels = null)
        {
            if (keyOnly)
            {
                return new EmployerSearchModel {OrganisationId = OrganisationId.ToString()};
            }

            // Get the last two names for the org. Most recent name first
            string[] names = OrganisationNames.Select(n => n.Name).Reverse().Take(2).ToArray();

            var abbreviations = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            names.ForEach(n => abbreviations.Add(n.ToAbbr()));
            names.ForEach(n => abbreviations.Add(n.ToAbbr(".")));
            var excludes = new[] {"Ltd", "Limited", "PLC", "Corporation", "Incorporated", "LLP", "The", "And", "&", "For", "Of", "To"};
            names.ForEach(n => abbreviations.Add(n.ToAbbr(excludeWords: excludes)));
            names.ForEach(n => abbreviations.Add(n.ToAbbr(".", excludeWords: excludes)));

            abbreviations.RemoveWhere(a => string.IsNullOrWhiteSpace(a));
            abbreviations.Remove(OrganisationName);

            // extract the prev org name (if exists)
            var prevOrganisationName = "";
            if (names.Length > 1)
            {
                prevOrganisationName = names[names.Length - 1];
                abbreviations.Remove(prevOrganisationName);
            }

            //Get the latest sic codes
            IEnumerable<OrganisationSicCode> sicCodes = GetSicCodes();

            Return[] submittedReports = GetSubmittedReports().ToArray();

            var result = new EmployerSearchModel {
                OrganisationId = OrganisationId.ToString(),
                OrganisationIdEncrypted = GetEncryptedId(),
                Name = OrganisationName,
                PreviousName = prevOrganisationName,
                PartialNameForSuffixSearches = OrganisationName,
                PartialNameForCompleteTokenSearches = OrganisationName,
                Abbreviations = abbreviations.ToArray(),
                Size = GetLatestReturn() == null ? 0 : (int)GetLatestReturn().OrganisationSize,
                SicSectionIds = sicCodes.Select(sic => sic.SicCode.SicSectionId.ToString()).Distinct().ToArray(),
                SicSectionNames = sicCodes.Select(sic => sic.SicCode.SicSection.Description).Distinct().ToArray(),
                SicCodeIds = sicCodes.Select(sicCode => sicCode.SicCodeId.ToString()).Distinct().ToArray(),
                Address = GetLatestAddress()?.GetAddressString(),
                LatestReportedDate = submittedReports.Select(x => x.Created).FirstOrDefault(),
                ReportedYears = submittedReports.Select(x => x.AccountingDate.Year.ToString()).ToArray(),
                ReportedLateYears =
                    submittedReports.Where(x => x.IsLateSubmission).Select(x => x.AccountingDate.Year.ToString()).ToArray(),
                ReportedExplanationYears = submittedReports.Where(x => string.IsNullOrEmpty(x.CompanyLinkToGPGInfo) == false)
                    .Select(x => x.AccountingDate.Year.ToString())
                    .ToArray()
            };

            if (listOfSicCodeSearchModels != null)
            {
                result.SicCodeListOfSynonyms = GetListOfSynonyms(result.SicCodeIds, listOfSicCodeSearchModels);
            }

            return result;
        }

        private string[] GetListOfSynonyms(string[] resultSicCodeIds, List<SicCodeSearchModel> listOfSicCodeSearchModels)
        {
            var result = new List<string>();

            foreach (string resultSicCodeId in resultSicCodeIds)
            {
                SicCodeSearchModel sicCodeSearchModel = listOfSicCodeSearchModels.FirstOrDefault(x => x.SicCodeId == resultSicCodeId);

                if (sicCodeSearchModel == null)
                {
                    continue;
                }

                result.Add(sicCodeSearchModel.SicCodeDescription);

                if (sicCodeSearchModel.SicCodeListOfSynonyms != null && sicCodeSearchModel.SicCodeListOfSynonyms.Length > 0)
                {
                    result.AddRange(sicCodeSearchModel.SicCodeListOfSynonyms);
                }
            }

            return result.Any()
                ? result.ToArray()
                : null;
        }

        public string GetRegistrationStatus()
        {
            UserOrganisation reg = UserOrganisations.OrderBy(uo => uo.PINConfirmedDate).FirstOrDefault(uo => uo.HasBeenActivated());
            if (reg != null)
            {
                return $"Registered {reg.PINConfirmedDate?.ToFriendly(false)}";
            }

            reg = UserOrganisations.OrderBy(uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo => uo.IsAwaitingActivationPIN());
            if (reg != null)
            {
                return "Awaiting PIN";
            }

            reg = UserOrganisations.OrderBy(uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo => uo.IsAwaitingRegistrationApproval() && uo.Method == RegistrationMethods.Manual);
            if (reg != null)
            {
                return "Awaiting Approval";
            }

            return "No registrations";
        }

        public string GetSicSectorsString(DateTime? maxDate = null, string delimiter = ", ")
        {
            IEnumerable<OrganisationSicCode> organisationSicCodes = GetSicCodes(maxDate);
            return organisationSicCodes.Select(s => s.SicCode.SicSection.Description.Trim())
                .UniqueI()
                .OrderBy(s => s)
                .ToDelimitedString(delimiter);
        }

        /// <summary>
        ///     Returns the latest SIC Codes before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore SIC codes changes after this date/time - if empty returns the latest SIC codes</param>
        /// <returns>The employer SIC codes</returns>
        public IEnumerable<OrganisationSicCode> GetSicCodes(DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue)
            {
                maxDate = SectorType.GetAccountingStartDate().AddYears(1);
            }

            return OrganisationSicCodes.Where(s => s.Created < maxDate.Value && (s.Retired == null || s.Retired.Value > maxDate.Value));
        }

        public SortedSet<int> GetSicCodeIds(DateTime? maxDate = null)
        {
            IEnumerable<OrganisationSicCode> organisationSicCodes = GetSicCodes(maxDate);

            var codes = new SortedSet<int>();
            foreach (OrganisationSicCode sicCode in organisationSicCodes)
            {
                codes.Add(sicCode.SicCodeId);
            }

            return codes;
        }


        public string GetSicSource(DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue)
            {
                maxDate = SectorType.GetAccountingStartDate().AddYears(1);
            }

            return OrganisationSicCodes
                .FirstOrDefault(s => s.Created < maxDate.Value && (s.Retired == null || s.Retired.Value > maxDate.Value))
                ?.Source;
        }

        public string GetSicCodeIdsString(DateTime? maxDate = null, string delimiter = ", ")
        {
            return GetSicCodes(maxDate).OrderBy(s => s.SicCodeId).Select(s => s.SicCodeId).ToDelimitedString(delimiter);
        }

        public string GetSicSectionIdsString(DateTime? maxDate = null, string delimiter = ", ")
        {
            IEnumerable<OrganisationSicCode> organisationSicCodes = GetSicCodes(maxDate);
            return organisationSicCodes.Select(s => s.SicCode.SicSectionId).UniqueI().OrderBy(s => s).ToDelimitedString(delimiter);
        }

        public string GetEncryptedId()
        {
            return obfuscator.Obfuscate(OrganisationId.ToString());
        }

        public Return GetLatestReturn()
        {
            return Returns
                .Where(r => r.Status == ReturnStatuses.Submitted)
                .OrderByDescending(r => r.AccountingDate)
                .FirstOrDefault();
        }

        //Returns the latest return for the specified accounting year or the latest ever if no accounting year is 
        public Return GetReturn(int year = 0)
        {
            DateTime accountingStartDate = SectorType.GetAccountingStartDate(year);
            return Returns
                .Where(r => r.Status == ReturnStatuses.Submitted && r.AccountingDate == accountingStartDate)
                .OrderByDescending(r => r.StatusDate)
                .FirstOrDefault();
        }

        //Returns the latest scope for the current accounting date
        public OrganisationScope GetCurrentScope()
        {
            var accountingStartDate = SectorType.GetAccountingStartDate();

            return GetScopeForYear(accountingStartDate);
        }

        //Returns the scope for the specified accounting date
        public OrganisationScope GetScopeForYear(DateTime accountingStartDate)
        {
            return GetScopeForYear(accountingStartDate.Year);
        }

        public OrganisationScope GetScopeForYear(int year)
        {
            return OrganisationScopes.FirstOrDefault(
                s => s.Status == ScopeRowStatuses.Active && s.SnapshotDate.Year == year);
        }


        public ScopeStatuses GetScopeStatus(int year = 0)
        {
            DateTime accountingStartDate = SectorType.GetAccountingStartDate(year);
            return GetScopeStatus(accountingStartDate);
        }

        public ScopeStatuses GetScopeStatus(DateTime accountingStartDate)
        {
            OrganisationScope scope = GetScopeForYear(accountingStartDate);
            return scope == null ? ScopeStatuses.Unknown : scope.ScopeStatus;
        }

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public OrganisationName GetName(DateTime? maxDate = null)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue)
            {
                maxDate = SectorType.GetAccountingStartDate().AddYears(1);
            }

            return OrganisationNames.Where(n => n.Created < maxDate.Value).OrderByDescending(n => n.Created).FirstOrDefault();
        }

        public OrganisationAddress GetLatestAddress()
        {
            return OrganisationAddresses
                .OrderByDescending(add => add.Created)
                .FirstOrDefault();
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var target = (Organisation) obj;
            return OrganisationId == target.OrganisationId;
        }

        public override int GetHashCode()
        {
            return OrganisationId.GetHashCode();
        }

        public static IQueryable<Organisation> Search(IQueryable<Organisation> searchData,
            string searchText,
            int records,
            int levenshteinDistance = 0)
        {
            var levenshteinRecords =
                searchData.ToList().Select(o => new {distance = o.OrganisationName.LevenshteinCompute(searchText), org = o});
            string pattern = searchText?.ToLower();

            IQueryable<Organisation> searchResults = levenshteinRecords.AsQueryable()
                .Where(
                    data => data.org.OrganisationName.ToLower().Contains(pattern)
                            || data.org.OrganisationName.Length > levenshteinDistance && data.distance <= levenshteinDistance)
                .OrderBy(o => o.distance)
                .Take(records)
                .Select(o => o.org);
            return searchResults;
        }
        
        public IEnumerable<Return> GetRecentReports(int recentCount)
        {
            foreach (int year in GetRecentReportingYears(recentCount))
            { 
                var defaultReturn = new Return {
                    Organisation = this,
                    AccountingDate = SectorType.GetAccountingStartDate(year),
                    Modified = VirtualDateTime.Now
                };
                defaultReturn.IsLateSubmission = defaultReturn.CalculateIsLateSubmission();

                yield return GetReturn(year) ?? defaultReturn;
            }
        }

        public IEnumerable<int> GetRecentReportingYears(int recentCount)
        {
            int endYear = SectorType.GetAccountingStartDate().Year;
            int startYear = endYear - (recentCount - 1);
            if (startYear < Global.FirstReportingYear)
            {
                startYear = Global.FirstReportingYear;
            }

            for (int year = endYear; year >= startYear; year--)
            {
                yield return year;
            }
        }

        public bool GetIsInscope(int year)
        {
            return !GetScopeStatus(year).IsAny(ScopeStatuses.PresumedOutOfScope, ScopeStatuses.OutOfScope);
        }
        
        public IEnumerable<Return> GetSubmittedReports()
        {
            return Returns.Where(
                    r =>
                        r.Status == ReturnStatuses.Submitted)
                .OrderByDescending(r => r.AccountingDate);
        }

        public OrganisationScope GetLatestScopeForSnapshotYear(int snapshotYear)
        {
            return OrganisationScopes.FirstOrDefault(
                orgScope =>
                    orgScope.Status == ScopeRowStatuses.Active
                    && orgScope.SnapshotDate.Year == snapshotYear);
        }

        public bool IsSearchable()
        {
            return Status == Core.OrganisationStatuses.Active || Status == Core.OrganisationStatuses.Retired;
        }

        public override string ToString()
        {
            return $"ref:{EmployerReference}, name:{OrganisationName}";
        }

        public bool HasSubmittedReturn(int reportingYear)
        {
            return GetReturn(reportingYear) != null;
        }

    }
}
