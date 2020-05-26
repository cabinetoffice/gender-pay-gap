using System;
using System.Collections.Generic;
using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;

namespace GenderPayGap.Tests.Common.TestHelpers
{

    public static class UserHelpers
    {

        public static List<User> CreateUsers()
        {
            byte[] salt = Convert.FromBase64String("TestSalt");
            string passwordHash = Crypto.GetPBKDF2("ad5bda75-e514-491b-b74d-4672542cbd15", salt);

            return new List<User> {
                new User {
                    UserId = 235251,
                    EmailAddress = "new1@ad5bda75-e514-491b-b74d-4672542cbd15.com",
                    EmailVerifiedDate = VirtualDateTime.Now,
                    PasswordHash = passwordHash,
                    Salt = Convert.ToBase64String(salt),
                    HashingAlgorithm = HashingAlgorithm.PBKDF2,
                    Status = UserStatuses.New
                },
                new User {
                    UserId = 23322,
                    EmailAddress = "active1@ad5bda75-e514-491b-b74d-4672542cbd15.com",
                    EmailVerifiedDate = VirtualDateTime.Now,
                    PasswordHash = passwordHash,
                    Salt = Convert.ToBase64String(salt),
                    HashingAlgorithm = HashingAlgorithm.PBKDF2,
                    Status = UserStatuses.Active
                },
                new User {
                    UserId = 707643,
                    EmailAddress = "retired1@ad5bda75-e514-491b-b74d-4672542cbd15.com",
                    EmailVerifiedDate = VirtualDateTime.Now,
                    PasswordHash = passwordHash,
                    Salt = Convert.ToBase64String(salt),
                    HashingAlgorithm = HashingAlgorithm.PBKDF2,
                    Status = UserStatuses.Retired
                },
                new User {
                    UserId = 21555,
                    EmailAddress = "active2@ad5bda75-e514-491b-b74d-4672542cbd15.com",
                    EmailVerifiedDate = VirtualDateTime.Now,
                    PasswordHash = passwordHash,
                    Salt = Convert.ToBase64String(salt),
                    HashingAlgorithm = HashingAlgorithm.PBKDF2,
                    Status = UserStatuses.Active
                },
                new User {
                    UserId = 24572,
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
