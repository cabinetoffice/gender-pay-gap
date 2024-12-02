using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.Helpers;

namespace GenderPayGap.Tests.Common.TestHelpers
{

    public static class UserHelpers
    {

        public static List<User> CreateUsers()
        {
            byte[] salt = Convert.FromBase64String("TestSalt");
            string passwordHash = PasswordHelper.GetPBKDF2("ad5bda75-e514-491b-b74d-4672542cbd15", salt);

            return new List<User> {
                new User {
                    UserId = 235251,
                    Firstname = "Firstname", Lastname = "Lastname", JobTitle = "JobTitle",
                    EmailAddress = "new1@ad5bda75-e514-491b-b74d-4672542cbd15.com",
                    EmailVerifiedDate = VirtualDateTime.Now,
                    PasswordHash = passwordHash,
                    Salt = Convert.ToBase64String(salt),
                    HashingAlgorithm = HashingAlgorithm.PBKDF2,
                    Status = UserStatuses.New
                },
                new User {
                    UserId = 23322,
                    Firstname = "Firstname", Lastname = "Lastname", JobTitle = "JobTitle",
                    EmailAddress = "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com",
                    EmailVerifiedDate = VirtualDateTime.Now,
                    PasswordHash = passwordHash,
                    Salt = Convert.ToBase64String(salt),
                    HashingAlgorithm = HashingAlgorithm.PBKDF2,
                    Status = UserStatuses.Active
                },
                new User {
                    UserId = 707643,
                    Firstname = "Firstname", Lastname = "Lastname", JobTitle = "JobTitle",
                    EmailAddress = "retired1@ad5bda75-e514-491b-b74d-4672542cbd15.com",
                    EmailVerifiedDate = VirtualDateTime.Now,
                    PasswordHash = passwordHash,
                    Salt = Convert.ToBase64String(salt),
                    HashingAlgorithm = HashingAlgorithm.PBKDF2,
                    Status = UserStatuses.Retired
                },
                new User {
                    UserId = 21555,
                    Firstname = "Firstname", Lastname = "Lastname", JobTitle = "JobTitle",
                    EmailAddress = "active2@ad5bda75-e514-491b-b74d-4672542cbd15.com",
                    EmailVerifiedDate = VirtualDateTime.Now,
                    PasswordHash = passwordHash,
                    Salt = Convert.ToBase64String(salt),
                    HashingAlgorithm = HashingAlgorithm.PBKDF2,
                    Status = UserStatuses.Active
                },
                new User {
                    UserId = 24572,
                    Firstname = "Firstname", Lastname = "Lastname", JobTitle = "JobTitle",
                    EmailAddress = "active3@ad5bda75-e514-491b-b74d-4672542cbd15.com",
                    EmailVerifiedDate = VirtualDateTime.Now,
                    PasswordHash = passwordHash,
                    Salt = Convert.ToBase64String(salt),
                    HashingAlgorithm = HashingAlgorithm.PBKDF2,
                    Status = UserStatuses.Active
                }
            };
        }

    }

}
