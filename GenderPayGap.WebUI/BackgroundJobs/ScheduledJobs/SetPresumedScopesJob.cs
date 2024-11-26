using System;
using System.Collections.Generic;
using System.Linq;
using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.BackgroundJobs.ScheduledJobs
{

    public class SetPresumedScopesJob
    {

        private readonly IDataRepository dataRepository;

        public SetPresumedScopesJob(IDataRepository dataRepository)
        {
            this.dataRepository = dataRepository;
        }


        //Set presumed scope of previous years and current years
        public void SetPresumedScopes()
        {
            JobHelpers.RunAndLogJob(SetPresumedScopesAction, nameof(SetPresumedScopes));
        }

        private void SetPresumedScopesAction()
        {
            foreach (Organisation organisation in dataRepository.GetAll<Organisation>())
            {
                // Fix any scope statuses (e.g. set the most recent to Active and all others to Retired)
                // TODO: work out whether this is really necessary
                // 1. I'd like to think we're careful enough about the business logic that we won't mess this up
                // 2. Maybe we shouldn't have 2 ways of specifying the most recent scope - maybe just use the latest one, rather than have statuses?
                FixScopeStatusesForOrganisation(organisation);

                // Find any organisations that have "missing" scopes and fill them in
                // This mainly happens at the start of the new reporting year when this code creates new scopes for all organisations for the new year
                // We create new scopes based on the previous year's scope - e.g. InScope becomes PresumedInScope
                SetPresumedScopesForOrganisation(organisation);
            }

            dataRepository.SaveChanges();
        }

        private static void FixScopeStatusesForOrganisation(Organisation organisation)
        {
            foreach (int reportingYear in ReportingYearsHelper.GetReportingYears(organisation.SectorType))
            {
                List<OrganisationScope> scopes = organisation.GetAllScopesForYear(reportingYear).OrderByDescending(scope => scope.ScopeStatusDate).ToList();

                // Set the first Scope to be Active
                if (scopes.Any())
                {
                    scopes.First().Status = ScopeRowStatuses.Active;
                }

                // Set all other Scopes to be Retired
                foreach (OrganisationScope scope in scopes.Skip(1))
                {
                    scope.Status = ScopeRowStatuses.Retired;
                }
            }
        }

        private static void SetPresumedScopesForOrganisation(Organisation organisation)
        {
            // If the organisation has NO SCOPES AT ALL, then set all the scopes based on the organisation creation date:
            // - for years that FINISH before the creation date, set the organisation to be PRESUMED OUT OF SCOPE
            // - for the YEAR THE ORGANISATION REGISTERED and SUBSEQUENT reporting years, set the organisation to be PRESUMED IN SCOPE
            // Note: this will probably never happen! since these scopes are created when the organisation is created
            
            ScopeStatuses? previousScope = null;
            foreach (int year in ReportingYearsHelper.GetReportingYears(organisation.SectorType).OrderBy(year => year))
            {
                OrganisationScope scopeForYear = organisation.GetScopeForYear(year);
                if (scopeForYear != null)
                {
                    // We found a scope for this year.
                    // Set previousScope and move to the next year
                    previousScope = scopeForYear.ScopeStatus;
                }
                else
                {
                    // We didn't find a scope for this year...
                    if (previousScope.HasValue)
                    {
                        //...but there was a previous scope for last year
                        // So, calculate and set the new scope
                        ScopeStatuses newScope = CalculateNewScope(previousScope.Value);
                        organisation.SetScopeForYear(year, newScope, "Set by SetPresumedScopesJob (based on previous year)");
                        
                        // Set previousScope and move to the next year
                        previousScope = newScope;
                    }
                    else
                    {
                        //...and there also wasn't a previous scope
                        // Set a new scope based on the organisation created date
                        SetScopeBasedOnOrganisationCreatedDate(organisation, year);
                        
                        // Do NOT set previousScope (we'll also set any future missing scopes based on the organisation created date)
                        previousScope = null;
                    }
                }
            }
        }

        private static ScopeStatuses CalculateNewScope(ScopeStatuses previousScope)
        {
            switch (previousScope)
            {
                case ScopeStatuses.InScope:
                case ScopeStatuses.PresumedInScope:
                    return ScopeStatuses.PresumedInScope;

                case ScopeStatuses.OutOfScope:
                case ScopeStatuses.PresumedOutOfScope:
                    return ScopeStatuses.PresumedOutOfScope;
                
                case ScopeStatuses.Unknown:
                default:
                    return ScopeStatuses.Unknown;
            }
        }

        private static void SetScopeBasedOnOrganisationCreatedDate(Organisation organisation, int year)
        {
            DateTime snapshotDateForSpecifiedYear = organisation.SectorType.GetAccountingStartDate(year);
            DateTime snapshotDateForOrganisationCreatedDate = organisation.SectorType.GetSnapshotDateForDateWithinReportingYear(organisation.Created);

            ScopeStatuses newScope =
                snapshotDateForSpecifiedYear < snapshotDateForOrganisationCreatedDate
                    ? ScopeStatuses.PresumedOutOfScope // The specified year is BEFORE the reporting year during which the organisation was created
                    : ScopeStatuses.PresumedInScope; // The specified year IS the reporting year during which the organisation was created, OR IS AFTER IT
            
            organisation.SetScopeForYear(year, newScope, "Set by SetPresumedScopesJob (based on Organisation created date)");
        }

    }

}
