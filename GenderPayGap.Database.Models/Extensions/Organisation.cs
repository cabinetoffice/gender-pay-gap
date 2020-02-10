using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Classes.ErrorMessages;
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

        public OrganisationStatus PreviousStatus
        {
            get
            {
                return OrganisationStatuses
                    .OrderByDescending(os => os.StatusDate)
                    .Skip(1)
                    .FirstOrDefault();
            }
        }
        
        public void SetStatus(OrganisationStatuses status, long byUserId, string details = null)
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
                address = LatestAddress;
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
                PostCode = address.PostCode,
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
            DateTime pinExpiresDate = Global.PinExpiresDate;
            return Status == Core.OrganisationStatuses.Active
                   && (LatestScope.ScopeStatus == ScopeStatuses.InScope || LatestScope.ScopeStatus == ScopeStatuses.PresumedInScope)
                   && (UserOrganisations == null
                       || !UserOrganisations.Any(
                           uo => uo.PINConfirmedDate != null
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
                Size = LatestReturn == null ? 0 : (int) LatestReturn.OrganisationSize,
                SicSectionIds = sicCodes.Select(sic => sic.SicCode.SicSectionId.ToString()).Distinct().ToArray(),
                SicSectionNames = sicCodes.Select(sic => sic.SicCode.SicSection.Description).Distinct().ToArray(),
                SicCodeIds = sicCodes.Select(sicCode => sicCode.SicCodeId.ToString()).Distinct().ToArray(),
                Address = LatestAddress?.GetAddressString(),
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
            UserOrganisation reg = UserOrganisations.OrderBy(uo => uo.PINConfirmedDate).FirstOrDefault(uo => uo.PINConfirmedDate != null);
            if (reg != null)
            {
                return $"Registered {reg.PINConfirmedDate?.ToFriendly(false)}";
            }

            reg = UserOrganisations.OrderBy(uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo => uo.PINSentDate != null && uo.PINConfirmedDate == null);
            if (reg != null)
            {
                return "Awaiting PIN";
            }

            reg = UserOrganisations.OrderBy(uo => uo.PINConfirmedDate)
                .FirstOrDefault(uo => uo.PINSentDate == null && uo.PINConfirmedDate == null && uo.Method == RegistrationMethods.Manual);
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
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
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

        //Returns the latest return for the specified accounting year or the latest ever if no accounting year is 
        public Return GetReturn(int year = 0)
        {
            DateTime accountingStartDate = SectorType.GetAccountingStartDate(year);
            return Returns
                .Where(r => r.Status == ReturnStatuses.Submitted && r.AccountingDate == accountingStartDate)
                .OrderByDescending(r => r.StatusDate)
                .FirstOrDefault();
        }

        //Returns the latest return for the specified accounting date or the latest ever if no accounting date specified
        public Return GetReturn(DateTime? accountingStartDate = null)
        {
            if (accountingStartDate == null || accountingStartDate.Value == DateTime.MinValue)
            {
                accountingStartDate = SectorType.GetAccountingStartDate();
            }

            return Returns.FirstOrDefault(r => r.Status == ReturnStatuses.Submitted && r.AccountingDate == accountingStartDate);
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

        /// <summary>
        ///     Returns the latest address before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore address changes after this date/time - if empty returns the latest address</param>
        /// <returns>The address of the organisation</returns>
        public OrganisationAddress GetAddress(DateTime? maxDate = null, AddressStatuses status = AddressStatuses.Active)
        {
            if (maxDate == null || maxDate.Value == DateTime.MinValue)
            {
                maxDate = SectorType.GetAccountingStartDate().AddYears(1);
            }

            if (status == AddressStatuses.Active && LatestAddress != null && maxDate == SectorType.GetAccountingStartDate().AddYears(1))
            {
                return LatestAddress;
            }

            AddressStatus addressStatus = OrganisationAddresses
                .SelectMany(a => a.AddressStatuses.Where(s => s.Status == status && s.StatusDate < maxDate.Value))
                .OrderByDescending(s => s.StatusDate)
                .FirstOrDefault();

            if (addressStatus != null && addressStatus.Address.Status == status)
            {
                return addressStatus.Address;
            }

            if (LatestAddress != null && LatestAddress.Status == status)
            {
                return LatestAddress;
            }

            return null;
        }

        public OrganisationAddress FindAddress(AddressModel address, AddressStatuses status = AddressStatuses.Active)
        {
            return OrganisationAddresses.FirstOrDefault(a => a.Status == status && a.Equals(address));
        }

        /// <summary>
        ///     Returns the latest organisation name before specified date/time
        /// </summary>
        /// <param name="maxDate">Ignore name changes after this date/time - if empty returns the latest name</param>
        /// <returns>The name of the organisation</returns>
        public string GetAddressString(DateTime? maxDate = null, AddressStatuses status = AddressStatuses.Active, string delimiter = ", ")
        {
            OrganisationAddress address = GetAddress(maxDate, status);

            return address?.GetAddressString(delimiter);
        }

        public AddressModel GetAddressModel(DateTime? maxDate = null, AddressStatuses status = AddressStatuses.Active)
        {
            OrganisationAddress address = GetAddress(maxDate, status);

            return address?.GetAddressModel();
        }

        public bool GetIsDissolved()
        {
            return DateOfCessation != null && DateOfCessation < SectorType.GetAccountingStartDate();
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

        private void RevertToLastStatus(long byUserId, string details = null)
        {
            OrganisationStatus previousStatus = PreviousStatus
                                                ?? throw new InvalidOperationException(
                                                    $"The list of Statuses for Organisation '{OrganisationName}' employerReference '{EmployerReference}' isn't long enough to perform a '{nameof(RevertToLastStatus)}' command. It needs to have at least 2 statuses so these can reverted.");

            SetStatus(previousStatus.Status, byUserId, details);
        }

        public virtual CustomError UnRetire(long byUserId, string details = null)
        {
            if (Status != Core.OrganisationStatuses.Retired)
            {
                return InternalMessages.OrganisationRevertOnlyRetiredErrorMessage(OrganisationName, EmployerReference, Status.ToString());
            }

            RevertToLastStatus(byUserId, details);

            return null;
        }

        public OrganisationScope GetLatestScopeForSnapshotYear(int snapshotYear)
        {
            return OrganisationScopes.FirstOrDefault(
                orgScope =>
                    orgScope.Status == ScopeRowStatuses.Active
                    && orgScope.SnapshotDate.Year == snapshotYear);
        }

        public OrganisationScope GetLatestScopeForSnapshotYearOrThrow(int snapshotYear)
        {
            OrganisationScope organisationScope = GetLatestScopeForSnapshotYear(snapshotYear);

            if (organisationScope == null)
            {
                throw new ArgumentOutOfRangeException(
                    $"Cannot find an scope with status 'Active' for snapshotYear '{snapshotYear}' linked to organisation '{OrganisationName}', employerReference '{EmployerReference}'.");
            }

            return organisationScope;
        }

        public bool IsActive()
        {
            return Status == Core.OrganisationStatuses.Active;
        }

        public bool IsPending()
        {
            return Status == Core.OrganisationStatuses.Pending;
        }

        /// <summary>
        ///     The security code is created exclusively during setting, for all other cases (extend/expire) see method
        ///     'SetSecurityCodeExpiryDate'
        /// </summary>
        /// <param name="securityCodeExpiryDateTime"></param>
        public virtual void SetSecurityCode(DateTime securityCodeExpiryDateTime)
        {
            //Set the security token
            string newSecurityCode = null;
            do
            {
                newSecurityCode = Crypto.GeneratePasscode(Global.SecurityCodeChars.ToCharArray(), Global.SecurityCodeLength);
            } while (newSecurityCode == SecurityCode);

            SecurityCode = newSecurityCode;
            SetSecurityCodeExpiryDate(securityCodeExpiryDateTime);
        }

        /// <summary>
        ///     Method to modify the security code expiring information (create/extend/expire). It additionally timestamps such
        ///     change.
        /// </summary>
        /// <param name="securityCodeExpiryDateTime"></param>
        public void SetSecurityCodeExpiryDate(DateTime securityCodeExpiryDateTime)
        {
            if (SecurityCode == null)
            {
                throw new Exception(
                    "Nothing to modify: Unable to modify the security code's expiry date of the organisation because it's security code is null");
            }

            SecurityCodeExpiryDateTime = securityCodeExpiryDateTime;
            SecurityCodeCreatedDateTime = VirtualDateTime.Now;
        }

        public bool HasSecurityCodeExpired()
        {
            return SecurityCodeExpiryDateTime < VirtualDateTime.Now;
        }

        public UserOrganisation GetLatestRegistration()
        {
            return UserOrganisations
                .Where(uo => uo.PINConfirmedDate != null)
                .OrderByDescending(uo => uo.PINConfirmedDate)
                .FirstOrDefault();
        }

        public override string ToString()
        {
            return $"ref:{EmployerReference}, name:{OrganisationName}";
        }

    }
}
