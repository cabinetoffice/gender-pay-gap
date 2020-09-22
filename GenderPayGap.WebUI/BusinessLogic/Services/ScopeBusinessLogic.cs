﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Models.Scope;
using Microsoft.EntityFrameworkCore;

namespace GenderPayGap.WebUI.BusinessLogic.Services
{

    public interface IScopeBusinessLogic
    {

        // scope repo
        Task<OrganisationScope> GetLatestScopeBySnapshotYearAsync(long organisationId, int snapshotYear = 0);
        Task SaveScopeAsync(Organisation org, bool saveToDatabase = true, params OrganisationScope[] newScopes);

        // business logic
        Task<ScopeStatuses> GetLatestScopeStatusForSnapshotYearAsync(long organisationId, int snapshotYear = 0);

        Task<HashSet<Organisation>> SetPresumedScopesAsync();

        Task<HashSet<OrganisationMissingScope>> FindOrgsWhereScopeNotSetAsync();
        bool FillMissingScopes(Organisation org);

        Task<HashSet<Organisation>> SetScopeStatusesAsync();

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
        public virtual async Task<ScopeStatuses> GetLatestScopeStatusForSnapshotYearAsync(long organisationId, int snapshotYear)
        {
            OrganisationScope latestScope = await GetLatestScopeBySnapshotYearAsync(organisationId, snapshotYear);
            if (latestScope == null)
            {
                return ScopeStatuses.Unknown;
            }

            return latestScope.ScopeStatus;
        }

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

        public virtual async Task SaveScopeAsync(Organisation org, bool saveToDatabase = true, params OrganisationScope[] newScopes)
        {
            await SaveScopesAsync(org, newScopes, saveToDatabase);
        }

        public virtual async Task SaveScopesAsync(Organisation org, IEnumerable<OrganisationScope> newScopes, bool saveToDatabase = true)
        {
            foreach (OrganisationScope newScope in newScopes.OrderBy(s => s.SnapshotDate).ThenBy(s => s.ScopeStatusDate))
            {
                // find any prev submitted scopes in the same snapshot year year and retire them
                org.OrganisationScopes
                    .Where(x => x.Status == ScopeRowStatuses.Active && x.SnapshotDate.Year == newScope.SnapshotDate.Year)
                    .ToList()
                    .ForEach(x => x.Status = ScopeRowStatuses.Retired);

                // add the new scope
                newScope.Status = ScopeRowStatuses.Active;
                org.OrganisationScopes.Add(newScope);
            }

            // save to db
            if (saveToDatabase)
            {
                await DataRepository.SaveChangesAsync();
            }
        }

        public async Task<HashSet<Organisation>> SetScopeStatusesAsync()
        {
            DateTime lastSnapshotDate = DateTime.MinValue;
            long lastOrganisationId = -1;
            int index = -1;
            var count = 0;
            IOrderedQueryable<OrganisationScope> scopes = DataRepository.GetAll<OrganisationScope>()
                .OrderBy(os => os.SnapshotDate)
                .ThenBy(os => os.OrganisationId)
                .ThenByDescending(os => os.ScopeStatusDate);
            var changedOrgs = new HashSet<Organisation>();
            foreach (OrganisationScope scope in scopes)
            {
                count++;
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
                    changedOrgs.Add(scope.Organisation);
                }

                lastSnapshotDate = scope.SnapshotDate;
                lastOrganisationId = scope.OrganisationId;
            }

            await DataRepository.SaveChangesAsync();

            return changedOrgs;
        }

        public async Task<HashSet<Organisation>> SetPresumedScopesAsync()
        {
            HashSet<OrganisationMissingScope> missingOrgs = await FindOrgsWhereScopeNotSetAsync();
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
                await DataRepository.SaveChangesAsync();
            }

            return changedOrgs;
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
                OrganisationScope scope = org.GetLatestScopeForSnapshotYear(snapshotYear);

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

        public async Task<HashSet<OrganisationMissingScope>> FindOrgsWhereScopeNotSetAsync()
        {
            // get all orgs of any status
            List<Organisation> allOrgs = await DataRepository
                .GetAll<Organisation>()
                .ToListAsync();

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
                    OrganisationScope scope = org.GetLatestScopeForSnapshotYear(year);
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
                    $"A scope already exists for snapshot year {snapshotDate.Year} for organisation employer reference '{org.EmployerReference}'",
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
                        $"Cannot set {scopeStatus} for snapshot year {snapshotDate.Year} when previos year was {previousScope} for organisation employer reference '{org.EmployerReference}'",
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
        public virtual async Task<OrganisationScope> GetLatestScopeBySnapshotYearAsync(long organisationId, int snapshotYear = 0)
        {
            Organisation org = await DataRepository.GetAll<Organisation>().FirstOrDefaultAsync(o => o.OrganisationId == organisationId);
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
