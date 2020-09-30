using System;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class UserOrganisationBuilder
    {
        private User user = new UserBuilder().Build();
        private Organisation organisation = new OrganisationBuilder().Build();
        private DateTime created = DateTime.Now.AddDays(-5);
        private DateTime pinConfirmedDate = DateTime.Now.AddDays(-2);

        public UserOrganisationBuilder ForUser(User user)
        {
            this.user = user;
            return this;
        }

        public UserOrganisationBuilder ForOrganisation(Organisation organisation)
        {
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
                UserId = user.UserId, 
                User = user,
                OrganisationId = organisation.OrganisationId, 
                Organisation = organisation,
                Created = created,
                PINConfirmedDate = pinConfirmedDate
            };
        }

    }
}
