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

        public User Build()
        {
            return new User
            {
                UserId = userId,
                EmailAddress = emailAddress,
                Firstname = firstname,
                Lastname = lastname,
                Status = status
            };
        }

        public static implicit operator User(UserBuilder instance)
        {
            return instance.Build();
        }

    }
}
