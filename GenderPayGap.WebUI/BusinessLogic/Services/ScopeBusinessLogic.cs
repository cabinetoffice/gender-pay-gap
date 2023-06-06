using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.WebUI.BusinessLogic.Models.Scope;

namespace GenderPayGap.WebUI.BusinessLogic.Services
{

    public interface IScopeBusinessLogic
    {

        // scope repo
        OrganisationScope GetLatestScopeBySnapshotYear(long organisationId, int snapshotYear = 0);

        // business logic
        void SetPresumedScopes();

        HashSet<OrganisationMissingScope> FindOrgsWhereScopeNotSet();
        bool FillMissingScopes(Organisation org);

        void SetScopeStatuses();

    }

    public class ScopeBusinessLogic : IScopeBusinessLogic
    {

        public ScopeBusinessLogic(IDataRepository dataRepo)
        {
            DataRepository = dataRepo;
        }

        private IDataRepository DataRepository { get; }


        /// <summary>
        ///     Returns the latest scope status for an organisation and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        public virtual ScopeStatuses GetLatestScopeStatusForSnapshotYear(Organisation org, int snapshotYear = 0)
        {
            OrganisationScope latestScope = GetLatestScopeBySnapshotYear(org, snapshotYear);
            if (latestScope == null)
            {
                return ScopeStatuses.Unknown;
            }

            return latestScope.ScopeStatus;
        }

        public void SetScopeStatuses()
        {
            DateTime lastSnapshotDate = DateTime.MinValue;
            long lastOrganisationId = -1;
            int index = -1;
            IOrderedQueryable<OrganisationScope> scopes = DataRepository.GetAll<OrganisationScope>()
                .OrderBy(os => os.SnapshotDate)
                .ThenBy(os => os.OrganisationId)
                .ThenByDescending(os => os.ScopeStatusDate);

            foreach (OrganisationScope scope in scopes)
            {
                if (lastSnapshotDate != scope.SnapshotDate || lastOrganisationId != scope.OrganisationId)
                {
                    index = 0;
                }
                else
                {
                    index++;
                }

                //Set the status
                ScopeRowStatuses newStatus = index == 0 ? ScopeRowStatuses.Active : ScopeRowStatuses.Retired;
                if (scope.Status != newStatus)
                {
                    scope.Status = newStatus;
                }

                lastSnapshotDate = scope.SnapshotDate;
                lastOrganisationId = scope.OrganisationId;
            }

            DataRepository.SaveChanges();
        }

        public void SetPresumedScopes()
        {
            HashSet<OrganisationMissingScope> missingOrgs = FindOrgsWhereScopeNotSet();
            var changedOrgs = new HashSet<Organisation>();

            foreach (OrganisationMissingScope org in missingOrgs)
            {
                if (FillMissingScopes(org.Organisation))
                {
                    changedOrgs.Add(org.Organisation);
                }
            }

            if (changedOrgs.Count > 0)
            {
                DataRepository.SaveChanges();
            }
        }

        public bool FillMissingScopes(Organisation org)
        {
            int firstYear = Global.FirstReportingYear;
            DateTime currentSnapshotDate = org.SectorType.GetAccountingStartDate();
            int currentSnapshotYear = currentSnapshotDate.Year;
            var prevYearScope = ScopeStatuses.Unknown;
            var neverDeclaredScope = true;
            var changed = false;

            for (int snapshotYear = firstYear; snapshotYear <= currentSnapshotYear; snapshotYear++)
            {
                OrganisationScope scope = org.GetScopeForYear(snapshotYear);

                // if we already have a scope then flag (prevYearScope, neverDeclaredScope) and skip this year
                if (scope != null && scope.ScopeStatus != ScopeStatuses.Unknown)
                {
                    prevYearScope = scope.ScopeStatus;
                    neverDeclaredScope = false;
                    continue;
                }

                // determine the snapshot date from year
                var snapshotDate = new DateTime(snapshotYear, currentSnapshotDate.Month, currentSnapshotDate.Day);

                // determine if need to presume scope
                bool shouldPresumeScope = neverDeclaredScope
                                          && (prevYearScope == ScopeStatuses.PresumedOutOfScope
                                              || prevYearScope == ScopeStatuses.PresumedInScope
                                              || prevYearScope == ScopeStatuses.Unknown);

                // presumed scope from created date
                if (shouldPresumeScope)
                {
                    bool createdAfterSnapshotYear = org.Created >= snapshotDate.AddYears(1);
                    if (createdAfterSnapshotYear)
                    {
                        prevYearScope = ScopeStatuses.PresumedOutOfScope;
                    }
                    else
                    {
                        prevYearScope = ScopeStatuses.PresumedInScope;
                    }
                }
                // otherwise presume scope from declared scope
                else if (prevYearScope == ScopeStatuses.InScope)
                {
                    prevYearScope = ScopeStatuses.PresumedInScope;
                }
                else if (prevYearScope == ScopeStatuses.OutOfScope)
                {
                    prevYearScope = ScopeStatuses.PresumedOutOfScope;
                }

                // update the scope status
                SetPresumedScope(org, prevYearScope, snapshotDate);

                changed = true;
            }

            return changed;
        }

