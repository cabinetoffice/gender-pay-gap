using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GenderPayGap.Core;
using GenderPayGap.Database;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class UserBuilder
    {

        private User userInProgress;

        public UserBuilder()
        {
            userInProgress = new User
            {
                UserId = 1,
                Firstname = "John",
                Lastname = "Smith",
                EmailAddress = "user@test.com",
                AcceptedPrivacyStatement = DateTime.Now,
                AllowContact = true,
                EmailVerifiedDate = DateTime.Now,
                Status = UserStatuses.Active,
                UserOrganisations = new List<UserOrganisation>()
            };
        }

        public UserBuilder DefaultAdminUser()
        {
            userInProgress.EmailAddress = "admin-user@geo.gov.uk";
            return this;
        }

        public UserBuilder WithUserId(long userId)
        {
            userInProgress.UserId = userId;
            return this;
        }

        public UserBuilder WithEmailAddress(string emailAddress)
        {
            userInProgress.EmailAddress = emailAddress;
            return this;
        }

        public UserBuilder WithFirstname(string firstname)
        {
            userInProgress.Firstname = firstname;
            return this;
        }

        public UserBuilder WithLastname(string lastname)
        {
            userInProgress.Lastname = lastname;
            return this;
        }

        public UserBuilder WithStatus(UserStatuses status)
        {
            userInProgress.Status = status;
            return this;
        }

        public UserBuilder WithOrganisation(Organisation organisation)
        {
            UserOrganisation userOrganisation = new UserOrganisationBuilder().ForUser(userInProgress).ForOrganisation(organisation).Build();
            userInProgress.UserOrganisations.Add(userOrganisation);
            return this;
        }

        public User Build()
        {
            return userInProgress;
        }

    }
}
