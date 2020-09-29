using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GenderPayGap.Core;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class UserBuilder
    {
        private long userId = 1;
        private string emailAddress = "user@test.com";
        private string firstname = "John";
        private string lastname = "Smith";
        private UserStatuses status = UserStatuses.Active;
        private DateTime emailVerifiedDate = DateTime.Now.AddDays(-5);
        private ICollection<UserOrganisation> organisations = new List<UserOrganisation>();

        public UserBuilder WithUserId(long userId)
        {
            this.userId = userId;
            return this;
        }

        public UserBuilder WithEmailAddress(string emailAddress)
        {
            this.emailAddress = emailAddress;
            return this;
        }

        public UserBuilder WithFirstname(string firstname)
        {
            this.firstname = firstname;
            return this;
        }

        public UserBuilder WithLastname(string lastname)
        {
            this.lastname = lastname;
            return this;
        }

        public UserBuilder WithStatus(UserStatuses status)
        {
            this.status = status;
            return this;
        }

        public UserBuilder WithOrganisation(Organisation organisation)
        {
            UserOrganisation userOrganisation = new UserOrganisationBuilder().ForUser(userId).ForOrganisation(organisation).Build();
            organisations.Add(userOrganisation);
            return this;
        }

        public User Build()
        {
            return new User
            {
                UserId = userId,
                EmailAddress = emailAddress,
                Firstname = firstname,
                Lastname = lastname,
                Status = status,
                EmailVerifiedDate = emailVerifiedDate,
                UserOrganisations = organisations
            };
        }

    }
}
