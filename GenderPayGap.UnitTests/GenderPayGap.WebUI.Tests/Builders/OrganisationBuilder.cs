﻿using GenderPayGap.Core;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class OrganisationBuilder
    {

        private long organisationId = 1;
        private string organisationName = "Test Organisation Ltd";
        private string companyNumber = "12345678";
        private SectorTypes sectorType = SectorTypes.Public;
        private OrganisationStatuses status = OrganisationStatuses.Active;

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
            return new Organisation
            {
                OrganisationId = organisationId,
                OrganisationName = organisationName,
                CompanyNumber = companyNumber,
                SectorType = sectorType,
                Status = status
            };
        }

    }
}