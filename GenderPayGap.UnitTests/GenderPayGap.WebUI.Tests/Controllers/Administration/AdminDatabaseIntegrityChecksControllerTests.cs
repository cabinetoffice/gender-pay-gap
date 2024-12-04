using GenderPayGap.Core;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Controllers.Admin;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace GenderPayGap.WebUI.Tests.Controllers.Administration
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class AdminDatabaseIntegrityChecksControllerTests
    {

        private static UserOrganisation CreateUserOrganisation(Organisation org, long userId)
        {
            return new UserOrganisation {Organisation = org, UserId = userId};
        }

        private static Organisation CreateOrganisation()
        {
            return new Organisation
            {
                OrganisationId = 1,
                OrganisationName = "Test Organisation",
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                CompanyNumber = "1234"
            };
        }

        private static User CreateUser()
        {
            return new User
            {
                UserId = 1,
                EmailAddress = "user@test.com",
                EmailAddressDB = "user@test.com",
                Firstname = "FirstName",
                Lastname = "LastName",
                JobTitle = "Job Title",
                EmailVerifiedDate = VirtualDateTime.Now,
                Status = UserStatuses.Active,
                PasswordHash = "PasswordHash"
            };
        }

        private static Return CreateReturn(long organisationId)
        {
            return new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 50,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisationId,
                Status = ReturnStatuses.Submitted
            };
        }

        private UserOrganisation userOrganisation;
        private User user;
        private Organisation organisation;
        private User govEqualitiesOfficeUser;
        private Return validReturn;
        private RouteData routeData;

        [SetUp]
        public void BeforeEach()
        {
            UiTestHelper.SetDefaultEncryptionKeys();
            
            organisation = CreateOrganisation();
            user = CreateUser();
            userOrganisation = CreateUserOrganisation(organisation, user.UserId);
            validReturn = CreateReturn(organisation.OrganisationId);

            govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            govEqualitiesOfficeUser.EmailVerifiedDate = VirtualDateTime.Now;

            routeData = new RouteData();
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithFiguresWithMoreThanOneDecimalPlace_Success()
        {
            // Arrange
            var invalidReturn = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = (decimal?) 50.555,
                DiffMeanHourlyPayPercent = 50,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };


            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn,
                validReturn);

            // Act
            var result = controller.ReturnsWithFiguresWithMoreThanOneDecimalPlace() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithQuartersFiguresSumDifferentThan100_Success()
        {
            // Arrange
            var invalidReturn = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 50,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 52,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn,
                validReturn);

            // Act
            var result = controller.ReturnsWithQuartersFiguresSumDifferentThan100() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithInvalidQuartersFigures_Success()
        {
            // Arrange
            var invalidReturn1 = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 50,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 120,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };

            var invalidReturn2 = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 50,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                OrganisationId = organisation.OrganisationId,
                OptedOutOfReportingPayQuarters = true,
                Status = ReturnStatuses.Submitted
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn1,
                invalidReturn2,
                validReturn);

            // Act
            var result = controller.ReturnsWithInvalidQuartersFigures() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn1, invalidReturn2);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithInvalidMeanMedianFigures_Success()
        {
            // Arrange
            var invalidReturn = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 120,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 52,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn,
                validReturn);

            // Act
            var result = controller.ReturnsWithInvalidMeanMedianFigures() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithInvalidBonusFigures_Success()
        {
            // Arrange
            var invalidReturn = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 12,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = 502,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn,
                validReturn);

            // Act
            var result = controller.ReturnsWithInvalidBonusFigures() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithInvalidBonusMeanMedianFigures_Success()
        {
            // Arrange
            var invalidReturn = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 500,
                DiffMeanHourlyPayPercent = 12,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn,
                validReturn);

            // Act
            var result = controller.ReturnsWithInvalidBonusMeanMedianFigures() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithMissingFigures_Success()
        {
            // Arrange
            var invalidReturn1 = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 12,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleMedianBonusPayPercent = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleMedianBonusPayPercent = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted,
                OptedOutOfReportingPayQuarters = false
            };

            var invalidReturn2 = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanHourlyPayPercent = 12,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn1,
                invalidReturn2,
                validReturn);

            // Act
            var result = controller.ReturnsWithMissingFigures() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn1, invalidReturn2);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_PrivateEmployersReturnsWithoutResponsiblePerson_Success()
        {
            // Arrange
            var invalidReturn = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 12,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn,
                validReturn);

            // Act
            var result = controller.PrivateEmployersReturnsWithoutResponsiblePerson() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithInvalidBonusFiguresGivenNoWomenBonus_Success()
        {
            // Arrange
            var invalidReturn = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 12,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = 0,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn,
                validReturn);

            // Act
            var result = controller.ReturnsWithInvalidBonusFiguresGivenNoWomenBonus() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithInvalidTextFieldsValues_Success()
        {
            // Arrange
            var invalidReturn = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = new string('*', 256),
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 12,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = 10,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn,
                validReturn);

            // Act
            var result = controller.ReturnsWithInvalidTextFieldsValues() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithInvalidCompanyLink_Success()
        {
            // Arrange
            var invalidReturn = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "test",
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 12,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = 10,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn,
                validReturn);

            // Act
            var result = controller.ReturnsWithInvalidCompanyLink() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn);
        }

        [Test]
        public void AdminDatabaseIntegrityChecksController_ReturnsWithInvalidOptedOutOfReportingPayQuartersValue_Success()
        {
            // Arrange
            var invalidReturn = new Return
            {
                AccountingDate = Global.PrivateAccountingDate,
                DiffMeanBonusPercent = 50,
                DiffMeanHourlyPayPercent = 12,
                DiffMedianBonusPercent = 50,
                DiffMedianHourlyPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = 10,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                OrganisationId = organisation.OrganisationId,
                Status = ReturnStatuses.Submitted,
                OptedOutOfReportingPayQuarters = true
            };

            var controller = UiTestHelper.GetController<AdminDatabaseIntegrityChecksController>(
                govEqualitiesOfficeUser.UserId,
                routeData,
                organisation,
                user,
                userOrganisation,
                govEqualitiesOfficeUser,
                invalidReturn,
                validReturn);

            // Act
            var result = controller.ReturnsWithInvalidOptedOutOfReportingPayQuartersValue() as PartialViewResult;

            // Assert
            AssertReturnsAreDisplayed(result, invalidReturn);
        }

        private void AssertReturnsAreDisplayed(PartialViewResult result, params Return[] returns)
        {
            Assert.NotNull(result);
            var model = result.Model as List<Return>;
            Assert.AreEqual(returns.Length, model.Count);

            foreach (Return ret in returns)
            {
                Assert.Contains(ret, model);
            }
        }

    }
}
