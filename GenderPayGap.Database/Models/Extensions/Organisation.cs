using System.Diagnostics;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Extensions;

namespace GenderPayGap.Database
{

    [Serializable]
    [DebuggerDisplay("{OrganisationName},{Status}")]
    public partial class Organisation
    {

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

        public string GetSicSectorsString(DateTime? maxDate = null, string delimiter = ", ")
        {
            IEnumerable<OrganisationSicCode> organisationSicCodes = GetSicCodes(maxDate);
            IOrderedEnumerable<string> sicSectionDescriptions = organisationSicCodes.Select(s => s.SicCode.SicSection.Description.Trim())
                .Distinct()
                .OrderBy(s => s);
            return string.Join(delimiter, sicSectionDescriptions);
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

        public string GetSicCodeIdsString(DateTime? maxDate = null, string delimiter = ", ")
        {
            IEnumerable<int> sicCodeIds = GetSicCodes(maxDate).OrderBy(s => s.SicCodeId).Select(s => s.SicCodeId);
            return string.Join(delimiter, sicCodeIds);
        }

        public string GetSicSectionIdsString(DateTime? maxDate = null, string delimiter = ", ")
        {
            IEnumerable<OrganisationSicCode> organisationSicCodes = GetSicCodes(maxDate);
            IOrderedEnumerable<string> sicSectionIds = organisationSicCodes.Select(s => s.SicCode.SicSectionId).Distinct().OrderBy(s => s);
            return string.Join(delimiter, sicSectionIds);
        }

        #region Scope

        public IEnumerable<OrganisationScope> GetAllScopesForYear(int reportingYear)
        {
            return OrganisationScopes
                .Where(s => s.SnapshotDate.Year == reportingYear);
        }

        public IEnumerable<OrganisationScope> GetActiveScopesForYear(int reportingYear)
        {
            return GetAllScopesForYear(reportingYear)
                .Where(s => s.Status == ScopeRowStatuses.Active);
        }

        public OrganisationScope GetScopeForYear(int reportingYear)
        {
            return GetAllScopesForYear(reportingYear)
                .Where(s => s.Status == ScopeRowStatuses.Active)
                .OrderByDescending(s => s.ScopeStatusDate)
                .FirstOrDefault();
        }

        public OrganisationScope GetScopeForYearAsOfDate(int reportingYear, DateTime asOfDate)
        {
            return GetAllScopesForYear(reportingYear)
                .Where(s => s.ScopeStatusDate.Date <= asOfDate.Date)  // Use "X.Date <= Y.Date" to make sure we're not comparing times of day
                .OrderByDescending(s => s.ScopeStatusDate)
                .FirstOrDefault();
        }

        public ScopeStatuses GetScopeStatusForYear(int reportingYear)
        {
            OrganisationScope scope = GetScopeForYear(reportingYear);
            return scope == null ? ScopeStatuses.Unknown : scope.ScopeStatus;
        }

        public ScopeStatuses GetScopeStatusForYearAsOfDate(int reportingYear, DateTime asOfDate)
        {
            OrganisationScope scope = GetScopeForYearAsOfDate(reportingYear, asOfDate);
            return scope == null ? ScopeStatuses.Unknown : scope.ScopeStatus;
        }

        public OrganisationScope GetCurrentScope()
        {
            var accountingStartDate = SectorType.GetAccountingStartDate();

            return GetScopeForYear(accountingStartDate.Year);
        }

        public bool GetIsInscope(int year)
        {
            return GetScopeStatusForYear(year).IsInScopeVariant();
        }

        public void SetScopeForYear(int reportingYear, ScopeStatuses newScope, string statusDetails)
        {
            // Retire old OrganisationScopes
            foreach (OrganisationScope oldOrganisationScope in GetAllScopesForYear(reportingYear))
            {
                oldOrganisationScope.Status = ScopeRowStatuses.Retired;
            }

            // Add new OrganisationScope
            var newOrganisationScope = new OrganisationScope {
                OrganisationId = OrganisationId,
                SnapshotDate = SectorType.GetAccountingStartDate(reportingYear),
                ScopeStatus = newScope,
                Status = ScopeRowStatuses.Active,
                StatusDetails = statusDetails,
            };

            OrganisationScopes.Add(newOrganisationScope);
        }

        #endregion

        #region Returns

        public IEnumerable<Return> GetAllReturnsForYear(int reportingYear)
        {
            return Returns
                .Where(r => r.AccountingDate.Year == reportingYear);
        }

        public IEnumerable<Return> GetSubmittedReturnsForYear(int reportingYear)
        {
            return GetAllReturnsForYear(reportingYear)
                .Where(r => r.Status == ReturnStatuses.Submitted);
        }

        public IEnumerable<Return> GetSubmittedReports()
        {
            return Returns
                .Where(r => r.Status == ReturnStatuses.Submitted)
                .OrderByDescending(r => r.AccountingDate);
        }

        public Return GetLatestReturn()
        {
            return Returns
                .Where(r => r.Status == ReturnStatuses.Submitted)
                .OrderByDescending(r => r.AccountingDate)
                .FirstOrDefault();
        }

        public Return GetReturn(int year = 0)
        {
            return GetSubmittedReturnsForYear(year)
                .OrderByDescending(r => r.StatusDate)
                .FirstOrDefault();
        }

        public Return GetReturnForYearAsOfDate(int reportingYear, DateTime asOfDate)
        {
            return GetAllReturnsForYear(reportingYear)
                .Where(r => r.Status == ReturnStatuses.Submitted || r.Status == ReturnStatuses.Retired)
                .Where(r => r.Modified.Date <= asOfDate.Date)  // Use "X.Date <= Y.Date" to make sure we're not comparing times of day
                .OrderByDescending(r => r.Modified)
                .FirstOrDefault();
        }

        public bool HasSubmittedReturn(int reportingYear)
        {
            return GetSubmittedReturnsForYear(reportingYear).Any();
        }

        public bool HadSubmittedReturnAsOfDate(int reportingYear, DateTime asOfDate)
        {
            return GetReturnForYearAsOfDate(reportingYear, asOfDate) != null;
        }

        #endregion
        
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

        public bool IsSearchable()
        {
            return Status == Core.OrganisationStatuses.Active || Status == Core.OrganisationStatuses.Retired;
        }

        public override string ToString()
        {
            return $"name:{OrganisationName}";
        }

        public OrganisationStatuses GetStatusAsOfDate(DateTime asOfDate)
        {
            OrganisationStatus mostRecentStatusBeforeDate = OrganisationStatuses
                .Where(os => os.StatusDate <= asOfDate)
                .OrderByDescending(os => os.StatusDate)
                .FirstOrDefault(); // We use FirstOrDefault because this list might be empty (for dates before the organisation was created)

            if (mostRecentStatusBeforeDate != null)
            {
                return mostRecentStatusBeforeDate.Status;
            }

            // If we don't have a status for this date (e.g. a date before the organisation was created) then use Unknown
            return Core.OrganisationStatuses.Unknown;
        }

    }
}
