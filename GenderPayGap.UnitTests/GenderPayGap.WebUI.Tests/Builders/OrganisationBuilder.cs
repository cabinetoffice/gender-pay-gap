using GenderPayGap.Core;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class OrganisationBuilder
    {

        private long organisationId = 1;
        private string organisationName = "Test Organisation Ltd";
        private string companyNumber = "12345678";
        private SectorTypes sectorType = SectorTypes.Public;

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

        private Organisation Build()
        {
            return new Organisation
            {
                OrganisationId = organisationId,
                OrganisationName = organisationName,
                CompanyNumber = companyNumber,
                SectorType = sectorType
            };
        }
        
        public static implicit operator Organisation(OrganisationBuilder instance)
        {
            return instance.Build();
        }

    }
}