        public HashSet<OrganisationMissingScope> FindOrgsWhereScopeNotSet()
        {
            // get all orgs of any status
            List<Organisation> allOrgs = DataRepository
                .GetAll<Organisation>()
                .ToList();

            int firstYear = Global.FirstReportingYear;

            // find all orgs who have no scope or unknown scope statuses
            var orgsWithMissingScope = new HashSet<OrganisationMissingScope>();
            foreach (Organisation org in allOrgs)
            {
                DateTime currentSnapshotDate = org.SectorType.GetAccountingStartDate();
                int currentYear = currentSnapshotDate.Year;
                var missingSnapshotYears = new List<int>();

                // for all snapshot years check if scope exists
                for (int year = firstYear; year <= currentYear; year++)
                {
                    OrganisationScope scope = org.GetScopeForYear(year);
                    if (scope == null || scope.ScopeStatus == ScopeStatuses.Unknown)
                    {
                        missingSnapshotYears.Add(year);
                    }
                }

                // collect
                if (missingSnapshotYears.Count > 0)
                {
                    orgsWithMissingScope.Add(
                        new OrganisationMissingScope {Organisation = org, MissingSnapshotYears = missingSnapshotYears});
                }
            }

            return orgsWithMissingScope;
        }

        /// <summary>
        ///     Adds a new scope and updates the latest scope (if required)
        /// </summary>
        /// <param name="org"></param>
        /// <param name="scopeStatus"></param>
        /// <param name="snapshotDate"></param>
        /// <param name="currentUser"></param>
        public virtual OrganisationScope SetPresumedScope(Organisation org,
            ScopeStatuses scopeStatus,
            DateTime snapshotDate,
            User currentUser = null)
        {
            //Ensure scopestatus is presumed
            if (scopeStatus != ScopeStatuses.PresumedInScope && scopeStatus != ScopeStatuses.PresumedOutOfScope)
            {
                throw new ArgumentOutOfRangeException(nameof(scopeStatus));
            }

            //Check no previous scopes
            if (org.OrganisationScopes.Any(os => os.SnapshotDate == snapshotDate))
            {
                throw new ArgumentException(
                    $"A scope already exists for snapshot year {snapshotDate.Year} for organisation ID '{org.OrganisationId}'",
                    nameof(scopeStatus));
            }

            //Check for conflict with previous years scope
            if (snapshotDate.Year > Global.FirstReportingYear)
            {
                ScopeStatuses previousScope = GetLatestScopeStatusForSnapshotYear(org, snapshotDate.Year - 1);
                if (previousScope == ScopeStatuses.InScope && scopeStatus == ScopeStatuses.PresumedOutOfScope
                    || previousScope == ScopeStatuses.OutOfScope && scopeStatus == ScopeStatuses.PresumedInScope)
                {
                    throw new ArgumentException(
                        $"Cannot set {scopeStatus} for snapshot year {snapshotDate.Year} when previos year was {previousScope} for organisation ID '{org.OrganisationId}'",
                        nameof(scopeStatus));
                }
            }

            var newScope = new OrganisationScope {
                OrganisationId = org.OrganisationId,
                ContactEmailAddress = currentUser?.EmailAddress,
                ContactFirstname = currentUser?.Firstname,
                ContactLastname = currentUser?.Lastname,
                ScopeStatus = scopeStatus,
                Status = ScopeRowStatuses.Active,
                StatusDetails = "Generated by the system",
                SnapshotDate = snapshotDate
            };

            org.OrganisationScopes.Add(newScope);

            return newScope;
        }

        #region Repo

        /// <summary>
        ///     Gets the latest scope for the specified organisation id and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        public virtual OrganisationScope GetLatestScopeBySnapshotYear(long organisationId, int snapshotYear = 0)
        {
            Organisation org = DataRepository.GetAll<Organisation>().FirstOrDefault(o => o.OrganisationId == organisationId);
            if (org == null)
            {
                throw new ArgumentException($"Cannot find organisation with id {organisationId}", nameof(organisationId));
            }

            return GetLatestScopeBySnapshotYear(org, snapshotYear);
        }

        /// <summary>
        ///     Gets the latest scope for the specified organisation id and snapshot year
        /// </summary>
        /// <param name="organisationId"></param>
        /// <param name="snapshotYear"></param>
        public virtual OrganisationScope GetLatestScopeBySnapshotYear(Organisation organisation, int snapshotYear = 0)
        {
            if (snapshotYear == 0)
            {
                snapshotYear = organisation.SectorType.GetAccountingStartDate().Year;
            }

            OrganisationScope orgScope = organisation.OrganisationScopes
                .SingleOrDefault(s => s.SnapshotDate.Year == snapshotYear && s.Status == ScopeRowStatuses.Active);

            return orgScope;
        }

        #endregion

    }
}
