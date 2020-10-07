using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Tests.TestHelpers;

namespace GenderPayGap.WebUI.Tests.Builders
{
    public class UserBuilder
    {

        private readonly User userInProgress;
        private const string Salt = "salt";

        public UserBuilder()
        {
            userInProgress = new User
            {
                UserId = TestIdGenerator.GenerateIdFor<User>(),
                Firstname = "John",
                Lastname = "Smith",
                EmailAddress = "user@test.com",
                AcceptedPrivacyStatement = DateTime.Now,
                AllowContact = true,
                EmailVerifiedDate = DateTime.Now,
                Status = UserStatuses.Active,
                UserOrganisations = new List<UserOrganisation>(),
                HashingAlgorithm = HashingAlgorithm.PBKDF2,
                Salt = Salt,
                PasswordHash = Crypto.GetPBKDF2("password", Convert.FromBase64String(Salt))
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
            organisation.UserOrganisations.Add(userOrganisation);
            return this;
        }
        
        public UserBuilder WithPassword(string password)
        {
            userInProgress.Salt = Salt;
            userInProgress.PasswordHash = Crypto.GetPBKDF2(password, Convert.FromBase64String(Salt));
            userInProgress.HashingAlgorithm = HashingAlgorithm.PBKDF2; 
            return this; 
        }

        public User Build()
        {
            return userInProgress;
        }

    }
}
