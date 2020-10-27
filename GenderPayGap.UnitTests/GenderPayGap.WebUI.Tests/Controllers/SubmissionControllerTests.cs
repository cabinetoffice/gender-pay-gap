using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.BusinessLogic.Classes;
using GenderPayGap.WebUI.BusinessLogic.Models.Organisation;
using GenderPayGap.WebUI.BusinessLogic.Models.Submit;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Controllers.Submission;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers
{
    [TestFixture]
    [SetCulture("en-GB")]
    public partial class SubmitControllerTests
    {

        [Test]
        public async Task
            SubmitController_CheckData_GET_ShouldProvideLateReason_Returns_False_When_Public_Org_Reports_On_Time_And_No_Records_In_Db()
        {
            // Arrange
            var user = new User
            {
                UserId = 98788,
                Firstname = "Joe",
                Lastname = "Ken",
                JobTitle = "carpenter",
                EmailAddress = "joekenthecarpenter@google.com",
                EmailVerifiedDate = new DateTime(2017, 03, 13, 13, 23, 0)
            };

            var organisation = new Organisation { OrganisationId = 1254, SectorType = SectorTypes.Public };

            DateTime currentYearAccountingStartDate = organisation.SectorType.GetAccountingStartDate();
            var previousYearsAccountingStartYear = currentYearAccountingStartDate
                .AddYears(-1)
                .Year
                .ToInt32();

            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                UserId = user.UserId,
                PINConfirmedDate = new DateTime(2018, 02, 12, 12, 32, 32),
                PIN = null
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            var model = new ReturnViewModel
            {
                SectorType = organisation.SectorType,
                ReportInfo = new ReportInfoModel
                {
                    Draft = new Draft(organisation.OrganisationId, previousYearsAccountingStartYear)
                    {
                        HasDraftBeenModifiedDuringThisSession = true,
                        IsUserAllowedAccess = true
                    }
                }
            };

            var controller = UiTestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = currentYearAccountingStartDate.Year; // reporting in time
            controller.StashModel(model);

            // Act:
            var result = await controller.CheckData() as ViewResult;
            Assert.NotNull(result);
            var resultModel = result?.Model as ReturnViewModel;
            Assert.NotNull(resultModel);

            // Assert:
            Assert.False(
                resultModel?.ShouldProvideLateReason,
                $"The user is reporting for deadline {currentYearAccountingStartDate.ToString("dd/MMM/yy", DateTimeFormatInfo.InvariantInfo)} - the user is on time -, the flag 'should provide late reason' was expected to be 'false'.");
        }

        [Test]
        public async Task
            SubmitController_CheckData_GET_ShouldProvideLateReason_Returns_True_When_Private_Org_Reports_Late_And_No_Records_In_Db()
        {
            // Arrange
            var user = new User
            {
                UserId = 49,
                Firstname = "Harry",
                Lastname = "Kane",
                JobTitle = "England striker",
                EmailAddress = "harryKane@englandStrikersAssociation.co.uk",
                EmailVerifiedDate = new DateTime(2017, 04, 12, 18, 24, 0)
            };

            var organisation = new Organisation { OrganisationId = 6812, SectorType = SectorTypes.Private };

            DateTime currentYearAccountingStartDate = organisation.SectorType.GetAccountingStartDate();
            var previousYearsAccountingStartYear = currentYearAccountingStartDate
                .AddYears(-1)
                .Year
                .ToInt32();

            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                UserId = user.UserId,
                PINConfirmedDate = new DateTime(2018, 02, 19, 11, 30, 31),
                PIN = null
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            var model = new ReturnViewModel
            {
                SectorType = organisation.SectorType,
                ReportInfo = new ReportInfoModel
                {
                    Draft = new Draft(organisation.OrganisationId, previousYearsAccountingStartYear)
                    {
                        HasDraftBeenModifiedDuringThisSession = true,
                        IsUserAllowedAccess = true
                    }
                }
            };

            var controller = UiTestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = previousYearsAccountingStartYear; // reporting for previous year, user is late
            controller.StashModel(model);

            // Act:
            var result = await controller.CheckData() as ViewResult;
            Assert.NotNull(result);
            var resultModel = result?.Model as ReturnViewModel;
            Assert.NotNull(resultModel);

            // Assert:
            Assert.True(
                resultModel?.ShouldProvideLateReason,
                $"The user is reporting for deadline {currentYearAccountingStartDate.ToString("dd/MMM/yy", DateTimeFormatInfo.InvariantInfo)} - the user is late -, the flag 'should provide late reason' was expected to be 'true'.");
        }

        [Test]
        [Description("Ensure the Check Data form is returned for the current user ")]
        public async Task SubmitController_CheckData_GET_Success()
        {
            // Arrange
            var user = new User
            {
                UserId = new Random().Next(1000, 9999),
                Firstname = "First_test",
                Lastname = "Last_test",
                JobTitle = "JobTitle_test",
                EmailAddress = "magnuski@hotmail.com",
                EmailVerifiedDate = VirtualDateTime.Now
            };
            var organisation = new Organisation { OrganisationId = new Random().Next(1000, 9999), SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = user.UserId,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            //Stash an object to pass in for this.ClearStash()
            var model = new ReturnViewModel
            {
                DiffMeanBonusPercent = 12,
                DiffMeanHourlyPayPercent = 14,
                DiffMedianBonusPercent = 12,
                DiffMedianHourlyPercent = 43,
                FemaleLowerPayBand = 23,
                FemaleMedianBonusPayPercent = 21,
                FemaleMiddlePayBand = 16,
                FemaleUpperPayBand = 17,
                FemaleUpperQuartilePayBand = 41,
                MaleLowerPayBand = 12,
                MaleMedianBonusPayPercent = 11,
                MaleMiddlePayBand = 56,
                MaleUpperPayBand = 33,
                MaleUpperQuartilePayBand = 42,
                OrganisationId = 1,
                OrganisationName = "test org name",
                ReturnId = 1,
                SectorType = SectorTypes.Private
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            model.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller = UiTestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = 2017;
            controller.StashModel(model);

            //ACT:
            var result = await controller.CheckData() as ViewResult;
            var resultModel = result.Model as ReturnViewModel;

            //ASSERT:
            Assert.That(
                result != null && result.GetType() == typeof(ViewResult),
                " Incorrect resultType returned"); //TODO redundant again due to previous line
            Assert.AreEqual("CheckData", result.ViewName, "Incorrect view returned");
            Assert.NotNull(resultModel, "Expected ReturnViewModel");
            Assert.That(
                resultModel != null && resultModel.GetType() == typeof(ReturnViewModel),
                "Expected ReturnViewModel or Incorrect viewModel returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            await testService.DiscardDraftFileAsync(model);
        }

        [Test]
        [Description("Ensure the Check Data returns Error View Model When User Isn't verified")]
        public async Task SubmitController_CheckData_GET_When_Not_Verified_User()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = null };
            var organisation = new Organisation { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            var controller = UiTestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);

            //ACT:
            var result = await controller.CheckData() as ViewResult;
            Assert.NotNull(result);

            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel)result.Model).Title);
        }

        [Test]
        [Description("Ensure Check Data loads an existing submission when no stashed model")]
        public async Task SubmitController_CheckData_GET_When_Old_Submission_NoError()
        {
            // Arrange
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            mockedOrganisation.OrganisationId = new Random().Next(5000, 9999);
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            var expectedReturnViewModel = new ReturnViewModel
            {
                ReturnId = mockedReturn.ReturnId,
                AccountingDate = mockedReturn.AccountingDate,
                DiffMeanBonusPercent = mockedReturn.DiffMeanBonusPercent,
                DiffMeanHourlyPayPercent = mockedReturn.DiffMeanHourlyPayPercent,
                DiffMedianBonusPercent = mockedReturn.DiffMedianBonusPercent,
                DiffMedianHourlyPercent = mockedReturn.DiffMedianHourlyPercent,
                FemaleLowerPayBand = mockedReturn.FemaleLowerPayBand,
                FemaleMedianBonusPayPercent = mockedReturn.FemaleMedianBonusPayPercent,
                FemaleMiddlePayBand = mockedReturn.FemaleMiddlePayBand,
                FemaleUpperPayBand = mockedReturn.FemaleUpperPayBand,
                FemaleUpperQuartilePayBand = mockedReturn.FemaleUpperQuartilePayBand,
                MaleLowerPayBand = mockedReturn.MaleLowerPayBand,
                MaleMedianBonusPayPercent = mockedReturn.MaleMedianBonusPayPercent,
                MaleMiddlePayBand = mockedReturn.MaleMiddlePayBand,
                MaleUpperPayBand = mockedReturn.MaleUpperPayBand,
                MaleUpperQuartilePayBand = mockedReturn.MaleUpperQuartilePayBand,
                SectorType = mockedReturn.Organisation.SectorType,
                LateReason = null
            };

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            #region mocking DB

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            //controller.StashModel(model);

            #endregion

            //ACT:

            var result = await controller.CheckData() as ViewResult;
            var actualReturnViewModel = result?.Model as ReturnViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "CheckData", $"Expected a ViewResult to CheckData but was '{result.ViewName}'");
            Assert.NotNull(actualReturnViewModel, "Unstashed model is Invalid Expected ReturnViewModel");

            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");

            Assert.Multiple(
                () =>
                {
                    Assert.AreEqual(
                        expectedReturnViewModel.AccountingDate,
                        actualReturnViewModel.AccountingDate,
                        "AccountingDate value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.DiffMeanBonusPercent,
                        actualReturnViewModel.DiffMeanBonusPercent,
                        "DiffMeanBonusPercent value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.DiffMeanHourlyPayPercent,
                        actualReturnViewModel.DiffMeanHourlyPayPercent,
                        "DiffMeanHourlyPayPercent value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.DiffMedianBonusPercent,
                        actualReturnViewModel.DiffMedianBonusPercent,
                        "DiffMedianBonusPercent value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.DiffMedianHourlyPercent,
                        actualReturnViewModel.DiffMedianHourlyPercent,
                        "DiffMedianHourlyPercent value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.FemaleLowerPayBand,
                        actualReturnViewModel.FemaleLowerPayBand,
                        "FemaleLowerPayBand value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.FemaleMedianBonusPayPercent,
                        actualReturnViewModel.FemaleMedianBonusPayPercent,
                        "FemaleMedianBonusPayPercent value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.FemaleMiddlePayBand,
                        actualReturnViewModel.FemaleMiddlePayBand,
                        "FemaleMiddlePayBand value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.FemaleUpperPayBand,
                        actualReturnViewModel.FemaleUpperPayBand,
                        "FemaleUpperPayBand value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.FemaleUpperQuartilePayBand,
                        actualReturnViewModel.FemaleUpperQuartilePayBand,
                        "FemaleUpperQuartilePayBand value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.MaleLowerPayBand,
                        actualReturnViewModel.MaleLowerPayBand,
                        "MaleLowerPayBand value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.MaleMiddlePayBand,
                        actualReturnViewModel.MaleMiddlePayBand,
                        "MaleMiddlePayBand value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.MaleUpperPayBand,
                        actualReturnViewModel.MaleUpperPayBand,
                        "MaleUpperPayBand value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.MaleUpperQuartilePayBand,
                        actualReturnViewModel.MaleUpperQuartilePayBand,
                        "MaleUpperQuartilePayBand value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.ReturnId,
                        actualReturnViewModel.ReturnId,
                        "ReturnId value does not match model");
                    Assert.AreEqual(
                        expectedReturnViewModel.SectorType,
                        actualReturnViewModel.SectorType,
                        "SectorType value does not match model");
                });
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("Ensure that CheckData Excludes The Person Responsible Section When Sector Is Public")]
        public async Task SubmitController_CheckData_POST_Success_When_Sector_Is_Public_Then_Exclude_Person_Responsible_Section()
        {
            // Arrange
            var testUserId = 1000;
            var testReturnId = 1000;
            var testOrganisationId = 2421;
            var user = new User { UserId = testUserId, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address = new OrganisationAddress
            {
                OrganisationId = testOrganisationId,
                Address1 = "Address line 1",
                PostCode = "PC1",
                Status = AddressStatuses.Active
            };
            var organisation = new Organisation { OrganisationId = testOrganisationId, SectorType = SectorTypes.Public };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = testUserId,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };
            DateTime testAccountingDate = SectorTypes.Public.GetAccountingStartDate();

            //mock return existing in the DB
            var @return = new Return
            {
                ReturnId = testReturnId,
                OrganisationId = testOrganisationId,
                CompanyLinkToGPGInfo = "http://www.test.com",
                AccountingDate = testAccountingDate
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            //mock entered 'return' at review CheckData view
            var model = new ReturnViewModel
            {
                SectorType = SectorTypes.Public,
                AccountingDate = testAccountingDate,
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 30,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 50,
                MaleLowerPayBand = 30,
                MaleMedianBonusPayPercent = 40,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 70,
                OrganisationId = organisation.OrganisationId,
                ReturnId = testReturnId
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            model.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller = UiTestHelper.GetController<SubmitController>(
                testUserId,
                routeData,
                user,
                address,
                organisation,
                userOrganisation,
                @return);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = SectorTypes.Public.GetAccountingStartDate().Year;

            controller.Bind(model);

            controller.StashModel(model);

            // ACT
            var result = await controller.CheckData(model) as RedirectToActionResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.That(
                result != null && result.GetType() == typeof(RedirectToActionResult),
                "Expected RedirectToActionResult or Incorrect resultType returned");
            Assert.AreEqual("SubmissionComplete", result.ActionName, "Incorrect view returned");

            // Clean up
            await testService.DiscardDraftFileAsync(model);
        }

        [Test]
        public async Task SubmitController_CheckData_POST_When_CompanyLinkToGPGInfo_Is_Bigger_Than_250_Char_Model_Is_Invalid()
        {
            // Arrange
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation);
            mockedDatabase.Setup(x => x.Get<User>(It.IsAny<long>())).Returns(mockedUser);
            mockedDatabase.Setup(x => x.Get<Organisation>(It.IsAny<long>())).Returns(mockedOrganisation);

            var returnViewModel = new ReturnViewModel
            {
                CompanyLinkToGPGInfo =
                    "This string represents a company link to gpg info that is purposely longer than 255 characters so this test fails, This string represents a company link to gpg info that is purposely longer than 255 characters so this test, fails This string represents a company link"
            };

            controller.Bind(returnViewModel);
            controller.StashModel(returnViewModel);

            // Act
            var result = await controller.CheckData(returnViewModel) as ViewResult;
            var actualReturnViewModel = result?.Model as ReturnViewModel;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("CheckData", result.ViewName, "Incorrect view returned");
            Assert.NotNull(actualReturnViewModel, "Expected a ReturnViewModel object, null object is returned");

            Assert.False(result.ViewData.ModelState.IsValid, "Model is Invalid");
            Assert.False(
                result.ViewData.ModelState.IsValidField("CompanyLinkToGPGInfo"),
                "Expected CompanyLinkToGPGInfo value to be considered invalid as it should be set to > 255 characters");

            ModelErrorCollection companyLinkToGPGInfoModelStateErrors = controller.ModelState["CompanyLinkToGPGInfo"].Errors;

            const string expectedError = "The web address (URL) cannot be longer than 255 characters.";
            string errorMessage = $"Unable to find expected Error {expectedError}. List available: ";
            var errorFound = false;

            for (var i = 0; i < companyLinkToGPGInfoModelStateErrors.Count; i++)
            {
                if (errorFound)
                {
                    break;
                }

                errorFound = companyLinkToGPGInfoModelStateErrors[i].ErrorMessage == expectedError;

                errorMessage += $"Error {i}-{companyLinkToGPGInfoModelStateErrors[i].ErrorMessage} ";
            }

            Assert.True(errorFound, errorMessage);
        }

        [Test(Author = "Oscar Lagattas")]
        [Description("Ensure that CheckData form has at least one invalid data point value")]
        public async Task SubmitController_CheckData_POST_When_ModelState_Is_Invalid_Then_Returns_Error()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address = new OrganisationAddress
            {
                OrganisationId = 1,
                Address1 = "Address line 1",
                PostCode = "PC1",
                Status = AddressStatuses.Active
            };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //mock return existing in the DB
            var @return = new Return { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "http://www.test.com" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            //mock entered 'return' at review CheckData view
            var model = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 30,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 30,
                MaleMedianBonusPayPercent = 40,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 9999, // makes ModelState Invalid
                OrganisationId = organisation.OrganisationId,
                ReturnId = 987896
            };

            var controller = UiTestHelper.GetController<SubmitController>(
                1,
                routeData,
                user,
                address,
                organisation,
                userOrganisation,
                @return);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = SectorTypes.Private.GetAccountingStartDate().Year;

            controller.Bind(model);
            controller.StashModel(model);

            //ACT:
            var result = await controller.CheckData(model) as RedirectToRouteResult;

            Assert.AreNotEqual(controller.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"), true);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("Check Data Returns Error when User Isn't Registered")]
        public async Task SubmitController_CheckData_POST_When_Not_Registered_User()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = null };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user);
            controller.ReportingOrganisationStartYear = SectorTypes.Private.GetAccountingStartDate().Year;

            //ACT:
            var result = await controller.CheckData(new ReturnViewModel()) as ViewResult;

            // ASSERT:
            Assert.NotNull(result);
            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel)result.Model).Title);
        }


        [Test(Author = "Oscar Lagatta")]
        [Description("Ensure that CheckData form hasn't changed Figures or PersonResponsible or OrganisationSize or Website URL")]
        public async Task SubmitController_CheckData_POST_When_PersonResponsible_Hasnt_Changed_Then_Returns_Bad_Request()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);
            //var mockedModel = OrganisationViewModelHelper.GetMockedOrganisationModelHelperForReturn(mockedReturn, OrganisationSizes.NotProvided);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;
            ;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            mockedDatabase.Setup(x => x.Get<Organisation>(It.IsAny<long>())).Returns(mockedOrganisation);


            mockedDatabase
                .Setup(md => md.Insert(It.IsAny<Return>()))
                .Callback<Return>(returnSentIn => { returnSentIn.Organisation = mockedOrganisation; });

            var returnViewModel = new ReturnViewModel
            {
                CompanyLinkToGPGInfo = mockedReturn.CompanyLinkToGPGInfo,
                DiffMeanBonusPercent = mockedReturn.DiffMeanBonusPercent,
                DiffMeanHourlyPayPercent = mockedReturn.DiffMeanHourlyPayPercent,
                DiffMedianBonusPercent = mockedReturn.DiffMedianBonusPercent,
                DiffMedianHourlyPercent = mockedReturn.DiffMedianHourlyPercent,
                FemaleLowerPayBand = mockedReturn.FemaleLowerPayBand,
                FemaleMedianBonusPayPercent = mockedReturn.FemaleMedianBonusPayPercent,
                FemaleMiddlePayBand = mockedReturn.FemaleMiddlePayBand,
                FemaleUpperPayBand = mockedReturn.FemaleUpperPayBand,
                FemaleUpperQuartilePayBand = mockedReturn.FemaleUpperQuartilePayBand,
                FirstName = mockedReturn.FirstName,
                LastName = mockedReturn.LastName,
                JobTitle = mockedReturn.JobTitle,
                MaleLowerPayBand = mockedReturn.MaleLowerPayBand,
                MaleMedianBonusPayPercent = mockedReturn.MaleMedianBonusPayPercent,
                MaleMiddlePayBand = mockedReturn.MaleMiddlePayBand,
                MaleUpperPayBand = mockedReturn.MaleUpperPayBand,
                MaleUpperQuartilePayBand = mockedReturn.MaleUpperQuartilePayBand,
                OrganisationId = mockedOrganisation.OrganisationId,
                ReturnId = mockedOrganisation.Returns.First().ReturnId,
                LateReason = mockedReturn.LateReason,
                EHRCResponse = mockedReturn.EHRCResponse.ToString()
            };

            returnViewModel.ReportInfo = new ReportInfoModel
            {
                Draft = new Draft(returnViewModel.OrganisationId, returnViewModel.AccountingDate.Year) { IsUserAllowedAccess = true }
            };

            controller.Bind(returnViewModel);
            controller.StashModel(returnViewModel);

            //ACT:
            var result = await controller.CheckData(returnViewModel) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            // when the request is null 
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("Ensure that CheckData form Shouldn't Change Modified Date")]
        public async Task SubmitController_CheckData_POST_When_Should_not_Change_Modified_Date_Then_Sets_ModifiedDate()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;
            ;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            mockedDatabase.Setup(x => x.Get<Organisation>(It.IsAny<long>())).Returns(mockedOrganisation);

            mockedDatabase
                .Setup(md => md.Insert(It.IsAny<Return>()))
                .Callback<Return>(returnSentIn => { returnSentIn.Organisation = mockedOrganisation; });

            var returnViewModel = new ReturnViewModel
            {
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = mockedReturn.DiffMeanBonusPercent,
                DiffMeanHourlyPayPercent = mockedReturn.DiffMeanHourlyPayPercent,
                DiffMedianBonusPercent = mockedReturn.DiffMedianBonusPercent,
                DiffMedianHourlyPercent = mockedReturn.DiffMedianHourlyPercent,
                FemaleLowerPayBand = mockedReturn.FemaleLowerPayBand,
                FemaleMedianBonusPayPercent = mockedReturn.FemaleMedianBonusPayPercent,
                FemaleMiddlePayBand = mockedReturn.FemaleMiddlePayBand,
                FemaleUpperPayBand = mockedReturn.FemaleUpperPayBand,
                FemaleUpperQuartilePayBand = mockedReturn.FemaleUpperQuartilePayBand,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = mockedReturn.MaleLowerPayBand,
                MaleMedianBonusPayPercent = mockedReturn.MaleMedianBonusPayPercent,
                MaleMiddlePayBand = mockedReturn.MaleMiddlePayBand,
                MaleUpperPayBand = mockedReturn.MaleUpperPayBand,
                MaleUpperQuartilePayBand = mockedReturn.MaleUpperQuartilePayBand,
                OrganisationId = mockedOrganisation.OrganisationId,
                ReturnId = mockedOrganisation.Returns.First().ReturnId,
                LateReason = "Test Late Reason",
                EHRCResponse = "1"
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                mockedOrganisation.OrganisationId,
                mockedOrganisation.SectorType.GetAccountingStartDate().Year,
                mockedUser.UserId);

            #endregion

            controller.Bind(returnViewModel);
            controller.StashModel(returnViewModel);

            Global.EnableSubmitAlerts = true;

            //ACT:
            var result = await controller.CheckData(returnViewModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("SubmissionComplete", result.ActionName);

            // Cleanup of the draft isn't required as the submission complete work is expected to have removed it anyway.
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("Ensure that CheckData has an existing previous return with same return Id")]
        public async Task SubmitController_CheckData_POST_When_The_Return_Already_Exists_Then_Returns_The_Old_Submission()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;
            ;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            mockedDatabase.Setup(x => x.Get<Organisation>(It.IsAny<long>())).Returns(mockedOrganisation);

            mockedDatabase
                .Setup(md => md.Insert(It.IsAny<Return>()))
                .Callback<Return>(returnSentIn => { returnSentIn.Organisation = mockedOrganisation; });

            var model = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 30,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 30,
                MaleMedianBonusPayPercent = 40,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 70,
                OrganisationId = mockedOrganisation.OrganisationId,
                ReturnId = mockedOrganisation.Returns.First().ReturnId,
                LateReason = "Test Late Reason",
                EHRCResponse = "2"
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            model.ReportInfo.Draft = await testService.GetDraftFileAsync(
                mockedOrganisation.OrganisationId,
                mockedOrganisation.SectorType.GetAccountingStartDate().Year,
                mockedUser.UserId);

            #endregion

            controller.Bind(model);
            controller.StashModel(model);

            //ACT:
            var result = await controller.CheckData(model) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.AreEqual("SubmissionComplete", result.ActionName);

            // Clean up
            await testService.DiscardDraftFileAsync(model);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("Check Data Returns Session Expired When ViewModel Isn't Stashed")]
        public async Task SubmitController_CheckData_POST_When_UnStashed_View_Model_Then_Error_Session_Expired()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var address = new OrganisationAddress
            {
                OrganisationId = 1,
                Address1 = "Address line 1",
                PostCode = "PC1",
                Status = AddressStatuses.Active
            };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //mock return existing in the DB
            var @return = new Return { ReturnId = 1, OrganisationId = 1, CompanyLinkToGPGInfo = "http://www.test.com" };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "CheckData");
            routeData.Values.Add("Controller", "Submit");

            //mock entered 'return' at review CheckData view
            var model = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 30,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 30,
                MaleMedianBonusPayPercent = 40,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 70,
                OrganisationId = organisation.OrganisationId,
                ReturnId = 1
            };

            var controller = UiTestHelper.GetController<SubmitController>(
                1,
                routeData,
                user,
                address,
                organisation,
                userOrganisation,
                @return);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = SectorTypes.Private.GetAccountingStartDate().Year;

            controller.Bind(model);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.CheckData(model) as ViewResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result);
            Assert.AreEqual("Session has expired", ((ErrorViewModel)result.Model).Title);
        }


        [Test]
        [Description("Ensure the employer Website form is returned for the current user ")]
        public async Task SubmitController_EmployerWebsite_GET_Success()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            organisation.OrganisationId = new Random().Next(1000, 9999);
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = null;

            var model = new ReturnViewModel();

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            model.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller = UiTestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            //Stash an object to pass in for this.ClearStash()
            controller.StashModel(model);

            //ACT:
            var result = await controller.EmployerWebsite(returnurl) as ViewResult;
            var resultModel = result.Model as ReturnViewModel;

            //ASSERT:
            Assert.That(
                result != null && result.GetType() == typeof(ViewResult),
                "Expected an object other than null or Incorrect resultType returned");
            Assert.That(result.ViewName == "EmployerWebsite", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected a ReturnViewModel object, null object is returned");
            Assert.That(result.Model.GetType() == typeof(ReturnViewModel), "Incorrect resultType returned");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");
            Assert.That(
                result.ViewData.ModelState.IsValidField("CompanyLinkToGPGInfo"),
                "Expected CompanyLinkToGPGInfo value is malformed or incorrect format");
            Assert.Null(resultModel.CompanyLinkToGPGInfo, "CompanyLinkToGPGInfo:Expected a null  or empty field");

            // Clean up
            await testService.DiscardDraftFileAsync(model);
        }

        [Test]
        [Description("Ensure the Employer Website Returns Session Expired When View Model Isn't Stashed")]
        public async Task SubmitController_EmployerWebsite_GET_When_Model_Not_Stashed_Then_Returns_Session_Expired()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            string returnUrl = null;

            var model = new ReturnViewModel();

            var controller =
                UiTestHelper.GetController<SubmitController>(
                    user.UserId,
                    routeData,
                    user,
                    organisation,
                    userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            // ACT
            var result = await controller.EmployerWebsite(returnUrl) as ViewResult;

            Assert.NotNull(result);
            Assert.AreEqual("Session has expired", ((ErrorViewModel)result.Model).Title);
        }

        [Test]
        public async Task SubmitController_EmployerWebsite_GET_When_Not_Registered_User()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = null };
            var organisation = new Organisation { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            string returnUrl = null;

            var model = new ReturnViewModel();

            var controller =
                UiTestHelper.GetController<SubmitController>(
                    user.UserId,
                    routeData,
                    user,
                    organisation,
                    userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            controller.StashModel(model);

            // ACT
            var result = await controller.EmployerWebsite(returnUrl) as ViewResult;

            Assert.NotNull(result);

            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel)result.Model).Title);
        }

        [Test]
        [Description("Ensure the employer Website Excludes The Person Responsible Section When Sector Is Public")]
        public async Task SubmitController_EmployerWebsite_GET_When_Sector_Is_Public_Then_Exclude_Person_Responsible_Section()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Public };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            string returnUrl = null;

            var model = new ReturnViewModel { SectorType = SectorTypes.Public };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            model.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller =
                UiTestHelper.GetController<SubmitController>(
                    user.UserId,
                    routeData,
                    user,
                    organisation,
                    userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            controller.StashModel(model);

            // ACT
            var result = await controller.EmployerWebsite(returnUrl) as ViewResult;

            // Assert
            Assert.AreEqual(result.ViewName, "EmployerWebsite");

            // Clean up
            await testService.DiscardDraftFileAsync(model);
        }

        //I don't think this test is necessary as the above does the same thing this just does the same but in opposite
        [Test]
        [Description("Verify that a bad url link with the improper web protocol prefix is not validated or allowed")]
        public async Task SubmitController_EmployerWebsite_POST_VerifyGPGInfoLink_BadURL_Link()
        {
            //Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "0"
            };
            // var @return = new Return() { ReturnId = 1, OrganisationId = 1 };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);

            var model = new ReturnViewModel { CompanyLinkToGPGInfo = "http:www.//google.com" };

            //Act
            await controller.EmployerWebsite(model);

            //ASSERT
            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");
            Assert.That(
                controller.ViewData.ModelState.IsValidField("CompanyLinkToGPGInfo"),
                "value for CompanyLinkToGPGInfo is malformed or incorrect format");
        }

        [Test]
        [Description("Verify an existing GPGInfo Link is what is returned")]
        public async Task SubmitController_EmployerWebsite_POST_VerifyGPGInfoLink_WhatYouPutIn_IsWhatYouGetOut()
        {
            //ARRANGE:
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "0"
            };

            //mock return with CompanyLinkToGPGInfo in the DB
            var @return = new Return
            {
                ReturnId = 1,
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                CompanyLinkToGPGInfo = "http://www.test.com"
            };

            var routeData = new RouteData();
            routeData.Values.Add("action", "EmployerWebsite");
            routeData.Values.Add("controller", "submit");

            //mock entered return CompanyLinkToGPGInfo in the CompanyLinkToGPGInfo EmployerWebsite view
            var returnViewModel = new ReturnViewModel { CompanyLinkToGPGInfo = "http://www.test.com", ReportInfo = new ReportInfoModel() };

            //added into the mock DB via mockRepository
            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            controller.StashModel(returnViewModel);

            //ACT:
            var result = await controller.EmployerWebsite(returnViewModel) as RedirectToRouteResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //ASSERT:
            Assert.That(
                resultModel.CompanyLinkToGPGInfo == returnViewModel.CompanyLinkToGPGInfo,
                "CompanyLinkToGPGInfoLink that was input by the user is not what is returned");

            //TODO not really a valid test as there is no code which changes this - you should maybe just be checking there are no modelstate errors but then its a repeat test of one you did earlier
            //TODO also your not checking for the correct redirectresult and the rest of the model the correct model - why just test one field remains unchanged?

            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("When View Model Isn't stashed then returns error")]
        public async Task SubmitController_EmployerWebsite_POST_When_ViewModel_Not_Stashed_Then_Returns_Session_Has_Expired()
        {
            //ARRANGE:
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "0"
            };

            //mock return with CompanyLinkToGPGInfo in the DB
            var @return = new Return
            {
                ReturnId = 1,
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                CompanyLinkToGPGInfo = "http://www.test.com"
            };

            var routeData = new RouteData();
            routeData.Values.Add("action", "EmployerWebsite");
            routeData.Values.Add("controller", "submit");

            //mock entered return CompanyLinkToGPGInfo in the CompanyLinkToGPGInfo EmployerWebsite view
            var model = new ReturnViewModel { CompanyLinkToGPGInfo = "http://www.test.com" };

            //added into the mock DB via mockRepository
            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            //ACT:
            var result = await controller.EmployerWebsite(model) as ViewResult;
            Assert.NotNull(result);
            Assert.AreEqual("Session has expired", ((ErrorViewModel)result.Model).Title);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("When View Model Isn't valid then returns error")]
        public async Task SubmitController_EmployerWebsite_POST_When_ViewModel_Not_Valid_Then_Return_Error()
        {
            //ARRANGE:
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "0"
            };

            //mock return with CompanyLinkToGPGInfo in the DB
            var @return = new Return
            {
                ReturnId = 1,
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                CompanyLinkToGPGInfo = "http://www.test.com"
            };

            var routeData = new RouteData();
            routeData.Values.Add("action", "EmployerWebsite");
            routeData.Values.Add("controller", "submit");

            //mock entered return CompanyLinkToGPGInfo in the CompanyLinkToGPGInfo EmployerWebsite view
            var returnViewModel = new ReturnViewModel
            {
                CompanyLinkToGPGInfo = "httpx://www.google.com",
                FirstName = string.Empty,
                ReportInfo = new ReportInfoModel()
            };


            //added into the mock DB via mockRepository
            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            controller.ModelState.AddModelError("CompanyLinkToGPGInfo", "Error generated manually from the Test");

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            controller.StashModel(returnViewModel);

            //ACT:
            var result = await controller.EmployerWebsite(returnViewModel) as ViewResult;

            controller.StashModel(returnViewModel);
            //var firstNameModelState = controller.ModelState.Values.Select(e => e.Errors).FirstOrDefault()?[0].ErrorMessage; 

            Assert.NotNull(result);
            Assert.AreEqual(
                "Error generated manually from the Test",
                controller.ModelState.Values.Select(e => e.Errors).FirstOrDefault()?[0].ErrorMessage);

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test]
        [Description("Ensure that employer Website form is filled and sent successfully when its field value is a valid url value")]
        public async Task SubmitController_EmployerWebsite_POST_With_CompanyLinkToGPGInfoValue_Success()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            var PrivateAccountingDate = new DateTime(2017, 4, 4);

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 10,
                DiffMedianBonusPercent = 10,
                DiffMedianHourlyPercent = 10,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 10,
                FemaleMiddlePayBand = 10,
                FemaleUpperPayBand = 10,
                FemaleUpperQuartilePayBand = 10,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 10,
                MaleMedianBonusPayPercent = 10,
                MaleMiddlePayBand = 10,
                MaleUpperPayBand = 10,
                MaleUpperQuartilePayBand = 10,
                OrganisationId = organisation.OrganisationId,
                ReturnId = 10,
                ReportInfo = new ReportInfoModel()
            };

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            controller.Bind(returnViewModel);

            #endregion

            controller.StashModel(returnViewModel);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.EmployerWebsite(returnViewModel) as RedirectToActionResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.AreEqual("CheckData", result.ActionName, "Expected a RedirectToActionResult to CheckData");

            //DONE:Since it was stashed no need to check the fields as it is exactlywhat it was going in before stashing it, Hence ony check that the model is unstashed
            Assert.NotNull(resultModel, "unstashed model is Invalid");

            //DONE you should be checking modelstate.isvalid and each modelstate error
            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");
            Assert.That(
                controller.ViewData.ModelState.IsValidField("CompanyLinkToGPGInfo"),
                "value for CompanyLinkToGPGInfo is malformed or incorrect format");
            Assert.That(resultModel.CompanyLinkToGPGInfo.StartsWith("http://"), "Expected CompanyLinkToGPGInfoLink URL Prefix:'http://' ");

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test]
        [Description("Ensure that employer Website form is filled and sent successfully when there is no value as it is optional")]
        public async Task SubmitController_EmployerWebsite_POST_Without_CompanyLinkToGPGInfoValue_Success()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerWebsite");
            routeData.Values.Add("Controller", "Submit");

            var PrivateAccountingDate = new DateTime(2017, 4, 4);

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = 0,
                DiffMeanHourlyPayPercent = 0,
                DiffMedianBonusPercent = 0,
                DiffMedianHourlyPercent = 0,
                FemaleLowerPayBand = 0,
                FemaleMedianBonusPayPercent = 0,
                FemaleMiddlePayBand = 0,
                FemaleUpperPayBand = 0,
                FemaleUpperQuartilePayBand = 0,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 0,
                MaleMedianBonusPayPercent = 0,
                MaleMiddlePayBand = 0,
                MaleUpperPayBand = 0,
                MaleUpperQuartilePayBand = 0,
                OrganisationId = organisation.OrganisationId,
                ReturnId = 0,
                ReportInfo = new ReportInfoModel()
            };

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            controller.Bind(returnViewModel);
            controller.StashModel(returnViewModel);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.EmployerWebsite(returnViewModel) as RedirectToActionResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == "CheckData", "Expected a RedirectToActionResult to CheckData");

            // See if there are anymore asserts that can be done for a redirect here.
            //TODO you should be checking modelstate.isvalid and also that all other fields dont fail in modelstate

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test]
        [Description("Ensure the EnterCalculations form is returned for the current user ")]
        public async Task SubmitController_EnterCalculations_GET_Success()
        {
            ///TODO: create a real mock of the following (user, organisation and userOrganisation) 
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = new Random().Next(1000, 9999), SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            string returnUrl = null;

            var returnViewModel = new ReturnViewModel
            {
                ReportInfo = new ReportInfoModel
                {
                    Draft = new Draft(organisation.OrganisationId, organisation.SectorType.GetAccountingStartDate().Year)
                    {
                        IsUserAllowedAccess = true
                    }
                }
            };

            var controller = UiTestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            controller.StashModel(returnViewModel);

            // Act
            var result = await controller.EnterCalculations(returnUrl) as ViewResult;
            var resultModel = result.Model as ReturnViewModel;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result != null && result is ViewResult, "Expected viewResult  or incorrect resultType returned");
            Assert.That(
                result.ViewName == "EnterCalculations",
                $"Incorrect view returned, expected 'EnterCalculations' but was '{result.ViewName}'");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            Assert.Null(resultModel.DiffMeanBonusPercent, "DiffMeanBonusPercent:Expected a null or empty field");
            Assert.Null(resultModel.DiffMeanHourlyPayPercent, "DiffMeanHourlyPayPercent:Expected a null  or empty field");
            Assert.Null(resultModel.DiffMedianBonusPercent, "DiffMedianBonusPercent:Expected a null  or empty field");
            Assert.Null(resultModel.DiffMedianHourlyPercent, "DiffMedianHourlyPercent:Expected a null  or empty field");
            Assert.Null(resultModel.FemaleLowerPayBand, "FemaleLowerPayBand:Expected a null  or empty field");
            Assert.Null(resultModel.FemaleMedianBonusPayPercent, "FemaleMedianBonusPayPercent:Expected a null  or empty field");
            Assert.Null(resultModel.FemaleMiddlePayBand, "FemaleMiddlePayBand:Expected a null  or empty field");
            Assert.Null(resultModel.FemaleUpperPayBand, "FemaleUpperPayBand:Expected a null  or empty field");
            Assert.Null(resultModel.FemaleUpperQuartilePayBand, "FemaleUpperQuartilePayBand:Expected a null  or empty field");
            Assert.Null(resultModel.MaleLowerPayBand, "MaleLowerPayBand:Expected a null  or empty field");
            Assert.Null(resultModel.MaleMedianBonusPayPercent, "MaleMedianBonusPayPercent:Expected a null  or empty field");
            Assert.Null(resultModel.MaleMiddlePayBand, "MaleMiddlePayBand:Expected a null  or empty field");
            Assert.Null(resultModel.MaleUpperPayBand, "MaleUpperPayBand:Expected a null  or empty field");
            Assert.Null(resultModel.MaleUpperQuartilePayBand, "MaleUpperQuartilePayBand:Expected a null  or empty field");
        }
        
        [Test]
        [Description("Ensure the EnterCalculations form returns an existing return if there is one for the current user ")]
        public async Task SubmitController_EnterCalculations_GET_VerifyActionReturns_AnExistingReturn()
        {
            ///TODO: create a real mock of the following (user, organisation and userOrganisation)

            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "0"
            };

            var @return = new Return
            {
                ReturnId = 1,
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                CompanyLinkToGPGInfo = "https://www.test.com"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Register");

            string returnurl = null;

            //Stash an object to unStash()
            var model = new ReturnViewModel();

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);

            controller.StashModel(model);

            //ACT:
            var result = await controller.EnterCalculations(returnurl) as RedirectToActionResult;

            //Assert
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.AreEqual(result.ControllerName, "Organisation", "Expected action");
            Assert.AreEqual(result.ActionName, "ManageOrganisations", "Expected action");

            //TODO you arent checking the returned model at all
            //TODO again you should be checking the returned model has correct values and is for the correct user and org and userorg
        }

        [Test]
        [Description(
            "Ensure the Enter Calculations form returns an existing return if there is one and the loaded model of the return is valid")]
        public async Task SubmitController_EnterCalculations_GET_VerifyActionReturns_ValidReturnModel()
        {
            ///TODO: create a real mock of the following (user, organisation and userOrganisation) 
            //ARRANGE:
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "0"
            };

            //return in the db
            var @return = new Return
            {
                ReturnId = 1,
                OrganisationId = 1,
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 10,
                DiffMedianBonusPercent = 10,
                DiffMedianHourlyPercent = 10,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 10,
                FemaleMiddlePayBand = 10,
                FemaleUpperPayBand = 10,
                FemaleUpperQuartilePayBand = 10,
                MaleLowerPayBand = 10,
                MaleMedianBonusPayPercent = 10,
                MaleMiddlePayBand = 10,
                MaleUpperPayBand = 10,
                MaleUpperQuartilePayBand = 10,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Dev",
                CompanyLinkToGPGInfo = "http:www.geo.gov.uk"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation, @return);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            var model = new ReturnViewModel
            {
                ReportInfo = new ReportInfoModel
                {
                    Draft = new Draft(organisation.OrganisationId, organisation.SectorType.GetAccountingStartDate().Year)
                    {
                        IsUserAllowedAccess = true
                    }
                }
            };

            controller.StashModel(model);

            //ACT:
            var result = await controller.EnterCalculations() as ViewResult;
            var resultModel = result.Model as ReturnViewModel;

            //ASSERT:
            Assert.That(
                result != null && result is ViewResult,
                "Expected returned ViewResult object not to be null  or incorrect resultType returned");
            Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");
            Assert.IsNotNull(resultModel, "Expected returned ReturnViewModel object not to be null");
            Assert.That(resultModel is ReturnViewModel, "Expected Model to be of type ReturnViewModel");
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            Assert.NotNull(
                resultModel.DiffMeanBonusPercent == @return.DiffMeanBonusPercent,
                "DiffMeanBonusPercent:Expected a null or empty field");
            Assert.NotNull(
                resultModel.DiffMeanHourlyPayPercent == @return.DiffMeanHourlyPayPercent,
                "DiffMeanHourlyPayPercent:Expected a null  or empty field");
            Assert.NotNull(
                resultModel.DiffMedianBonusPercent == @return.DiffMedianBonusPercent,
                "DiffMedianBonusPercent:Expected a null  or empty field");
            Assert.NotNull(
                resultModel.DiffMedianHourlyPercent == @return.DiffMedianHourlyPercent,
                "DiffMedianHourlyPercent:Expected a null  or empty field");
            Assert.NotNull(
                resultModel.FemaleLowerPayBand == @return.FemaleLowerPayBand,
                "FemaleLowerPayBand:Expected a null  or empty field");
            Assert.NotNull(
                resultModel.FemaleMedianBonusPayPercent == @return.FemaleMedianBonusPayPercent,
                "FemaleMedianBonusPayPercent:Expected a null  or empty field");
            Assert.NotNull(
                resultModel.FemaleMiddlePayBand == @return.FemaleMiddlePayBand,
                "FemaleMiddlePayBand:Expected a null  or empty field");
            Assert.NotNull(
                resultModel.FemaleUpperPayBand == @return.FemaleUpperPayBand,
                "FemaleUpperPayBand:Expected a null  or empty field");
            Assert.NotNull(
                resultModel.FemaleUpperQuartilePayBand == @return.FemaleUpperQuartilePayBand,
                "FemaleUpperQuartilePayBand:Expected a null  or empty field");
            Assert.NotNull(resultModel.MaleLowerPayBand == @return.MaleLowerPayBand, "MaleLowerPayBand:Expected a null  or empty field");
            Assert.NotNull(
                resultModel.MaleMedianBonusPayPercent == @return.MaleMedianBonusPayPercent,
                "MaleMedianBonusPayPercent:Expected a null  or empty field");
            Assert.NotNull(resultModel.MaleMiddlePayBand == @return.MaleMiddlePayBand, "MaleMiddlePayBand:Expected a null  or empty field");
            Assert.NotNull(resultModel.MaleUpperPayBand == @return.MaleUpperPayBand, "MaleUpperPayBand:Expected a null  or empty field");
            Assert.NotNull(
                resultModel.MaleUpperQuartilePayBand == @return.MaleUpperQuartilePayBand,
                "MaleUpperQuartilePayBand:Expected a null  or empty field");

            Assert.NotNull(resultModel.FirstName == @return.FirstName, "FirstName:Expected a null  or empty field");
            Assert.NotNull(resultModel.LastName == @return.LastName, "LastName:Expected a null  or empty field");
            Assert.NotNull(resultModel.JobTitle == @return.JobTitle, "JobTitle:Expected a null  or empty field");

            Assert.NotNull(
                resultModel.CompanyLinkToGPGInfo == @return.CompanyLinkToGPGInfo,
                "CompanyLinkToGPGInfo:Expected a null  or empty field");
        }

        [Test(Author = "Oscar Lagatta")]
        [Description(
            "If a user in the public sector does not have a return existing in the database, a new one should be created and verified with default values")]
        public async Task SubmitController_EnterCalculations_GET_Wen_User_Has_No_Return_Then_A_New_Public_Sector_Return_Should_Be_Created()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            mockedOrganisation.OrganisationId = new Random().Next(1000, 9999);
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetNewReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            Return clonedReturn = mockedReturn.GetClone();

            ReturnViewModel expectedReturnViewModel =
                new SubmissionBusinessLogic(null).ConvertSubmissionReportToReturnViewModel(clonedReturn);

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            expectedReturnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                mockedOrganisation.OrganisationId,
                mockedOrganisation.SectorType.GetAccountingStartDate().Year,
                mockedUser.UserId);

            #endregion

            controller.StashModel(expectedReturnViewModel);

            //Act:
            var result = await controller.EnterCalculations() as ViewResult;
            Assert.NotNull(result);

            var actualReturnViewModel = result.Model as ReturnViewModel;
            Assert.NotNull(actualReturnViewModel);

            expectedReturnViewModel.Compare(actualReturnViewModel, null, null, false, true, -1, false);

            // Clean up
            await testService.DiscardDraftFileAsync(expectedReturnViewModel);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("If a user has a return existing in the database, then the return can be retrieved")]
        public async Task SubmitController_EnterCalculations_GET_When_User_Has_Return_In_The_Database_Then_The_Return_Can_Be_Obtained()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            mockedOrganisation.OrganisationId = new Random().Next(1000, 9999);
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            Return clonedReturn = mockedReturn.GetClone();
            ReturnViewModel expectedReturnViewModel =
                new SubmissionBusinessLogic(null).ConvertSubmissionReportToReturnViewModel(clonedReturn);

            controller.ClearStash(); // empty, so it'll search for info on DB

            // Act:
            var result = await controller.EnterCalculations() as ViewResult;
            Assert.NotNull(result, "Expected a ViewResult");

            var actualReturnViewModel = result.Model as ReturnViewModel;
            Assert.NotNull(actualReturnViewModel, "Expected a ReturnViewModel");

            expectedReturnViewModel.Compare(
                actualReturnViewModel,
                new[] { "Report", "ShouldProvideLateReason", "LatestOrganisationName", "IsInScopeForThisReportYear" },
                null,
                false,
                true,
                -1,
                false);

            // Clean up
            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);
            await testService.DiscardDraftFileAsync(actualReturnViewModel);
        }

        [Test]
        public async Task SubmitController_EnterCalculations_POST_CancelEnterCalculations_When_Nothing_Stashed_Returns_Custom_Error()
        {
            // Arrange
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, null, null);
            controller.ClearAllStashes(); // confirm nothing is stashed

            // Act
            var result = await controller.CancelEnterCalculations(new ReturnViewModel()) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("CustomError", result.ViewName);

            var model = result.Model as ErrorViewModel;
            Assert.NotNull(model);
            Assert.AreEqual(1134, model.ErrorCode);
            Assert.AreEqual("Your session has expired.", model.Description);
        }
        
        [Test]
        [Description("EnterCalculations should fail when female bonus is greater than 0 be mean or median bonus is greater than 100%")]
        public async Task SubmitController_EnterCalculations_POST_FemaleBonusSetCannotExceed100percent_ShowsErrors()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Public };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            DateTime PrivateAccountingDate = Global.PrivateAccountingDate;

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = PrivateAccountingDate,
                MaleMedianBonusPayPercent = 66,
                FemaleMedianBonusPayPercent = 33,
                DiffMeanBonusPercent = 500,
                DiffMedianBonusPercent = 101,
                DiffMedianHourlyPercent = 50,
                DiffMeanHourlyPayPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                MaleLowerPayBand = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                SectorType = SectorTypes.Private
            };

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(returnViewModel);

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var submissionServiceMock = new SubmissionService(null, null, new DraftFileBusinessLogic(testDataRepository));
            returnViewModel.ReportInfo.Draft = await submissionServiceMock.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            // Act
            controller.StashModel(returnViewModel);
            controller.ReportingOrganisationId = 1;
            var result = await controller.EnterCalculations(returnViewModel) as ViewResult;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.AreEqual(
                "Please enter a percentage lower than or equal to 100",
                result.ViewData.ModelState["DiffMedianBonusPercent"].Errors[0].ErrorMessage);
            Assert.AreEqual(
                "Please enter a percentage lower than or equal to 100",
                result.ViewData.ModelState["DiffMeanBonusPercent"].Errors[0].ErrorMessage);
        }

        [Test]
        [Description("EnterCalculations should fail when male bonus is zero but provided mean or median bonus difference")]
        public async Task SubmitController_EnterCalculations_POST_MaleBonusIsZero_ShowsErrors()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Public };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            DateTime PrivateAccountingDate = Global.PrivateAccountingDate;

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = PrivateAccountingDate,
                MaleMedianBonusPayPercent = 0,
                FemaleMedianBonusPayPercent = 50,
                DiffMeanBonusPercent = -50,
                DiffMedianBonusPercent = -50,
                DiffMedianHourlyPercent = 50,
                DiffMeanHourlyPayPercent = 50,
                FemaleLowerPayBand = 50,
                FemaleMiddlePayBand = 50,
                FemaleUpperPayBand = 50,
                FemaleUpperQuartilePayBand = 50,
                MaleLowerPayBand = 50,
                MaleMiddlePayBand = 50,
                MaleUpperPayBand = 50,
                MaleUpperQuartilePayBand = 50,
                SectorType = SectorTypes.Private
            };

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.Bind(returnViewModel);

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var submissionServiceMock = new SubmissionService(null, null, new DraftFileBusinessLogic(testDataRepository));
            returnViewModel.ReportInfo.Draft = await submissionServiceMock.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            // Act
            controller.StashModel(returnViewModel);
            controller.ReportingOrganisationId = 1;
            var result = await controller.EnterCalculations(returnViewModel) as ViewResult;

            // Assert
            Assert.NotNull(result, "Expected ViewResult");
            Assert.That(result.ViewName == "EnterCalculations", "Incorrect view returned");
            Assert.NotNull(result.Model as ReturnViewModel, "Expected ReturnViewModel");
            Assert.AreEqual(
                "Do not enter a bonus difference if 0% of your male employees received a bonus",
                result.ViewData.ModelState["DiffMedianBonusPercent"].Errors[0].ErrorMessage);
            Assert.AreEqual(
                "Do not enter a bonus difference if 0% of your male employees received a bonus",
                result.ViewData.ModelState["DiffMeanBonusPercent"].Errors[0].ErrorMessage);
        }

        [Test]
        [Description("EnterCalculations should fail when any field is outside of the maximum allowed range of valid values")]
        public async Task SubmitController_EnterCalculations_POST_MaxValidValues_NoErrors()
        {
            ///TODO: create a real mock of the following (user, organisation and userOrganisation) 
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var returnurl = "CheckData";
            DateTime PrivateAccountingDate = Global.PrivateAccountingDate;
            var maxValidValue = 99.9M;
            decimal maleEquiValue = 50;
            decimal femaleEquiValue = 50;

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = PrivateAccountingDate,
                DiffMeanBonusPercent = maxValidValue,
                DiffMeanHourlyPayPercent = maxValidValue,
                DiffMedianBonusPercent = maxValidValue,
                DiffMedianHourlyPercent = maxValidValue,
                FemaleLowerPayBand = femaleEquiValue,
                FemaleMedianBonusPayPercent = maxValidValue,
                FemaleMiddlePayBand = femaleEquiValue,
                FemaleUpperPayBand = femaleEquiValue,
                FemaleUpperQuartilePayBand = femaleEquiValue,
                MaleLowerPayBand = maleEquiValue,
                MaleMedianBonusPayPercent = maxValidValue,
                MaleMiddlePayBand = maleEquiValue,
                MaleUpperPayBand = maleEquiValue,
                MaleUpperQuartilePayBand = maleEquiValue,
                SectorType = SectorTypes.Private,
                ReportInfo = new ReportInfoModel()
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = SectorTypes.Private.GetAccountingStartDate().Year;
            controller.ReportingOrganisation.SectorType = SectorTypes.Private;
            controller.Bind(returnViewModel);

            controller.StashModel(returnViewModel);

            //Act
            var result = await controller.EnterCalculations(returnViewModel, returnurl) as RedirectToActionResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            // Assert
            //DONE:Since it was stashed no need to check the fields as it is exactly what it was going in before stashing it, Hence ony check that the model is unstashed
            Assert.NotNull(resultModel, "Unstashed model is Invalid Expected ReturnViewModel");
            Assert.AreEqual("CheckData", result.ActionName, "Expected a RedirectToRouteResult to CheckData");

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test]
        [Description("EnterCalculations should fail when any field is outside of the minimum allowed range of valid values")]
        public async Task SubmitController_EnterCalculations_POST_MinValidValues_NoErrors()
        {
            ///TODO: create a real mock of the following (user, organisation and userOrganisation) 
            //ARRANGE:
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var returnurl = "CheckData";
            DateTime PrivateAccountingDate = Global.PrivateAccountingDate;
            var minValidValue = 0M; //-200.9M;
            decimal maleEquiValue = 50;
            decimal femaleEquiValue = 50;

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = PrivateAccountingDate,
                MaleMedianBonusPayPercent = maleEquiValue,
                FemaleMedianBonusPayPercent = femaleEquiValue,
                DiffMeanBonusPercent = minValidValue,
                DiffMeanHourlyPayPercent = minValidValue,
                DiffMedianBonusPercent = minValidValue,
                DiffMedianHourlyPercent = minValidValue,
                FemaleLowerPayBand = femaleEquiValue,
                FemaleMiddlePayBand = femaleEquiValue,
                FemaleUpperPayBand = femaleEquiValue,
                FemaleUpperQuartilePayBand = femaleEquiValue,
                MaleLowerPayBand = maleEquiValue,
                MaleMiddlePayBand = maleEquiValue,
                MaleUpperPayBand = maleEquiValue,
                MaleUpperQuartilePayBand = maleEquiValue,
                SectorType = SectorTypes.Private,
                ReportInfo = new ReportInfoModel()
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = SectorTypes.Private.GetAccountingStartDate().Year;
            controller.ReportingOrganisation.SectorType = SectorTypes.Private;
            controller.Bind(returnViewModel);

            controller.StashModel(returnViewModel);

            //ACT:
            var result = await controller.EnterCalculations(returnViewModel, returnurl) as RedirectToActionResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //ASSERT:
            //DONE:Since it was stashed no need to check the fields as it is exactly what it was going in before stashing it, Hence ony check that the model is unstashed
            Assert.NotNull(resultModel, "Unstashed model is Invalid Expected ReturnViewModel");
            Assert.AreEqual("CheckData", result.ActionName, "Expected a RedirectToRouteResult to CheckData");

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test]
        [Description("EnterCalculations should fail when any field is empty")]
        public async Task SubmitController_EnterCalculations_POST_Success_PrivateSector()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            var returnurl = "";

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,

                // website
                CompanyLinkToGPGInfo = null,

                // bonus
                MaleMedianBonusPayPercent = 0,
                FemaleMedianBonusPayPercent = 0,
                //DiffMeanBonusPercent = 0.0M, Can be null
                //DiffMedianBonusPercent = 0, Can be null

                // hourly rate
                DiffMeanHourlyPayPercent = 0,
                DiffMedianHourlyPercent = 0,

                // quartile
                FemaleLowerPayBand = 10,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 60,
                FemaleUpperQuartilePayBand = 80,
                MaleLowerPayBand = 90,
                MaleMiddlePayBand = 70,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 20,

                // person responsible
                FirstName = null,
                LastName = null,
                JobTitle = null,

                // size
                OrganisationSize = OrganisationSizes.NotProvided,
                OrganisationId = organisation.OrganisationId,
                SectorType = SectorTypes.Private,
                ReturnId = 0,
                ReportInfo = new ReportInfoModel()
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            //TODO line above is wrong as you should be setting the fields to null not zero

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = SectorTypes.Private.GetAccountingStartDate().Year;
            controller.ReportingOrganisation.SectorType = SectorTypes.Private;
            controller.Bind(returnViewModel);

            controller.StashModel(returnViewModel);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.EnterCalculations(returnViewModel, returnurl) as RedirectToActionResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();


            //TODO this test is completely wrong you should be cheking the all the fields are invalid in the modelstate

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");

            //4.Check that the redirection went to the right url step.
            Assert.AreEqual("PersonResponsible", result.ActionName, "Expected a RedirectToRouteResult to PersonResponsible");

            // See if there are anymore asserts that can be done for a redirect here.

            Assert.Multiple(
                () =>
                {
                    Assert.NotNull(resultModel is ReturnViewModel, "Expected ReturnViewModel");

                    Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");

                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"),
                        "Expected DiffMeanBonusPercent failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"),
                        "Expected DiffMeanHourlyPayPercent failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"),
                        "Expected DiffMedianBonusPercent failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"),
                        "Expected DiffMedianHourlyPercent failure");

                    Assert.That(controller.ViewData.ModelState.IsValidField("FemaleLowerPayBand"), "Expected FemaleLowerPayBand failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"),
                        "Expected FemaleMedianBonusPayPercent failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"),
                        "Expected FemaleMiddlePayBand  failure");
                    Assert.That(controller.ViewData.ModelState.IsValidField("FemaleUpperPayBand"), "Expected FemaleUpperPayBand  failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"),
                        "Expected FemaleUpperQuartilePayBand  failure");

                    Assert.That(controller.ViewData.ModelState.IsValidField("MaleLowerPayBand"), "Expected MaleLowerPayBand  failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"),
                        "Expected MaleMedianBonusPayPercent  failure");
                    Assert.That(controller.ViewData.ModelState.IsValidField("MaleMiddlePayBand"), "Expected MaleMiddlePayBand  failure");
                    Assert.That(controller.ViewData.ModelState.IsValidField("MaleUpperPayBand"), "Expected MaleUpperPayBand  failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"),
                        "Expected MaleUpperQuartilePayBand  failure");
                });

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test]
        [Description("EnterCalculations should fail when any field is empty")]
        public async Task SubmitController_EnterCalculations_POST_Success_PublicSector()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Public };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EnterCalculations");
            routeData.Values.Add("Controller", "Submit");

            var returnurl = "";

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = null,
                DiffMeanBonusPercent = null,
                DiffMeanHourlyPayPercent = 0,
                DiffMedianBonusPercent = null,
                DiffMedianHourlyPercent = 0,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 0,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 60,
                FemaleUpperQuartilePayBand = 80,
                FirstName = null,
                LastName = null,
                JobTitle = null,
                MaleLowerPayBand = 90,
                MaleMedianBonusPayPercent = 0,
                MaleMiddlePayBand = 70,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 20,
                OrganisationId = organisation.OrganisationId,
                SectorType = SectorTypes.Public,
                ReturnId = 0,
                OrganisationSize = OrganisationSizes.Employees0To249,
                ReportInfo = new ReportInfoModel()
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            //TODO line above is wrong as you should be setting the fields to null not zero

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = SectorTypes.Private.GetAccountingStartDate().Year;
            controller.ReportingOrganisation.SectorType = SectorTypes.Private;
            controller.Bind(returnViewModel);

            controller.StashModel(returnViewModel);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.EnterCalculations(returnViewModel, returnurl) as RedirectToActionResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToRouteResult");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == "OrganisationSize", "Expected a RedirectToRouteResult to OrganisationSize");

            //TODO This line is wrong as we should be returning the same view since model state was invalid

            // See if there are anymore asserts that can be done for a redirect here.

            Assert.Multiple(
                () =>
                {
                    Assert.NotNull(resultModel is ReturnViewModel, "Expected ReturnViewModel");

                    Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");

                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("DiffMeanBonusPercent"),
                        "Expected DiffMeanBonusPercent failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("DiffMeanHourlyPayPercent"),
                        "Expected DiffMeanHourlyPayPercent failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("DiffMedianBonusPercent"),
                        "Expected DiffMedianBonusPercent failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("DiffMedianHourlyPercent"),
                        "Expected DiffMedianHourlyPercent failure");

                    Assert.That(controller.ViewData.ModelState.IsValidField("FemaleLowerPayBand"), "Expected FemaleLowerPayBand failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("FemaleMedianBonusPayPercent"),
                        "Expected FemaleMedianBonusPayPercent failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("FemaleMiddlePayBand"),
                        "Expected FemaleMiddlePayBand  failure");
                    Assert.That(controller.ViewData.ModelState.IsValidField("FemaleUpperPayBand"), "Expected FemaleUpperPayBand  failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("FemaleUpperQuartilePayBand"),
                        "Expected FemaleUpperQuartilePayBand  failure");

                    Assert.That(controller.ViewData.ModelState.IsValidField("MaleLowerPayBand"), "Expected MaleLowerPayBand  failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("MaleMedianBonusPayPercent"),
                        "Expected MaleMedianBonusPayPercent  failure");
                    Assert.That(controller.ViewData.ModelState.IsValidField("MaleMiddlePayBand"), "Expected MaleMiddlePayBand  failure");
                    Assert.That(controller.ViewData.ModelState.IsValidField("MaleUpperPayBand"), "Expected MaleUpperPayBand  failure");
                    Assert.That(
                        controller.ViewData.ModelState.IsValidField("MaleUpperQuartilePayBand"),
                        "Expected MaleUpperQuartilePayBand  failure");
                });

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test]
        [Description("EnterCalculations should succeed when all fields have valid values")]
        public async Task SubmitController_EnterCalculations_POST_ValidValueInFields_NoErrors()
        {
            ///TODO: create a real mock of the following (user, organisation and userOrganisation) 
            //ARRANGE:
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var returnurl = "CheckData";
            DateTime PrivateAccountingDate = Global.PrivateAccountingDate;
            var validValue1 = 100M;
            var validValue2 = 95M;

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = PrivateAccountingDate,
                DiffMeanBonusPercent = validValue1,
                DiffMedianBonusPercent = validValue1,
                DiffMeanHourlyPayPercent = validValue2,
                DiffMedianHourlyPercent = validValue2,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = validValue1,
                FemaleMiddlePayBand = 60,
                FemaleUpperPayBand = 70,
                FemaleUpperQuartilePayBand = 50,
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = validValue1,
                MaleMiddlePayBand = 40,
                MaleUpperPayBand = 30,
                MaleUpperQuartilePayBand = 50,
                SectorType = SectorTypes.Private,
                ReportInfo = new ReportInfoModel()
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = SectorTypes.Private.GetAccountingStartDate().Year;
            controller.ReportingOrganisation.SectorType = SectorTypes.Private;
            controller.Bind(returnViewModel);

            controller.StashModel(returnViewModel);

            //ACT:
            var result = await controller.EnterCalculations(returnViewModel, returnurl) as RedirectToActionResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            //ASSERT:
            //DONE:Since it was stashed no need to check the fields as it is exactly what it was going in before stashing it, Hence ony check that the model is unstashed
            Assert.NotNull(resultModel, "Unstashed model is Invalid Expected ReturnViewModel");
            Assert.That(result.ActionName == "CheckData", "Expected a RedirectToActionResult to CheckData");

            Assert.NotNull(result, "Expected RedirectResult");
            var x = controller.ViewData.Model as ReturnViewModel;

            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");

            Assert.Multiple(
                () =>
                {
                    Assert.NotNull(result, "Expected ViewResult");
                    Assert.That(resultModel.AccountingDate == returnViewModel.AccountingDate, "Input value does not match model");
                    Assert.That(resultModel.Address == returnViewModel.Address, "Input value does not match model");
                    Assert.That(
                        resultModel.CompanyLinkToGPGInfo == returnViewModel.CompanyLinkToGPGInfo,
                        "Input value does not match model");
                    Assert.That(
                        resultModel.DiffMeanBonusPercent == returnViewModel.DiffMeanBonusPercent,
                        "Input value does not match model");
                    Assert.That(
                        resultModel.DiffMeanHourlyPayPercent == returnViewModel.DiffMeanHourlyPayPercent,
                        "Input value does not match model");
                    Assert.That(
                        resultModel.DiffMedianBonusPercent == returnViewModel.DiffMedianBonusPercent,
                        "Input value does not match model");
                    Assert.That(
                        resultModel.DiffMedianHourlyPercent == returnViewModel.DiffMedianHourlyPercent,
                        "Input value does not match model");
                    Assert.That(resultModel.FemaleLowerPayBand == returnViewModel.FemaleLowerPayBand, "Input value does not match model");
                    Assert.That(
                        resultModel.FemaleMedianBonusPayPercent == returnViewModel.FemaleMedianBonusPayPercent,
                        "Input value does not match model");
                    Assert.That(resultModel.FemaleMiddlePayBand == returnViewModel.FemaleMiddlePayBand, "Input value does not match model");
                    Assert.That(resultModel.FemaleUpperPayBand == returnViewModel.FemaleUpperPayBand, "Input value does not match model");
                    Assert.That(
                        resultModel.FemaleUpperQuartilePayBand == returnViewModel.FemaleUpperQuartilePayBand,
                        "Input value does not match model");
                    Assert.That(resultModel.MaleLowerPayBand == returnViewModel.MaleLowerPayBand, "Input value does not match model");
                    Assert.That(resultModel.MaleMiddlePayBand == returnViewModel.MaleMiddlePayBand, "Input value does not match model");

                    Assert.That(resultModel.MaleUpperPayBand == returnViewModel.MaleUpperPayBand, "Input value does not match model");
                    Assert.That(
                        resultModel.MaleUpperQuartilePayBand == returnViewModel.MaleUpperQuartilePayBand,
                        "Input value does not match model");
                    Assert.That(resultModel.OrganisationId == returnViewModel.OrganisationId, "Input value does not match model");
                    Assert.That(resultModel.OrganisationName == returnViewModel.OrganisationName, "Input value does not match model");
                    Assert.That(resultModel.ReturnId == returnViewModel.ReturnId, "Input value does not match model");
                    Assert.That(resultModel.ReturnUrl == returnViewModel.ReturnUrl, "Input value does not match model");
                    Assert.That(resultModel.Sector == returnViewModel.Sector, "Input value does not match model");
                    Assert.That(resultModel.SectorType == returnViewModel.SectorType, "Input value does not match model");
                    Assert.That(resultModel.FirstName == returnViewModel.FirstName, "Input value does not match model");
                    Assert.That(resultModel.JobTitle == returnViewModel.JobTitle, "Input value does not match model");
                    Assert.That(resultModel.LastName == returnViewModel.LastName, "Input value does not match model");
                });

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("Submit Controller returns Custom Error Session has expired")]
        public async Task SubmitController_EnterCalculations_POST_When_No_Stashed_Return_ViewModel()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            //Act:
            var result = await controller.EnterCalculations(new ReturnViewModel()) as ViewResult;

            Assert.NotNull(result);
            Assert.AreEqual("Session has expired", ((ErrorViewModel)result.Model).Title);
        }
        
        [Test(Author = "Oscar Lagatta")]
        [Description("Submit Controller returns Custom Error When User Needs to Verify Email")]
        public async Task SubmitController_EnterCalculations_POST_When_User_Not_Registered_Then_Return_Need_Verify_Email_Message()
        {
            // ARRANGE
            User mockedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);


            // ACT
            var result = await controller.EnterCalculations(new ReturnViewModel()) as ViewResult;

            // ASSERT
            Assert.NotNull(result);
            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel)result.Model).Title);
        }


        [Test]
        [Description(
            "Ensure that EnterCalculations passes when all zero values are entered in all/any of the fields as zero is a valid value")]
        public async Task SubmitController_EnterCalculations_POST_ZeroValidValueInFields_NoError()
        {
            ///TODO: create mock of the following (user, organisation and userOrganisation) 
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var returnurl = "CheckData";
            DateTime PrivateAccountingDate = Global.PrivateAccountingDate;
            decimal zero = 0;

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = PrivateAccountingDate,
                DiffMeanBonusPercent = null,
                DiffMeanHourlyPayPercent = zero,
                DiffMedianBonusPercent = null,
                DiffMedianHourlyPercent = zero,
                FemaleLowerPayBand = 50,
                FemaleMedianBonusPayPercent = zero,
                FemaleMiddlePayBand = 60,
                FemaleUpperPayBand = 70,
                FemaleUpperQuartilePayBand = 50,
                MaleLowerPayBand = 50,
                MaleMedianBonusPayPercent = zero,
                MaleMiddlePayBand = 40,
                MaleUpperPayBand = 30,
                MaleUpperQuartilePayBand = 50,
                SectorType = SectorTypes.Private,
                ReportInfo = new ReportInfoModel()
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = SectorTypes.Private.GetAccountingStartDate().Year;
            controller.ReportingOrganisation.SectorType = SectorTypes.Private;
            controller.Bind(returnViewModel);

            controller.StashModel(returnViewModel);

            //Act
            var result = await controller.EnterCalculations(returnViewModel, returnurl) as RedirectToActionResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            // Assert
            //DONE:Since it was stashed no need to check the fields as it is exactly what it was going in before stashing it, Hence ony check that the model is unstashed
            Assert.NotNull(resultModel, "Unstashed model is Invalid Expected ReturnViewModel");
            Assert.That(result.ActionName == "CheckData", "Expected a RedirectToRouteResult to CheckData");

            Assert.NotNull(result, "Expected RedirectResult");
            var x = controller.ViewData.Model as ReturnViewModel;

            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");

            Assert.Multiple(
                () =>
                {
                    Assert.NotNull(result, "Expected ViewResult");
                    Assert.That(resultModel.AccountingDate == returnViewModel.AccountingDate, "Input value does not match model");
                    Assert.That(resultModel.Address == returnViewModel.Address, "Input value does not match model");
                    Assert.That(
                        resultModel.CompanyLinkToGPGInfo == returnViewModel.CompanyLinkToGPGInfo,
                        "Input value does not match model");
                    Assert.That(
                        resultModel.DiffMeanBonusPercent == returnViewModel.DiffMeanBonusPercent,
                        "Input value does not match model");
                    Assert.That(
                        resultModel.DiffMeanHourlyPayPercent == returnViewModel.DiffMeanHourlyPayPercent,
                        "Input value does not match model");
                    Assert.That(
                        resultModel.DiffMedianBonusPercent == returnViewModel.DiffMedianBonusPercent,
                        "Input value does not match model");
                    Assert.That(
                        resultModel.DiffMedianHourlyPercent == returnViewModel.DiffMedianHourlyPercent,
                        "Input value does not match model");
                    Assert.That(resultModel.FemaleLowerPayBand == returnViewModel.FemaleLowerPayBand, "Input value does not match model");
                    Assert.That(
                        resultModel.FemaleMedianBonusPayPercent == returnViewModel.FemaleMedianBonusPayPercent,
                        "Input value does not match model");
                    Assert.That(resultModel.FemaleMiddlePayBand == returnViewModel.FemaleMiddlePayBand, "Input value does not match model");
                    Assert.That(resultModel.FemaleUpperPayBand == returnViewModel.FemaleUpperPayBand, "Input value does not match model");
                    Assert.That(
                        resultModel.FemaleUpperQuartilePayBand == returnViewModel.FemaleUpperQuartilePayBand,
                        "Input value does not match model");
                    Assert.That(resultModel.MaleLowerPayBand == returnViewModel.MaleLowerPayBand, "Input value does not match model");
                    Assert.That(resultModel.MaleMiddlePayBand == returnViewModel.MaleMiddlePayBand, "Input value does not match model");

                    Assert.That(resultModel.MaleUpperPayBand == returnViewModel.MaleUpperPayBand, "Input value does not match model");
                    Assert.That(
                        resultModel.MaleUpperQuartilePayBand == returnViewModel.MaleUpperQuartilePayBand,
                        "Input value does not match model");
                    Assert.That(resultModel.OrganisationId == returnViewModel.OrganisationId, "Input value does not match model");
                    Assert.That(resultModel.OrganisationName == returnViewModel.OrganisationName, "Input value does not match model");
                    Assert.That(resultModel.ReturnId == returnViewModel.ReturnId, "Input value does not match model");
                    Assert.That(resultModel.ReturnUrl == returnViewModel.ReturnUrl, "Input value does not match model");
                    Assert.That(resultModel.Sector == returnViewModel.Sector, "Input value does not match model");
                    Assert.That(resultModel.SectorType == returnViewModel.SectorType, "Input value does not match model");
                    Assert.That(resultModel.FirstName == returnViewModel.FirstName, "Input value does not match model");
                    Assert.That(resultModel.JobTitle == returnViewModel.JobTitle, "Input value does not match model");
                    Assert.That(resultModel.LastName == returnViewModel.LastName, "Input value does not match model");
                });

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test]
        [Description("SubmitController_Init_GET_Redirect Success")]
        public async Task SubmitController_Init_GET_Redirect_Success()
        {
            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "Redirect");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>();
            var result = await controller.Redirect() as RedirectToActionResult;

            //Test the google analytics tracker was executed once on the controller
            controller.WebTracker.GetMockFromObject()
                .Verify(mock => mock.TrackPageViewAsync(It.IsAny<Controller>(), null, null), Times.Once());

            Assert.NotNull(result);
        }

        [Test]
        public async Task SubmitController_Late_Warning_GET_Success()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "organisation-size");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);


            var returnViewModel = new ReturnViewModel();

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                mockedOrganisation.OrganisationId,
                mockedOrganisation.SectorType.GetAccountingStartDate().Year,
                mockedUser.UserId);

            #endregion

            controller.StashModel(returnViewModel);

            string requestString = Encryption.EncryptData($"{mockedOrganisation.OrganisationId.ToString()}:{2017.ToString()}");

            //Act:
            var result = await controller.LateWarning(requestString) as ViewResult;
            Assert.NotNull(result);
        }


        [Test]
        [Description("Submit Controller Late Warning POST When Cannot Decrypt Then Return Bad Request Code")]
        public async Task SubmitController_Late_Warning_POST_Cannot_Decrypt()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "organisation-size");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;
            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            // Act
            var result = await controller.LateWarning(string.Empty) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            // when the request is null 
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test]
        [Description("Submit Controller Late Reason GET Success")]
        public void SubmitController_LateReason_GET_Successs()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);


            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            controller.StashModel(new ReturnViewModel());

            //Act:
            IActionResult result = controller.LateReason();

            Assert.NotNull(result);
        }

        [Test]
        [Description("Submit Controller LateReason When No Stashed Model Then Returns Session Expired")]
        public void SubmitController_LateReason_GET_When_No_Stashed_Model_Then_Returns_Session_Expired()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);


            //Act:
            var result = controller.LateReason() as ViewResult;

            //Assert
            Assert.NotNull(result);
            Assert.AreEqual("Session has expired", ((ErrorViewModel)result.Model).Title);
        }

        [Test]
        [Description("Submit Controller LateReason When User Not Registered")]
        public void SubmitController_LateReason_GET_When_User_Not_Registered_Then_Returns_Custom_Error_Verify_Email()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            //Act:
            var result = controller.LateReason() as ViewResult;
            Assert.NotNull(result);
            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel)result.Model).Title);
        }

        [Test]
        [Description("SubmitController_LateReason_POST_Success")]
        public async Task SubmitController_LateReason_POST_Success()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;
            ;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            mockedDatabase.Setup(x => x.Get<Organisation>(It.IsAny<long>())).Returns(mockedOrganisation);

            mockedDatabase
                .Setup(md => md.Insert(It.IsAny<Return>()))
                .Callback<Return>(returnSentIn => { returnSentIn.Organisation = mockedOrganisation; });

            var model = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 30,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 30,
                MaleMedianBonusPayPercent = 40,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 70,
                OrganisationId = mockedOrganisation.OrganisationId,
                ReturnId = mockedOrganisation.Returns.First().ReturnId,
                LateReason = "Test Late Reason",
                EHRCResponse = "2"
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            model.ReportInfo.Draft = await testService.GetDraftFileAsync(
                mockedOrganisation.OrganisationId,
                mockedOrganisation.SectorType.GetAccountingStartDate().Year,
                mockedUser.UserId);

            #endregion

            controller.Bind(model);
            controller.StashModel(model);

            //ACT:
            var result = await controller.LateReason(model) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.AreEqual(result.ActionName, "SubmissionComplete");
        }

        [Test]
        [Description("Submit Controller Late Submission POST When Model Is Invalid")]
        public async Task SubmitController_LateReason_POST_When_Model_Is_Invalid()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;
            ;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            mockedDatabase
                .Setup(md => md.Insert(It.IsAny<Return>()))
                .Callback<Return>(returnSentIn => { returnSentIn.Organisation = mockedOrganisation; });

            var model = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.gov.uk",
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 30,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 10,
                FemaleMedianBonusPayPercent = 50,
                FemaleMiddlePayBand = 30,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 50,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 30,
                MaleMedianBonusPayPercent = 40,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 40,
                MaleUpperQuartilePayBand = 70,
                OrganisationId = mockedOrganisation.OrganisationId,
                ReturnId = mockedOrganisation.Returns.First().ReturnId,
                LateReason = "",
                EHRCResponse = ""
            };

            controller.Bind(model);
            controller.StashModel(model);

            //ACT:
            var result = await controller.LateReason(model) as ViewResult;

            Assert.NotNull(result);

            Assert.That(!controller.ViewData.ModelState.IsValid, "Model is Invalid");
        }


        [Test]
        [Description("Submit Controller Late Submission User Not Registered")]
        public async Task SubmitController_LateReason_Post_When_User_Is_Not_Registered_Then_Return_Need_Verify_Email_Message()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "EnterCalculations");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;
            ;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);


            var model = new ReturnViewModel();

            //ACT:
            var result = await controller.LateReason(model) as ViewResult;

            // ASSERT
            Assert.NotNull(result);
            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel)result.Model).Title);
        }


        [Test(Author = "Oscar Lagatta")]
        public async Task SubmitController_LateWarning_GET_When_Request_Is_String_Empty_Then_BadRequest_Is_Returned()
        {
            //Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "late-warning");
            mockRouteData.Values.Add("Controller", "Submit");

            DateTime dateTimeNow = VirtualDateTime.Now;
            var testOrganisation = Mock.Of<Organisation>(org => org.SectorType == SectorTypes.Public);
            var testUserOrganisation =
                Mock.Of<UserOrganisation>(x => x.PINConfirmedDate == dateTimeNow && x.Organisation == testOrganisation);

            User testUser = UserHelper.GetRegisteredUserAlreadyLinkedToAnOrganisation(testUserOrganisation);

            var controller = UiTestHelper.GetController<SubmitController>(testUser.UserId, mockRouteData, testUser);
            controller.ReportingOrganisationId = testUserOrganisation.OrganisationId;
            // Act
            var result = await controller.LateWarning(string.Empty) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            // when the request is null 
            Assert.AreEqual(400, result.StatusCode);
        }


        [Test(Author = "Oscar Lagatta")]
        public async Task SubmitController_LateWarning_GET_When_Request_Refers_To_Wrong_Year_Then_BadRequest_Is_Returned()
        {
            //Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "late-warning");
            mockRouteData.Values.Add("Controller", "Submit");

            DateTime dateTimeNow = VirtualDateTime.Now;
            var testOrganisation = Mock.Of<Organisation>(org => org.SectorType == SectorTypes.Public);
            var testUserOrganisation =
                Mock.Of<UserOrganisation>(x => x.PINConfirmedDate == dateTimeNow && x.Organisation == testOrganisation);

            User testUser = UserHelper.GetRegisteredUserAlreadyLinkedToAnOrganisation(testUserOrganisation);

            var controller = UiTestHelper.GetController<SubmitController>(testUser.UserId, mockRouteData, testUser);
            controller.ReportingOrganisationId = testUserOrganisation.OrganisationId;
            int wrongYear = Global.FirstReportingYear - 1;

            // Act
            var result = await controller.LateWarning($"5654:{wrongYear}") as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            // when the request is null 
            Assert.AreEqual(400, result.StatusCode);
        }

        [Test(Author = "Oscar Lagatta")]
        public async Task SubmitController_LateWarning_GET_When_User_Not_Authorised_Then_Unauthorized_Result_Is_Received()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "late-warning");
            mockRouteData.Values.Add("Controller", "Submit");
            var controller = UiTestHelper.GetController<SubmitController>(default, mockRouteData);

            // Act
            var result = await controller.LateWarning(string.Empty) as ChallengeResult;

            // Assert
            Assert.NotNull(result);
        }

        [Test]
        [Description("Organisation Size POST success")]
        public async Task SubmitController_Organisation_Size_POST_Success()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "organisation-size");
            routeData.Values.Add("Controller", "Submit");

            var returnurl = "OrganisationSize";

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.test.com",
                DiffMeanBonusPercent = 20,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 20,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 20,
                FemaleMedianBonusPayPercent = 20,
                FemaleMiddlePayBand = 20,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 20,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 20,
                MaleMedianBonusPayPercent = 20,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 20,
                MaleUpperQuartilePayBand = 20,
                OrganisationId = organisation.OrganisationId,
                OrganisationSize = OrganisationSizes.Employees0To249,
                ReportInfo = new ReportInfoModel()
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.Bind(returnViewModel);

            controller.StashModel(returnViewModel);

            // Act
            var result = await controller.OrganisationSize(returnViewModel, returnurl) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("EmployerWebsite", result.ActionName, "Expected a RedirectToActionResult to EmployerWebsite");

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("Submit Controller Organisation Size Get with a Non Registered User")]
        public async Task SubmitController_OrganisationSize_GET_Non_Registered_User()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "organisation-size");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);


            controller.StashModel(new ReturnViewModel());
            //Act:
            var result = await controller.OrganisationSize() as ViewResult;
            Assert.NotNull(result);
            Assert.IsTrue(result.ViewName == "CustomError");
        }

        [Test]
        [Description("SubmitController_OrganisationSize_GET_Success")]
        public async Task SubmitController_OrganisationSize_GET_Success()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            mockedOrganisation.OrganisationId = new Random().Next(5000, 9999);
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "organisation-size");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            var returnViewModel = new ReturnViewModel();

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                mockedOrganisation.OrganisationId,
                mockedOrganisation.SectorType.GetAccountingStartDate().Year,
                mockedUser.UserId);

            #endregion

            controller.StashModel(returnViewModel);

            //Act:
            var result = await controller.OrganisationSize() as ViewResult;
            Assert.NotNull(result);

            // Clean up
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("Submit Controller Organisation Size Get with Session Expired View")]
        public async Task SubmitController_OrganisationSize_GET_With_Session_Expired_View()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "organisation-size");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            //Act:
            var result = await controller.OrganisationSize() as ViewResult;
            Assert.NotNull(result);
            Assert.AreEqual("Session has expired", ((ErrorViewModel)result.Model).Title);
        }

        [Test]
        [Description("Organisation Size When Model Isn't Stashed Returns Session Expired")]
        public async Task SubmitController_OrganisationSize_POST_When_Model_Not_Stashed_Returns_Session_Expired()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "organisation-size");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            var model = new ReturnViewModel();

            var result = await controller.OrganisationSize(model) as ViewResult;

            // Assert 
            Assert.NotNull(result);
            Assert.AreEqual("Session has expired", ((ErrorViewModel)result.Model).Title);
        }

        [Test]
        [Description("SubmitController_OrganisationSize_POST_When_User_Not_Registered")]
        public async Task SubmitController_OrganisationSize_POST_When_User_Not_Registered()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "organisation-size");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);


            var model = new ReturnViewModel();
            //Act:
            var result = await controller.OrganisationSize(model) as ViewResult;
            Assert.NotNull(result);
            Assert.AreEqual(
                "Please verify your email address to continue",
                ((ErrorViewModel)result.Model).Description);
        }

        [Test]
        [Description("Ensure the Person Responsible form is returned for the current user")]
        public async Task SubmitController_PersonResponsible_GET_Success()
        {
            // Arrange
            var user = new User
            {
                UserId = new Random().Next(1000, 9999),
                EmailAddress = "magnuski@hotmail.com",
                EmailVerifiedDate = VirtualDateTime.Now
            };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            organisation.OrganisationId = new Random().Next(1000, 9999);
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = user.UserId,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "PersonResponsible");
            routeData.Values.Add("Controller", "Submit");

            string returnUrl = null;

            var returnViewModel = new ReturnViewModel();

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller = UiTestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.ReportingOrganisationStartYear = organisation.SectorType.GetAccountingStartDate().Year;

            controller.StashModel(returnViewModel);

            //ACT:
            var result = await controller.PersonResponsible(returnUrl) as ViewResult;
            var resultModel = result.Model as ReturnViewModel;

            //ASSERT:
            Assert.That(result != null && result is ViewResult, " Expected a viewResult or Incorrect resultType returned");
            Assert.That(result.Model is ReturnViewModel, "Incorrect model type returned");
            Assert.That(result.ViewName == "PersonResponsible", "Incorrect view returned");

            //TODO wrong should be checking its invalid: for negative tests yes.
            Assert.That(result.ViewData.ModelState.IsValid, "Model is Invalid");

            //DONE should be checking each field for exact error message in modelstate
            Assert.Null(resultModel.FirstName, "FirstName:Expected a null  or empty field");
            Assert.Null(resultModel.LastName, "LastName:Expected a null  or empty field");
            Assert.Null(resultModel.JobTitle, "JobTitle:Expected a null  or empty field");

            // Clean up
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test(Author = "Oscar Lagatta")]
        public async Task SubmitController_PersonResponsible_Get_When_Not_Registered_User_Then_Returns_Error()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = null };
            var organisation = new Organisation { OrganisationId = 1 };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "PersonResponsible");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = null;

            var model = new ReturnViewModel();

            var controller = UiTestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            //Stash an object to pass in for this.ClearStash()
            controller.StashModel(model);

            //ACT:
            var result = await controller.PersonResponsible(returnurl) as ViewResult;
            var resultModel = result.Model as ReturnViewModel;

            //ASSERT:
            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel)result.Model).Title);
            Assert.That(result != null && result is ViewResult, " Expected a viewResult or Incorrect resultType returned");
        }

        [Test(Author = "Oscar Lagatta")]
        public async Task SubmitController_PersonResponsible_Get_When_Not_Stashed_ViewModel_Then_Returns_Session_Expired()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", "PersonResponsible");
            routeData.Values.Add("Controller", "Submit");

            string returnurl = null;

            var controller = UiTestHelper.GetController<SubmitController>(user.UserId, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            //ACT:
            var result = await controller.PersonResponsible(returnurl) as ViewResult;

            //ASSERT:
            Assert.NotNull(result);
            Assert.AreEqual("Session has expired", ((ErrorViewModel)result.Model).Title);
        }

        [Test]
        [Description("Ensure that Person Responsible form is filled and sent successfully")]
        public async Task SubmitController_PersonResponsible_POST_Success()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "PersonResponsible");
            routeData.Values.Add("Controller", "Submit");

            var returnurl = "OrganisationSize";

            DateTime PrivateAccountingDate = Global.PrivateAccountingDate;

            var returnViewModel = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.test.com",
                DiffMeanBonusPercent = 20,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 20,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 20,
                FemaleMedianBonusPayPercent = 20,
                FemaleMiddlePayBand = 20,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 20,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 20,
                MaleMedianBonusPayPercent = 20,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 20,
                MaleUpperQuartilePayBand = 20,
                OrganisationId = organisation.OrganisationId,
                ReportInfo = new ReportInfoModel()
            };

            #region We must load a draft if we are calling Enter calculations with a stashed model

            var testDataRepository = UiTestHelper.DIContainer.Resolve<IDataRepository>();
            var testDraftFileBL = new DraftFileBusinessLogic(testDataRepository);
            var testService = new SubmissionService(null, null, testDraftFileBL);

            returnViewModel.ReportInfo.Draft = await testService.GetDraftFileAsync(
                organisation.OrganisationId,
                organisation.SectorType.GetAccountingStartDate().Year,
                user.UserId);

            #endregion

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;
            controller.Bind(returnViewModel);

            controller.StashModel(returnViewModel);

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.PersonResponsible(returnViewModel, returnurl) as RedirectToActionResult;
            var resultModel = controller.UnstashModel<ReturnViewModel>();

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result, "Expected RedirectToActionResult");
            Assert.NotNull(resultModel, "Unstashed model is Invalid Expected ReturnViewModel");

            //4.Check that the redirection went to the right url step.
            Assert.That(result.ActionName == "OrganisationSize", "Expected a RedirectToActionResult to OrganisationSize");

            //TODO you are not checking here for model state is invalid
            Assert.That(controller.ViewData.ModelState.IsValid, "Model is Invalid");
            //DONE you should be checking modelstate.isvalid and each modelstate error
            //DONE you should be checking only the exact failed fields show and error message
            Assert.That(controller.ViewData.ModelState.IsValidField("FirstName"), "Model is Invalid");
            Assert.That(controller.ViewData.ModelState.IsValidField("LastName"), "Model is Invalid");
            Assert.That(controller.ViewData.ModelState.IsValidField("Title"), "Model is Invalid");

            //TODO you should be checking each error message is exact as per config file

            // Cleanup
            await testService.DiscardDraftFileAsync(returnViewModel);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("When User Isn't a Registered User")]
        public async Task SubmitController_PersonResponsible_POST_User_Not_Registered()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = null };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "PersonResponsible");
            routeData.Values.Add("Controller", "Submit");

            var returnurl = "OrganisationSize";

            DateTime PrivateAccountingDate = Global.PrivateAccountingDate;

            var model = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.test.com",
                DiffMeanBonusPercent = 20,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 20,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 20,
                FemaleMedianBonusPayPercent = 20,
                FemaleMiddlePayBand = 20,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 20,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 20,
                MaleMedianBonusPayPercent = 20,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 20,
                MaleUpperQuartilePayBand = 20,
                OrganisationId = organisation.OrganisationId
            };

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.PersonResponsible(model, returnurl) as ViewResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result);
            Assert.AreEqual("You haven’t verified your email address yet", ((ErrorViewModel)result.Model).Title);
        }

        [Test(Author = "Oscar Lagatta")]
        [Description("When Session Expired")]
        public async Task SubmitController_PersonResponsible_POST_When_Session_Expired()
        {
            // Arrange
            var user = new User { UserId = 1, EmailAddress = "magnuski@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now };
            var organisation = new Organisation { OrganisationId = 1, SectorType = SectorTypes.Private };
            var userOrganisation = new UserOrganisation
            {
                OrganisationId = organisation.OrganisationId,
                Organisation = organisation,
                UserId = 1,
                PINConfirmedDate = VirtualDateTime.Now,
                PIN = "1"
            };

            //set mock routeData
            var routeData = new RouteData();
            routeData.Values.Add("Action", "PersonResponsible");
            routeData.Values.Add("Controller", "Submit");

            var returnurl = "OrganisationSize";

            DateTime PrivateAccountingDate = Global.PrivateAccountingDate;

            var model = new ReturnViewModel
            {
                AccountingDate = Global.PrivateAccountingDate,
                CompanyLinkToGPGInfo = "http://www.test.com",
                DiffMeanBonusPercent = 20,
                DiffMeanHourlyPayPercent = 20,
                DiffMedianBonusPercent = 20,
                DiffMedianHourlyPercent = 20,
                FemaleLowerPayBand = 20,
                FemaleMedianBonusPayPercent = 20,
                FemaleMiddlePayBand = 20,
                FemaleUpperPayBand = 20,
                FemaleUpperQuartilePayBand = 20,
                FirstName = "Test FirstName",
                LastName = "Test LastName",
                JobTitle = "Developer",
                MaleLowerPayBand = 20,
                MaleMedianBonusPayPercent = 20,
                MaleMiddlePayBand = 20,
                MaleUpperPayBand = 20,
                MaleUpperQuartilePayBand = 20,
                OrganisationId = organisation.OrganisationId
            };

            var controller = UiTestHelper.GetController<SubmitController>(1, routeData, user, organisation, userOrganisation);
            controller.ReportingOrganisationId = organisation.OrganisationId;

            //ACT:
            //2.Run and get the result of the test
            var result = await controller.PersonResponsible(model, returnurl) as ViewResult;

            // ASSERT:
            //3.Check that the result is not null
            Assert.NotNull(result);
            Assert.AreEqual("Session has expired", ((ErrorViewModel)result.Model).Title);
        }

        [Test]
        [Description("SubmitController_SubmissionComplete_GET_Success")]
        public async Task SubmitController_SubmissionComplete_GET_Success()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "submission-complete");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            controller.StashModel(new ReturnViewModel());
            //Act:
            var result = await controller.SubmissionComplete() as ViewResult;
            Assert.NotNull(result);
        }

        [Test]
        [Description("Submission Complete Get When User Not Verified")]
        public async Task SubmitController_SubmissionComplete_Get_User_Not_Registered()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "submission-complete");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);


            //Act:
            var result = await controller.SubmissionComplete() as ViewResult;

            Assert.NotNull(result);
        }

        [Test]
        public async Task SubmitController_SubmissionComplete_Get_When_Unstashed_Model_Error()
        {
            User mockedUser = UserHelper.GetNotAdminUserWithVerifiedEmailAddress();
            Organisation mockedOrganisation = OrganisationHelper.GetPublicOrganisation();
            UserOrganisation mockedUserOrganisation = UserOrganisationHelper.LinkUserWithOrganisation(mockedUser, mockedOrganisation);
            Return mockedReturn = ReturnHelper.GetSubmittedReturnForOrganisationAndYear(mockedUserOrganisation, Global.FirstReportingYear);

            OrganisationHelper.LinkOrganisationAndReturn(mockedOrganisation, mockedReturn);

            // route data
            var routeData = new RouteData();
            routeData.Values.Add("action", "submission-complete");
            routeData.Values.Add("controller", "submit");

            var controller = UiTestHelper.GetController<SubmitController>(mockedUser.UserId, routeData, null);
            controller.ReportingOrganisationId = mockedOrganisation.OrganisationId;
            controller.ReportingOrganisationStartYear = Global.FirstReportingYear;

            Mock<IDataRepository> mockedDatabase = AutoFacExtensions.ResolveAsMock<IDataRepository>();

            mockedDatabase.SetupGetAll(mockedUser, mockedOrganisation, mockedUserOrganisation, mockedReturn);

            mockedDatabase
                .Setup(x => x.Get<User>(It.IsAny<long>()))
                .Returns(mockedUser);

            //Act:
            var result = await controller.SubmissionComplete() as ViewResult;

            Assert.NotNull(result);
        }
    }

    internal static class SubmissionCOntrollerTestHelpers
    {
        public static bool IsValidField(this ModelStateDictionary modelState, string key)
        {
            return modelState[key] == null || modelState[key].ValidationState == ModelValidationState.Valid;
        }
    }


}
