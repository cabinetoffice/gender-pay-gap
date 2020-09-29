using System;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using Moq;

namespace GenderPayGap.Tests.TestHelpers
{
    public static class UserHelper
    {

        public static User GetNotAdminUserWithoutVerifiedEmailAddress()
        {
            Guid id = Guid.NewGuid();
            return new User {
                EmailAddress = $"{id}@user.com",
                Firstname = $"FirstName{id}",
                Lastname = $"LastName{id}",
                JobTitle = $"JobTitle{id}",
                ContactPhoneNumber = $"ContactPhoneNumber{id}",
                UserId = new Random().Next(5000, 9999),
                Status = UserStatuses.Active
            };
        }

        public static User GetNotAdminUserWithVerifiedEmailAddress()
        {
            User user = GetNotAdminUserWithoutVerifiedEmailAddress();
            user.EmailVerifiedDate = VirtualDateTime.Now;
            return user;
        }

        public static User GetGovEqualitiesOfficeUser()
        {
            User user = GetNotAdminUserWithoutVerifiedEmailAddress();
            user.EmailAddress = "test@GovEqualitiesOfficeUser.gov.uk";
            return user;
        }

        public static User GetDatabaseAdmin()
        {
            return Mock.Of<User>(
                u => u.EmailAddress == "databaseadmin@email.com"
                     && u.UserId == new Random().Next(1000, 9999)
                     && u.ContactEmailAddress == "testContactEmailAddress@emailAddress.com"
                     && u.ContactFirstName == "testContactFirstName"
                     && u.ContactLastName == "testContactLastName");
        }

        public static User GetRegisteredUserAlreadyLinkedToAnOrganisation(UserOrganisation userOrganisation)
        {
            User user = GetNotAdminUserWithVerifiedEmailAddress();
            user.UserOrganisations = new[] {userOrganisation};
            return user;
        }

    }
}
