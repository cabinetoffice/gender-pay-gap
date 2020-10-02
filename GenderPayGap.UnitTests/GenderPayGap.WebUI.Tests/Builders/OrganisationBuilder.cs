using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Database;
using GenderPayGap.Extensions;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class OrganisationBuilder
    {

        private static long nextOrganisationId = 1;
        private static readonly object nextOrganisationIdLock = new object();

        private static long GetNextOrganisationId()
        {
            lock (nextOrganisationIdLock)
            {
                long organisationIdToReturn = nextOrganisationId;
                nextOrganisationId++;
                return organisationIdToReturn;
            }
        }


        private long organisationId = GetNextOrganisationId();
        private string organisationName = "Test Organisation Ltd";
        private string companyNumber = "12345678";
        private SectorTypes sectorType = SectorTypes.Public;
        private OrganisationStatuses status = OrganisationStatuses.Active;
        private OrganisationScope scope = new OrganisationScope
        {
            ScopeStatus = ScopeStatuses.PresumedInScope,
            Status = ScopeRowStatuses.Active
        };
        private ICollection<OrganisationScope> scopes = new List<OrganisationScope>();

        public OrganisationBuilder WithOrganisationId(long organisationId)
        {
            this.organisationId = organisationId;
            return this;
        }

        public OrganisationBuilder WithOrganisationName(string organisationName)
        {
            this.organisationName = organisationName;
            return this;
        }

        public OrganisationBuilder WithCompanyNumber(string companyNumber)
        {
            this.companyNumber = companyNumber;
            return this;
        }

        public OrganisationBuilder WithSectorType(SectorTypes sectorType)
        {
            this.sectorType = sectorType;
            return this;
        }

        public OrganisationBuilder WithStatus(OrganisationStatuses status)
        {
            this.status = status;
            return this;
        }

        public Organisation Build()
        {
            scope.SnapshotDate = sectorType.GetAccountingStartDate(VirtualDateTime.Now.Year);
            scopes.Add(scope);
            
            return new Organisation
            {
                OrganisationId = organisationId,
                OrganisationName = organisationName,
                CompanyNumber = companyNumber,
                SectorType = sectorType,
                Status = status,
                OrganisationScopes = scopes
            };
        }

    }
}
