using GenderPayGap.Core;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class OrganisationScopeBuilder
    {

        private Organisation organisation = new OrganisationBuilder().Build();
        private bool readGuidance = false;
        private ScopeStatuses scopeStatus = ScopeStatuses.PresumedInScope;
        private string reason = "A reason";
        private ScopeRowStatuses status = ScopeRowStatuses.Active;
        private int reportingYear = 0;

        public OrganisationScopeBuilder WithOrganisation(Organisation organisation)
        {
            this.organisation = organisation;
            return this;
        }

        public OrganisationScopeBuilder WithReadGuidance(bool readGuidance)
        {
            this.readGuidance = readGuidance;
            return this;
        }

        public OrganisationScopeBuilder WithScopeStatus(ScopeStatuses scopeStatus)
        {
            this.scopeStatus = scopeStatus;
            return this;
        }

        public OrganisationScopeBuilder WithReason(string reason)
        {
            this.reason = reason;
            return this;
        }

        public OrganisationScopeBuilder WithStatus(ScopeRowStatuses status)
        {
            this.status = status;
            return this;
        }

        public OrganisationScopeBuilder WithReportingYear(int reportingYear)
        {
            this.reportingYear = reportingYear;
            return this;
        }

        public OrganisationScope Build()
        {
            return new OrganisationScope
            {
                Organisation = organisation,
                OrganisationId = organisation.OrganisationId,
                ReadGuidance = readGuidance,
                ScopeStatus = scopeStatus,
                Reason = reason,
                Status = status,
                SnapshotDate = organisation.SectorType.GetAccountingStartDate(reportingYear)
            };
        }

    }
}
