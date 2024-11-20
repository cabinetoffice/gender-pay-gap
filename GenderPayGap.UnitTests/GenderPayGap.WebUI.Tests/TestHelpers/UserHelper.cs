using System;
using GenderPayGap.Core;
using GenderPayGap.Database;

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

        public static User GetGovEqualitiesOfficeUser()
        {
            User user = GetNotAdminUserWithoutVerifiedEmailAddress();
            user.EmailAddress = "test@geo.gov.uk";
            return user;
        }

    }
}
