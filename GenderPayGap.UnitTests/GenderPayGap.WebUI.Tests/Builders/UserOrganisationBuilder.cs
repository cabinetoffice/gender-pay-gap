using System;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class UserOrganisationBuilder
    {

        private long userId = 1;
        private User user = new UserBuilder().Build();
        private long organisationId = 1;
        private Organisation organisation = new OrganisationBuilder().Build();
        private DateTime created = DateTime.Now.AddDays(-5);
        private DateTime pinConfirmedDate = DateTime.Now.AddDays(-2);

        public UserOrganisationBuilder ForUser(User user)
        {
            userId = user.UserId;
            this.user = user;
            return this;
        }

        public UserOrganisationBuilder ForOrganisation(Organisation organisation)
        {
            organisationId = organisation.OrganisationId;
            this.organisation = organisation;
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
                User = user,
                OrganisationId = organisationId, 
                Organisation = organisation,
                Created = created,
                PINConfirmedDate = pinConfirmedDate
            };
        }

    }
}
