using System;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class UserOrganisationBuilder
    {

        private long userId = 1;
        private long organisationId = 1;
        private DateTime created = DateTime.Now.AddDays(-5);
        private DateTime pinConfirmedDate = DateTime.Now.AddDays(-2);

        public UserOrganisationBuilder ForUser(long userId)
        {
            this.userId = userId;
            return this;
        }

        public UserOrganisationBuilder ForOrganisation(Organisation organisation)
        {
            organisationId = organisation.OrganisationId;
            return this;
        }

        public UserOrganisationBuilder WithCreatedDate(DateTime created)
        {
            this.created = created;
            return this;
        }

        public UserOrganisation Build()
        {
            return new UserOrganisation
            {
                UserId = userId, 
                OrganisationId = organisationId, 
                Created = created,
                PINConfirmedDate = pinConfirmedDate
            };
        }

    }
}
