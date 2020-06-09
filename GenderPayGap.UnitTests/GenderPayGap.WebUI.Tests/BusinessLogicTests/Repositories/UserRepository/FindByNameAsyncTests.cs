using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic.Account.Abstractions;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.WebUI.Services;
using MockQueryable.Moq;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.BusinessLogic.Tests.Repositories.UserRepository
{
    [TestFixture]
    public class FindByNameAsyncTests
    {

        [SetUp]
        public void SetUp()
        {
            var lisOfUsersInTheDatabase = new List<User> {
                new User {
                    JobTitle = "FirstNameJohn",
                    Firstname = "JOHN",
                    Lastname = "",
                    ContactFirstName = "",
                    ContactLastName = ""
                },
                new User {
                    JobTitle = "LastNameSmith",
                    Firstname = "",
                    Lastname = "SMITH",
                    ContactFirstName = "",
                    ContactLastName = ""
                },
                new User {
                    JobTitle = "FirstPlusLastNameMarieBrown",
                    Firstname = "MARIE",
                    Lastname = "BROWN",
                    ContactFirstName = "",
                    ContactLastName = ""
                },
                new User {
                    JobTitle = "ContactFirstNameBob",
                    Firstname = "",
                    Lastname = "",
                    ContactFirstName = "BOB",
                    ContactLastName = ""
                },
                new User {
                    JobTitle = "ContactLastNameThomas",
                    Firstname = "",
                    Lastname = "",
                    ContactFirstName = "",
                    ContactLastName = "THOMAS"
                },
                new User {
                    JobTitle = "ContactFirstNamePlusContactLastNameEnzoClay",
                    Firstname = "",
                    Lastname = "",
                    ContactFirstName = "ENZO",
                    ContactLastName = "CLAY"
                },
                new User {
                    JobTitle = "FirstNameContainsJohn",
                    Firstname = "JOHNNY",
                    Lastname = "",
                    ContactFirstName = "",
                    ContactLastName = ""
                },
                new User {
                    JobTitle = "LastNameContainsSmith",
                    Firstname = "",
                    Lastname = "BLACKSMITH",
                    ContactFirstName = "",
                    ContactLastName = ""
                },
                new User {
                    JobTitle = "FirstPlusLastNameContainsMarieBrown",
                    Firstname = "ANNEMARIE",
                    Lastname = "BROWNLOW",
                    ContactFirstName = "",
                    ContactLastName = ""
                },
                new User {
                    JobTitle = "ContactFirstNameContainsBob",
                    Firstname = "",
                    Lastname = "",
                    ContactFirstName = "BOBBIE",
                    ContactLastName = ""
                },
                new User {
                    JobTitle = "ContactLastNameContainsThomas",
                    Firstname = "",
                    Lastname = "",
                    ContactFirstName = "",
                    ContactLastName = "THOMASON"
                },
                new User {
                    JobTitle = "ContactFirstNamePlusContactLastNameContainsEnzoClay",
                    Firstname = "",
                    Lastname = "",
                    ContactFirstName = "VINCENZO",
                    ContactLastName = "CLAYTON"
                }
            };

            var configurableDataRepository = new Mock<IDataRepository>();
            configurableDataRepository
                .Setup(x => x.GetAll<User>())
                .Returns(
                    lisOfUsersInTheDatabase.AsQueryable()
                        .BuildMock()
                        .Object);

            var auditLoggerWithMocks = new AuditLogger(Mock.Of<IDataRepository>(), Mock.Of<IHttpSession>());

            _configuredIUserRepository = new Account.Repositories.UserRepository(
                configurableDataRepository.Object, auditLoggerWithMocks);
        }

        private IUserRepository _configuredIUserRepository;

        [Test]
        public async Task FindAllUsersByNameAsync_When_ContactFirstName_And_ContactLastName_Found_Returns_List()
        {
            // Arrange
            var userContactFirstNameSpaceContactLastName = "enzo clay";

            // Act
            List<User> actual = await _configuredIUserRepository.FindAllUsersByNameAsync(userContactFirstNameSpaceContactLastName);

            // Assert
            User userEnzoClay = actual.FirstOrDefault(
                x => x.ContactFirstName.ToLower() + " " + x.ContactLastName.ToLower() == userContactFirstNameSpaceContactLastName);
            Assert.NotNull(
                userEnzoClay,
                "Expected to have found a user whose ContactFirstName was " + userContactFirstNameSpaceContactLastName);
            actual.Remove(
                userEnzoClay); // Checked the first element, the list should have more than one element, remove the checked object and continue with the assertions.

            User userVincenzoClayton = actual.FirstOrDefault(
                x => (x.ContactFirstName.ToLower() + " " + x.ContactLastName.ToLower()).Contains(
                    userContactFirstNameSpaceContactLastName.ToLower()));
            Assert.NotNull(
                userVincenzoClayton,
                "expected to have found a user whose ContactFirstName contained " + userContactFirstNameSpaceContactLastName);
        }

        [Test]
        public async Task FindAllUsersByNameAsync_When_ContactFirstName_Found_Returns_List()
        {
            // Arrange
            var userContactFirstName = "bob";

            // Act
            List<User> actual = await _configuredIUserRepository.FindAllUsersByNameAsync(userContactFirstName);

            // Assert
            User userBob = actual.FirstOrDefault(x => x.ContactFirstName.ToLower() == userContactFirstName);
            Assert.NotNull(userBob, "Expected to have found a user whose ContactFirstName was " + userContactFirstName);
            actual.Remove(
                userBob); // Checked the first element, the list should have more than one element, remove the checked object and continue with the assertions.

            User userBobbie = actual.FirstOrDefault(x => x.ContactFirstName.ToLower().Contains(userContactFirstName));
            Assert.NotNull(userBobbie, "expected to have found a user whose ContactFirstName contained " + userContactFirstName);
        }

        [Test]
        public async Task FindAllUsersByNameAsync_When_ContactLastName_Found_Returns_List()
        {
            // Arrange
            var userContactLastName = "thomas";

            // Act
            List<User> actual = await _configuredIUserRepository.FindAllUsersByNameAsync(userContactLastName);

            // Assert
            User userThomas = actual.FirstOrDefault(x => x.ContactLastName.ToLower() == userContactLastName);
            Assert.NotNull(userThomas, "Expected to have found a user whose ContactLastName was " + userContactLastName);
            actual.Remove(
                userThomas); // Checked the first element, the list should have more than one element, remove the checked object and continue with the assertions.

            User userThomason = actual.FirstOrDefault(x => x.ContactLastName.ToLower().Contains(userContactLastName.ToLower()));
            Assert.NotNull(userThomason, "expected to have found a user whose ContactLastName contained " + userContactLastName);
        }

        [Test]
        public async Task FindAllUsersByNameAsync_When_Firstname_And_LastName_Found_Returns_List()
        {
            // Arrange
            var userFirstNameSpaceLastName = "marie brown";

            // Act
            List<User> actual = await _configuredIUserRepository.FindAllUsersByNameAsync(userFirstNameSpaceLastName);

            // Assert
            User userMarieBrown =
                actual.FirstOrDefault(x => x.Firstname.ToLower() + " " + x.Lastname.ToLower() == userFirstNameSpaceLastName);
            Assert.NotNull(userMarieBrown, "Expected to have found a user whose FirstName and LastName were " + userFirstNameSpaceLastName);
            actual.Remove(
                userMarieBrown); // Checked the first element, the list should have more than one element, remove the checked object and continue with the assertions.

            User userAnnemarieBrownlow = actual.FirstOrDefault(
                x => (x.Firstname.ToLower() + " " + x.Lastname.ToLower()).Contains(userFirstNameSpaceLastName.ToLower()));
            Assert.NotNull(
                userAnnemarieBrownlow,
                "Expected to have found a user whose FirstName and LastName contained " + userFirstNameSpaceLastName);
        }

        [Test]
        public async Task FindAllUsersByNameAsync_When_Firstname_Found_Returns_List()
        {
            // Arrange
            var userFirstName = "john";

            // Act
            List<User> actual = await _configuredIUserRepository.FindAllUsersByNameAsync(userFirstName);

            // Assert
            User userJohn = actual.FirstOrDefault(x => x.Firstname.ToLower() == userFirstName);
            Assert.NotNull(userJohn, "Expected to have found a user whose FirstName was " + userFirstName);
            actual.Remove(
                userJohn); // Checked the first element, the list should have more than one element, remove the checked object and continue with the assertions.

            User userJohnny = actual.FirstOrDefault(x => x.Firstname.ToLower().Contains(userFirstName));
            Assert.NotNull(userJohnny, "expected to have found a user whose FirstName contained " + userFirstName);
        }

        [Test]
        public async Task FindAllUsersByNameAsync_When_LastName_Found_Returns_List()
        {
            // Arrange
            var userLastName = "smith";

            // Act
            List<User> actual = await _configuredIUserRepository.FindAllUsersByNameAsync(userLastName);

            // Assert
            User userSmith = actual.FirstOrDefault(x => x.Lastname.ToLower() == userLastName);
            Assert.NotNull(userSmith, "Expected to have found a user whose Lastname was " + userLastName);
            actual.Remove(
                userSmith); // Checked the first element, the list should have more than one element, remove the checked object and continue with the assertions.

            User userBlacksmith = actual.FirstOrDefault(x => x.Lastname.ToLower().Contains(userLastName.ToLower()));
            Assert.NotNull(userBlacksmith, "expected to have found a user whose Lastname contained " + userLastName);
        }

    }
}
