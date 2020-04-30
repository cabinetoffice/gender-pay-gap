using System;
using System.Linq;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Scope;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers
{
    [TestFixture]
    [SetCulture("en-GB")]
    public class ScopeControllerTests
    {

        private const string ErrorMessage = "TheErrorMessage";

        //TODO: Add to scope unit tests
        [Ignore("Needs adding to scope in/out successful tests")]
        [Test]
        [Description("When an organisation is out of scope check if removed from search")]
        public void CheckOutOfScopeRemovesFromSearch()
        {
            //Check the organisation exists in search
            //var actualIndex = controller.SearchBusinessLogic.SearchRepository.Get(org.OrganisationId.ToString());
            //var expectedIndex = org.ToEmployerSearchResult();
            //expectedIndex.Compare(actualIndex);
        }

        //TODO: Add to scope unit tests
        [Ignore("Needs adding to scope in/out successful tests")]
        [Test]
        [Description("When an organisation is in scope check if added to search")]
        public void CheckInScopeAddedToSearch()
        {
            //Check the organisation exists in search
            //var actualIndex = controller.SearchBusinessLogic.SearchRepository.Get(org.OrganisationId.ToString());
            //var expectedIndex = org.ToEmployerSearchResult();
            //expectedIndex.Compare(actualIndex);
        }

        #region Helpers

        private static UserOrganisation CreateUserOrganisation(Organisation org, long userId, DateTime? pinConfirmedDate)
        {
            return new UserOrganisation {
                Organisation = org, UserId = userId, PINConfirmedDate = pinConfirmedDate, Address = org.GetLatestAddress()
            };
        }

        private static Organisation createPrivateOrganisation(long organistationId, string organisationName, int companyNumber)
        {
            return new Organisation {
                OrganisationId = organistationId,
                OrganisationName = organisationName,
                SectorType = SectorTypes.Private,
                Status = OrganisationStatuses.Active,
                CompanyNumber = companyNumber.ToString()
            };
        }

        private static User CreateUser(long userId, string emailAddress)
        {
            return new User {
                UserId = userId,
                EmailAddress = emailAddress,
                EmailAddressDB = emailAddress,
                Firstname = "FirstName" + userId,
                Lastname = "LastName" + userId,
                EmailVerifiedDate = VirtualDateTime.Now,
                Status = UserStatuses.Active
            };
        }

        #endregion

        #region InScope

        [Test]
        [Description("ScopeController_InScope_GET_Success")]
        public async Task ScopeController_InScope_GET_When_CurrentUser_Is_Not_Admin_Then_Redirect_To_ManageOrganisations()
        {
            // Arrange
            User notAdminUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.InScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(-1, routeData, notAdminUser);

            // Act
            var result = await controller.InScope() as RedirectToActionResult;

            // Assert

            //Test the google analytics tracker was executed once on the controller
            controller.WebTracker.GetMockFromObject()
                .Verify(mock => mock.TrackPageViewAsync(It.IsAny<Controller>(), null, null), Times.Once());

            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("ManageOrganisations", result.ActionName, "Expected the Action to be 'ManageOrganisations'");
            Assert.AreEqual("Organisation", result.ControllerName, "Expected the Controller to be 'Organisation'");
        }

        [Test]
        [Description("ScopeController_InScope_GET_Success")]
        public async Task ScopeController_InScope_GET_When_CurrentUser_Is_Null_Then_Redirect_To_ManageOrganisations()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.InScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(99, routeData);

            // Act
            var result = await controller.InScope() as RedirectToActionResult;

            // Assert
            //Test the google analytics tracker was executed once on the controller
            controller.WebTracker.GetMockFromObject()
                .Verify(mock => mock.TrackPageViewAsync(It.IsAny<Controller>(), null, null), Times.Once());

            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("ManageOrganisations", result.ActionName, "Expected the Action to be 'ManageOrganisations'");
            Assert.AreEqual("Organisation", result.ControllerName, "Expected the Controller to be 'Organisation'");
        }

        [Test]
        [Description("ScopeController InScope GET When User is Admin then Redirect to Home Admin")]
        public async Task ScopeController_InScope_GET_When_User_is_Admin_then_Redirect_to_Home_Admin()
        {
            // Arrange
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.InScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(-1, routeData, govEqualitiesOfficeUser);

            // Act
            var result = await controller.InScope() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("Home", result.ActionName, "Expected the Action to be 'Home'");
            Assert.AreEqual("Admin", result.ControllerName, "Expected the Controller to be 'Admin'");
        }

        #endregion

        #region ConfirmInScope

        [Test]
        [Description("ScopeController ConfirmInScope GET When User is Admin then Redirect to Admin")]
        public void ScopeController_ConfirmInScope_GET_When_User_is_Admin_then_Redirect_to_Admin()
        {
            // Arrange
            User adminUser = UserHelper.GetAdminUser();

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmInScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(-1, routeData, adminUser);

            // Act
            var result = controller.ConfirmInScope() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("Home", result.ActionName, "Expected the Action to be 'Home'");
            Assert.AreEqual("Admin", result.ControllerName, "Expected the Controller to be 'Admin'");
        }

        [Test]
        [Description("ScopeController ConfirmInScope GET When Session Cache Is Empty Then Return Session Has Expired Custom Error")]
        public void ScopeController_ConfirmInScope_GET_When_Session_Cache_Is_Empty_Then_Return_Session_Has_Expired_Custom_Error()
        {
            // Arrange
            User notAdminUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmInScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(-1, routeData, notAdminUser);

            // Act
            var result = controller.ConfirmInScope() as ViewResult;
            Assert.NotNull(result, "ViewResult should not be null");

            var errorViewModel = result.Model as ErrorViewModel;
            Assert.NotNull(errorViewModel, "ErrorViewModel should not be null");

            // Assert
            Assert.AreEqual("CustomError", result.ViewName, "Expected the resulting view to be a 'CustomError'");

            Assert.AreEqual("Session has expired", errorViewModel.Title);
            Assert.AreEqual("Your session has expired.", errorViewModel.Description);
            Assert.AreEqual("Continue", errorViewModel.ActionText);
            Assert.AreEqual(1134, errorViewModel.ErrorCode);
        }

        [Test]
        [Description("ScopeController ConfirmInScope GET When ScopingViewModel Can Be Unstashed Then Return ConfirmInScope ViewResult")]
        public void ScopeController_ConfirmInScope_GET_When_ScopingViewModel_Can_Be_Unstashed_Then_Return_ConfirmInScope_ViewResult()
        {
            // Arrange
            User notAdminUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmInScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(-1, routeData, notAdminUser);

            ScopingViewModel scopingViewModel = ScopingViewModelHelper.GetScopingViewModel();
            controller.StashModel(scopingViewModel);

            // Act
            var result = controller.ConfirmInScope() as ViewResult;
            Assert.NotNull(result, "ViewResult should not be null");

            // Assert
            Assert.AreEqual("ConfirmInScope", result.ViewName, "Expected the resulting ViewName to be 'ConfirmInScope'");
        }

        [Test]
        [Description("ScopeController ConfirmInScope POST When User is Admin then Redirect to Admin")]
        public async Task ScopeController_ConfirmInScope_POST_When_User_is_Admin_then_Redirect_to_AdminAsync()
        {
            // Arrange
            User adminUser = UserHelper.GetAdminUser();

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmInScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(-1, routeData, adminUser);

            // Act
            var result = await controller.ConfirmInScope(string.Empty) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("Home", result.ActionName, "Expected the Action to be 'Home'");
            Assert.AreEqual("Admin", result.ControllerName, "Expected the Controller to be 'Admin'");
        }

        [Test]
        [Description("ScopeController ConfirmInScope POST When Session Cache Is Empty Then Return Session Has Expired Custom Error")]
        public async Task
            ScopeController_ConfirmInScope_POST_When_Session_Cache_Is_Empty_Then_Return_Session_Has_Expired_Custom_ErrorAsync()
        {
            // Arrange
            User notAdminUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmInScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(-1, routeData, notAdminUser);

            // Act
            var result = await controller.ConfirmInScope(string.Empty) as ViewResult;
            Assert.NotNull(result, "ViewResult should not be null");

            var errorViewModel = result.Model as ErrorViewModel;
            Assert.NotNull(errorViewModel, "ErrorViewModel should not be null");

            // Assert
            Assert.AreEqual("CustomError", result.ViewName, "Expected the resulting view to be a 'CustomError'");

            Assert.AreEqual("Session has expired", errorViewModel.Title);
            Assert.AreEqual("Your session has expired.", errorViewModel.Description);
            Assert.AreEqual("Continue", errorViewModel.ActionText);
            Assert.AreEqual(1134, errorViewModel.ErrorCode);
        }

        [Test]
        [Description("ScopeController ConfirmInScope POST When ScopingViewModel Can Be Unstashed Then Return ConfirmInScope ViewResult")]
        public async Task
            ScopeController_ConfirmInScope_POST_When_ScopingViewModel_Can_Be_Unstashed_Then_Return_ConfirmInScope_ViewResultAsync()
        {
            // Arrange
            User user = CreateUser(1, "user@test.com");
            var organisationId = 400;
            Organisation organisation = createPrivateOrganisation(organisationId, "Company1", 12345678);
            UserOrganisation userOrganisation = CreateUserOrganisation(organisation, user.UserId, VirtualDateTime.Now);

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmInScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(
                user.UserId,
                routeData,
                user,
                organisation,
                userOrganisation);

            ScopingViewModel scopingViewModel = ScopingViewModelHelper.GetScopingViewModel();
            // organisationId is getting overriden somewhere and so this is a hack to keep them the same
            scopingViewModel.OrganisationId = organisationId;
            controller.StashModel(scopingViewModel);

            string encryptedOrganisationId = Encryption.EncryptQuerystring(organisationId.ToString());

            // Act
            var result = await controller.ConfirmInScope(string.Empty) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(
                encryptedOrganisationId,
                result.RouteValues["Id"],
                $"Expected the organisation id to be Zero, which, once encrypted, it'll result in this blurb --> {encryptedOrganisationId}");
            Assert.AreEqual("ManageOrganisation", result.ActionName, "Expected the Action to be 'ManageOrganisation'");
            Assert.AreEqual("Organisation", result.ControllerName, "Expected the Controller to be 'Organisation'");
        }

        [Test]
        [Description("ScopeController ConfirmInScope POST When Scope is Changed to In For Current Year Then Email Users")]
        public async Task ScopeController_ConfirmInScope_POST_When_Scope_is_Changed_to_In_For_Current_Year_Then_Email_Users()
        {
            // Arrange
            User user1 = CreateUser(1, "user1@test.com");
            User user2 = CreateUser(2, "user2@test.com");
            var organisationId = 100;
            Organisation organisation = createPrivateOrganisation(organisationId, "Company1", 12345678);
            UserOrganisation userOrganisation1 = CreateUserOrganisation(organisation, user1.UserId, VirtualDateTime.Now);
            UserOrganisation userOrganisation2 = CreateUserOrganisation(organisation, user2.UserId, VirtualDateTime.Now);

            User user3 = CreateUser(3, "user3@test.com");
            Organisation otherOrganisation = createPrivateOrganisation(200, "Company2", 12341234);
            UserOrganisation userOrganisation3 = CreateUserOrganisation(otherOrganisation, user3.UserId, VirtualDateTime.Now);

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmInScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(
                user1.UserId,
                routeData,
                user1,
                user2,
                user3,
                organisation,
                otherOrganisation,
                userOrganisation1,
                userOrganisation2,
                userOrganisation3);

            ScopingViewModel scopingViewModel = ScopingViewModelHelper.GetScopingViewModel();
            // organisationId is getting overriden somewhere and so this is a hack to keep them the same
            scopingViewModel.OrganisationId = organisationId;

            // this may well break at some point in future
            scopingViewModel.AccountingDate = new DateTime(2022, 4, 05);

            controller.StashModel(scopingViewModel);

            var mockEmailQueue = new Mock<IQueue>();
            Program.MvcApplication.SendNotifyEmailQueue = mockEmailQueue.Object;
            mockEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<NotifyEmail>()));

            // Act
            var result = await controller.ConfirmInScope(string.Empty) as RedirectToActionResult;

            // Assert
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user1.EmailAddress))),
                Times.Once(),
                "Expected the current user's email address to be in the email send queue");
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.ScopeChangeInEmail))),
                Times.Exactly(2),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.ScopeChangeInEmail}");
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user2.EmailAddress))),
                Times.Once(),
                "Expected the other user of the same organisation's email address to be in the email send queue");
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user3.EmailAddress))),
                Times.Never,
                "Do not expect users of other organisations to be in the email send queue");
        }

        [Test]
        [Description("ScopeController ConfirmInScope POST When Scope is Changed to In For Previous Years Then Dont Email Users")]
        public async Task ScopeController_ConfirmInScope_POST_When_Scope_is_Changed_to_In_For_Previous_Years_Then_Dont_Email_Users()
        {
            // Arrange
            User user1 = CreateUser(1, "user1@test.com");
            User user2 = CreateUser(2, "user2@test.com");
            var organisationId = 101;
            Organisation organisation = createPrivateOrganisation(organisationId, "Company1", 12345678);
            UserOrganisation userOrganisation1 = CreateUserOrganisation(organisation, user1.UserId, VirtualDateTime.Now);
            UserOrganisation userOrganisation2 = CreateUserOrganisation(organisation, user2.UserId, VirtualDateTime.Now);

            User user3 = CreateUser(3, "user3@test.com");
            Organisation otherOrganisation = createPrivateOrganisation(201, "Company2", 12341234);
            UserOrganisation userOrganisation3 = CreateUserOrganisation(otherOrganisation, user3.UserId, VirtualDateTime.Now);

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmInScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(
                user1.UserId,
                routeData,
                user1,
                user2,
                user3,
                organisation,
                otherOrganisation,
                userOrganisation1,
                userOrganisation2,
                userOrganisation3);

            ScopingViewModel scopingViewModel = ScopingViewModelHelper.GetScopingViewModel();
            // organisationId is getting overriden somewhere and so this is a hack to keep them the same
            scopingViewModel.OrganisationId = organisationId;

            // this may well break at some point in future
            scopingViewModel.AccountingDate = new DateTime(2016, 4, 05);

            controller.StashModel(scopingViewModel);

            var mockEmailQueue = new Mock<IQueue>();
            Program.MvcApplication.SendNotifyEmailQueue = mockEmailQueue.Object;
            mockEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<NotifyEmail>()));

            // Act
            var result = await controller.ConfirmInScope(string.Empty) as RedirectToActionResult;

            // Assert
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user1.EmailAddress))),
                Times.Never(),
                "Do not expect the current user's email address to be in the email send queue");
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user2.EmailAddress))),
                Times.Never(),
                "Do not expect the other user of the same organisation's email address to be in the email send queue");
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user3.EmailAddress))),
                Times.Never,
                "Do not expect users of other organisations to be in the email send queue");
        }

        #endregion

        #region GET (completed work)

        [Test]
        [Description("ScopeController OutOfScope GET: When User is Admin then Redirect to Admin")]
        public async Task ScopeController_OutOfScope_GET_When_User_is_Admin_then_Redirect_to_AdminAsync()
        {
            // Arrange
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.OutOfScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(-1, routeData, govEqualitiesOfficeUser);

            // Act
            var result = await controller.OutOfScope() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "Home", "Expected the Action to be 'Home'");
            Assert.AreEqual(result.ControllerName, "Admin", "Expected the Controller to be 'Admin'");
        }

        [Test]
        [Description("ScopeController OutOfScope GET When Called Then PendingFasttrackCodes Should Be Cleared")]
        public async Task ScopeController_OutOfScope_GET_When_Called_Then_PendingFasttrackCodes_Should_Be_ClearedAsync()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();
            controller.PendingFasttrackCodes =
                "This field, which normally will look like this 'EmployerReference:SecurityToken' must be changed to new ModelState() after this test";

            // Act
            var result = await controller.OutOfScope() as ViewResult;

            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "EnterCodes", "Expected the ViewName to be 'EnterCodes'");
            Assert.IsNull(controller.PendingFasttrackCodes, "Expected PendingFasttrackCodes to have been changed to null");
        }

        [Test]
        [Description(
            "ScopeController OutOfScope GET When scopingViewModel Was Not Stashed Then A New EnterCodesViewModel Will Be Created Internally")]
        public async Task
            ScopeController_OutOfScope_GET_When_scopingViewModel_Was_Not_Stashed_Then_A_New_EnterCodesViewModel_Will_Be_Created_InternallyAsync()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();
            controller.PendingFasttrackCodes =
                "This field, which normally will look like this 'EmployerReference:SecurityToken' must be changed to new ModelState() after this test";
            controller.StashModel((ScopingViewModel) null);

            // Act
            var result = await controller.OutOfScope() as ViewResult;
            Assert.NotNull(result, "ViewResult should not be null");

            var model = result.ViewData.Model as EnterCodesViewModel;
            Assert.NotNull(model, "EnterCodesViewModel should not be null");

            // Assert
            Assert.IsNull(model.EmployerReference, "Expected the EmployerReference of the model to be null");
            Assert.IsNull(model.SecurityToken, "Expected the SecurityToken of the model to be null");
        }

        [Test]
        [Description(
            "ScopeController OutOfScope GET When Stashed ScopingViewModel Has new ModelState() EnterCodes Then A New EnterCodesViewModel Will Be Created Internally")]
        public async Task
            ScopeController_OutOfScope_GET_When_Stashed_ScopingViewModel_Has_Null_EnterCodes_Then_A_New_EnterCodesViewModel_Will_Be_Created_InternallyAsync()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();
            controller.PendingFasttrackCodes =
                "This field, which normally will look like this 'EmployerReference:SecurityToken' must be changed to new ModelState() after this test";
            controller.StashModel(new ScopingViewModel());

            // Act
            var result = await controller.OutOfScope() as ViewResult;
            Assert.NotNull(result, "ViewResult should not be null");

            var model = result.ViewData.Model as EnterCodesViewModel;
            Assert.NotNull(model, "EnterCodesViewModel should not be null");

            // Assert
            Assert.IsNull(model.EmployerReference, "Expected the EmployerReference of the model to be null");
            Assert.IsNull(model.SecurityToken, "Expected the SecurityToken of the model to be null");
        }

        [Test]
        [Description("ScopeController OutOfScope GET: When HasSpamLock Then Return CustomError View")]
        public async Task ScopeController_OutOfScope_GET_When_HasSpamLock_Then_Return_CustomError_ViewAsync()
        {
            bool settingValueBeforeTheTest = Global.SkipSpamProtection; // Remember the original value of this setting

            try
            {
                // Arrange
                DateTime dateTimeNow = VirtualDateTime.Now;
                var controller = UiTestHelper.GetController<ScopeController>();
                await controller.Cache.SetAsync("127.0.0.1:lastScopeCode", dateTimeNow);

                Global.SkipSpamProtection = false;

                // Act
                var result = await controller.OutOfScope() as ViewResult;
                Assert.NotNull(result, "ViewResult should not be null");

                var model = result.Model as ErrorViewModel;
                Assert.NotNull(model, "ErrorViewModel should not be null");

                // Assert
                Assert.AreEqual(result.ViewName, "CustomError", "Expected the ViewName to be 'CustomError'");
                Assert.AreEqual("Continue", model.ActionText);
                Assert.AreEqual("/", model.ActionUrl);
                StringAssert.IsMatch("(Please try again in).*(minutes)", model.CallToAction);
                Assert.AreEqual("You've attempted this action too many times.", model.Description);
                Assert.AreEqual(1125, model.ErrorCode);
                Assert.AreEqual("Too many attempts", model.Title);
            }
            finally
            {
                Global.SkipSpamProtection = settingValueBeforeTheTest; // Reinstate the value as it was before this test.
            }
        }

        #endregion

        #region POST OutOfScope

        [Test]
        [Description("ScopeController OutOfScope POST: When User is Admin then Redirect to Admin")]
        public async Task ScopeController_OutOfScope_POST_When_User_is_Admin_then_Redirect_to_AdminAsync()
        {
            // Arrange
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            var controller = UiTestHelper.GetController<ScopeController>(-1, null, govEqualitiesOfficeUser);

            // Act
            var result = await controller.OutOfScope(new EnterCodesViewModel()) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "Home", "Expected the Action to be 'Home'");
            Assert.AreEqual(result.ControllerName, "Admin", "Expected the Controller to be 'Admin'");
        }

        [Test]
        [Description("ScopeController OutOfScope POST: When HasSpamLock Then Return CustomError View")]
        public async Task ScopeController_OutOfScope_POST_When_HasSpamLock_Then_Return_CustomError_ViewAsync()
        {
            bool settingValueBeforeTheTest = Global.SkipSpamProtection; // Remember the original value of this setting

            try
            {
                // Arrange
                DateTime dateTimeNow = VirtualDateTime.Now;
                var controller = UiTestHelper.GetController<ScopeController>();
                await controller.Cache.SetAsync("127.0.0.1:lastScopeCode", dateTimeNow);

                Global.SkipSpamProtection = false;

                // Act
                var result = await controller.OutOfScope(new EnterCodesViewModel()) as ViewResult;
                Assert.NotNull(result, "ViewResult should not be null");

                var model = result.Model as ErrorViewModel;
                Assert.NotNull(model, "ErrorViewModel should not be null");

                // Assert
                Assert.AreEqual(result.ViewName, "CustomError", "Expected the ViewName to be 'CustomError'");
                Assert.AreEqual("Continue", model.ActionText);
                Assert.AreEqual("/", model.ActionUrl);
                StringAssert.IsMatch("(Please try again in).*(minutes)", model.CallToAction);
                Assert.AreEqual("You've attempted this action too many times.", model.Description);
                Assert.AreEqual(1125, model.ErrorCode);
                Assert.AreEqual("Too many attempts", model.Title);
            }
            finally
            {
                Global.SkipSpamProtection = settingValueBeforeTheTest; // Reinstate the value as it was before this test.
            }
        }

        [Test]
        [Description("ScopeController OutOfScope POST: When ModelState is Not Valid Then Return the EnterCodes View")]
        public async Task ScopeController_OutOfScope_POST_When_ModelState_is_Not_Valid_Then_Return_the_EnterCodes_ViewAsync()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();
            controller.ModelState.AddModelError("SecurityToken", ErrorMessage);

            var testPostModel = new EnterCodesViewModel {SecurityToken = ""};

            // Act
            var result = await controller.OutOfScope(testPostModel) as ViewResult;
            ModelStateEntry modelState = controller.ModelState["SecurityToken"];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "EnterCodes", "Expected the ViewName to be 'EnterCodes'");
            Assert.AreEqual("TheErrorMessage", reportedError.ErrorMessage, "Expected the ErrorMessage to be 'TheErrorMessage'");
        }

        [Test]
        [Description("ScopeController OutOfScope POST: When Codes Do Not match Then Return OutOfScope View with error state")]
        public async Task ScopeController_OutOfScope_POST_When_Codes_Do_Not_Match_Then_Return_OutOfScope_ViewAsync()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();
            AutoFacExtensions.ResolveAsMock<IScopePresentation>(true);

            // Act
            var result = await controller.OutOfScope(new EnterCodesViewModel()) as ViewResult;
            Assert.NotNull(result, "ViewResult should not be null");
            ModelStateEntry modelState = controller.ModelState[""];
            ModelError reportedError = modelState.Errors.First();

            // Assert
            Assert.AreEqual("EnterCodes", result.ViewName, "Expected the ViewName to be 'EnterCodes'");
            Assert.NotNull(reportedError);
            Assert.AreEqual("There's a problem with your employer reference or security code", reportedError.ErrorMessage);
        }

        [Test]
        [Description("ScopeController OutOfScope POST: When SecurityCode Expired Then Return CodeExpired Redirect")]
        public async Task ScopeController_OutOfScope_POST_When_SecurityCode_Expired_Then_Return_CodeExpired_RedirectAsync()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.OutOfScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(0, routeData);
            var testPostModel = new EnterCodesViewModel {EmployerReference = "AUTH-KEY", SecurityToken = "AUTH-PASS"};

            // Mocks
            Mock<IScopePresentation> mockScopePresentation = AutoFacExtensions.ResolveAsMock<IScopePresentation>();
            mockScopePresentation.Setup(x => x.CreateScopingViewModelAsync(It.IsAny<EnterCodesViewModel>(), It.IsAny<User>()))
                .ReturnsAsync(new ScopingViewModel {IsSecurityCodeExpired = true});

            // Act
            var result = await controller.OutOfScope(testPostModel) as ViewResult;
            Assert.NotNull(result, "ViewResult should not be null");

            // Assert
            Assert.AreEqual("CodeExpired", result.ViewName, "Expected ViewName to be 'CodeExpired'");
        }

        [Test]
        [Description("ScopeController OutOfScope POST: When model.HasPrevScope Then Return ScopeKnown ViewResult")]
        public async Task ScopeController_OutOfScope_POST_When_HasPrevScope_Then_Return_ScopeKnown_ViewResultAsync()
        {
            // Arrange
            var testLastScopeViewModel = Mock.Of<ScopeViewModel>(svm => svm.ScopeStatus == ScopeStatuses.InScope);

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.OutOfScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(0, routeData);

            var testScopeModel = new ScopingViewModel {
                IsOutOfScopeJourney = true, IsChangeJourney = false, LastScope = testLastScopeViewModel
            };

            var testPostModel = new EnterCodesViewModel {EmployerReference = "AUTH-KEY", SecurityToken = "AUTH-PASS"};
            AutoFacExtensions.ResolveAsMock<IScopePresentation>();

            Mock<IScopePresentation> mockScopePresentation = AutoFacExtensions.ResolveAsMock<IScopePresentation>();
            mockScopePresentation.Setup(x => x.CreateScopingViewModelAsync(It.IsAny<EnterCodesViewModel>(), It.IsAny<User>()))
                .ReturnsAsync(testScopeModel);

            // Act
            var result = await controller.OutOfScope(testPostModel) as ViewResult;
            Assert.NotNull(result, "ViewResult should not be null");
            var unStashedModel = controller.UnstashModel<ScopingViewModel>();
            Assert.NotNull(unStashedModel, "Expected the Model to be stashed");

            // Assert
            Assert.AreEqual("ScopeKnown", result.ViewName, "Expected the ViewName to be 'ScopeKnown'");
        }

        [Test]
        [Description("ScopeController OutOfScope POST: When Not model.HasPrevScope Then Return ConfirmDetails Redirect")]
        public async Task ScopeController_OutOfScope_POST_When_Not_HasPrevScope_Successful_Then_Return_ConfirmDetails_RedirectAsync()
        {
            // Arrange
            var testPostModel = new EnterCodesViewModel {EmployerReference = "AUTH-KEY", SecurityToken = "AUTH-PASS"};

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.OutOfScope));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(0, routeData);
            controller.StashModel(new ScopingViewModel());

            Mock<IScopePresentation> mockScopePresentation = AutoFacExtensions.ResolveAsMock<IScopePresentation>();
            mockScopePresentation.Setup(x => x.CreateScopingViewModelAsync(It.IsAny<EnterCodesViewModel>(), It.IsAny<User>()))
                .ReturnsAsync(new ScopingViewModel {});

            // Act
            var result = await controller.OutOfScope(testPostModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("ConfirmOutOfScopeDetails", result.ActionName, "Expected the Action to be 'ConfirmOutOfScopeDetails'");
        }

        #endregion

        #region ConfirmOutOfScopeDetails

        [Test]
        [Description("ScopeController_ConfirmOutOfScopeDetails_GET_Success")]
        public void ScopeController_ConfirmOutOfScopeDetails_GET_When_CurrentUser_Is_Admin_Then_Redirect_To_Home_Admin()
        {
            // Arrange
            User adminUser = UserHelper.GetAdminUser();
            var controller = UiTestHelper.GetController<ScopeController>(-1, null, adminUser);

            // Act
            var result = controller.ConfirmOutOfScopeDetails() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("Home", result.ActionName, "Expected the Action to be 'ManageOrganisations'");
            Assert.AreEqual("Admin", result.ControllerName, "Expected the Controller to be 'Organisation'");
        }

        [Test]
        [Description(
            "ScopeController ConfirmOutOfScopeDetails GET: When Stashed Model is new ModelState() Then Return Session Expired CustomError")]
        public void ScopeController_ConfirmOutOfScopeDetails_GET_When_Stashed_Model_is_Null_Then_Return_Session_Expired_CustomError()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            // Act
            var result = controller.ConfirmOutOfScopeDetails() as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "CustomError", "Expected the ViewName to be 'CustomError'");
            Assert.AreEqual(((ErrorViewModel) result.Model).ErrorCode, 1134, "Expected title to match");
        }

        [Test]
        [Description("ScopeController ConfirmOutOfScopeDetails POST: When OutOfScope Then Return EnterAnswers Redirect")]
        public void ScopeController_ConfirmOutOfScopeDetails_POST_When_OutOfScope_Then_Return_EnterAnswers_Redirect()
        {
            // Arrange
            var testStateModel = new ScopingViewModel {IsOutOfScopeJourney = true};

            var controller = UiTestHelper.GetController<ScopeController>();
            controller.StashModel(testStateModel);

            // Act
            var result = controller.ConfirmOutOfScopeDetails() as ViewResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("ConfirmOutOfScopeDetails", result.ViewName, "Expected the Action to be 'ConfirmOutOfScopeDetails'");
        }

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description(
            "ScopeController ConfirmOutOfScopeDetails POST: When Authorised and InScope Then Generate Cookie and SaveScope overrides contact details")]
        public void
            ScopeController_ConfirmOutOfScopeDetails_POST_When_Authorised_and_InScope_Then_Generate_Cookie_and_SaveScope_Overrides_Contact_Details()
        {
            // Arrange
            var testOrgs = new[] {new Organisation {OrganisationId = 432, SectorType = SectorTypes.Private}};
            var testUserId = 135234;
            var testStateModel = new ScopingViewModel {
                //PrevOrgScopeId = 123,
                IsOutOfScopeJourney = false,
                EnterCodes = new EnterCodesViewModel {EmployerReference = "ABCDEFGH", SecurityToken = "HGFEDCBA"},
                AccountingDate = new DateTime(2017, 4, 5)
            };
            var testUsers = new[] {
                new User {
                    UserId = testUserId,
                    Firstname = "ExpectedFirstname",
                    Lastname = "ExpectedLastname",
                    EmailAddress = "ExpectedEmailAddress@domain.com",
                    JobTitle = "ExpectedJobTitle",
                    UserOrganisations = new[] {new UserOrganisation {OrganisationId = 1, UserId = testUserId}}
                }
            };

            var controller = UiTestHelper.GetController<ScopeController>(testUserId, null, testUsers, testOrgs);
            controller.StashModel(testStateModel);

            // ensure we call CreateNewScope implementation when called from the controller
            AutoFacExtensions.ResolveAsMock<IScopePresentation>(true);
            AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>(true);

            // Act
            var result = controller.ConfirmOutOfScopeDetails() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(testStateModel.EnterAnswers.FirstName, "ExpectedFirstname", "Expected the FirstName to be 'ExpectedFirstname'");
            Assert.AreEqual(testStateModel.EnterAnswers.LastName, "ExpectedLastname", "Expected the LastName to be 'ExpectedLastname'");
            Assert.AreEqual(
                testStateModel.EnterAnswers.EmailAddress,
                "ExpectedEmailAddress@domain.com",
                "Expected the EmailAddress to be 'ExpectedEmailAddress@domain.com'");
            //Assert.NotNull(testStateModel.RecentScopes.First().StatusDate, "Expected the CompletedDate not to be null");

            // saved scope
            Assert.That(testOrgs[0].OrganisationScopes.Count == 1, "OrganisationScopes should contain 1 result");

            OrganisationScope savedScope = testOrgs[0].OrganisationScopes.First();
            Assert.That(savedScope.OrganisationId == testOrgs[0].OrganisationId, "SavedScope OrganisationId does not match");
            Assert.That(savedScope.CampaignId == testStateModel.CampaignId, "SavedScope CampaignId does not match");
            Assert.That(
                savedScope.ContactEmailAddress == testStateModel.EnterAnswers.EmailAddress,
                "SavedScope ContactEmailAddress does not match");
            Assert.That(savedScope.ContactFirstname == testStateModel.EnterAnswers.FirstName, "SavedScope ContactFirstname does not match");
            Assert.That(savedScope.ContactLastname == testStateModel.EnterAnswers.LastName, "SavedScope ContactLastname does not match");
            Assert.That(savedScope.ReadGuidance == testStateModel.EnterAnswers.HasReadGuidance(), "SavedScope ReadGuidance does not match");
            Assert.That(savedScope.Reason == testStateModel.EnterAnswers.Reason, "SavedScope Reason does not match");
            Assert.That(savedScope.ScopeStatus == ScopeStatuses.InScope, "SavedScope ScopeStatus does not match");
            Assert.That(savedScope.RegisterStatus == RegisterStatuses.RegisterPending, "SavedScope RegisterStatus does not match");
            Assert.That(savedScope.SnapshotDate.ToShortDateString() == "05/04/2017", "SavedScope SnapshotDate does not match");

            // route result
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "ManageOrganisations", "Expected the Action to be 'ManageOrganisations'");
            Assert.AreEqual(result.ControllerName, "Home", "Expected the Controller to be 'Home'");

            // cookie
            /* Need to fix this
            var fastTrackCookie = controller.Response.Cookies["fasttrack"];
            Assert.NotNull(fastTrackCookie, "Expected FastTrackCookie not to be null");
            Assert.GreaterOrEqual(fastTrackCookie.Expires, VirtualDateTime.Now, "Expected FastTrackCookie expiry date to be in future");
            */
        }

        [Ignore("Needs fixing/deleting")]
        [Test]
        [Description("ConfirmDetails POST: When Not Authorised and InScope Then preserve contact details")]
        public void ConfirmDetails_POST_When_Not_Authorised_and_InScope_Then_Preserve_Contact_Details()
        {
            // Arrange
            var testOrgs = new[] {new Organisation {OrganisationId = 432, SectorType = SectorTypes.Private}};

            var testStateModel = new ScopingViewModel {
                //                PrevOrgScopeId = 123,
                IsOutOfScopeJourney = false,
                EnterCodes = new EnterCodesViewModel {EmployerReference = "ABCDEFGH", SecurityToken = "HGFEDCBA"},
                EnterAnswers = new EnterAnswersViewModel {
                    FirstName = "PreserveFirstName", LastName = "PreserveLastName", EmailAddress = "PreserveEmailAddress"
                },
                AccountingDate = new DateTime(2000, 04, 05)
            };

            var controller = UiTestHelper.GetController<ScopeController>(0, null, testOrgs);
            controller.Session[controller + ":Model"] = testStateModel;

            // ensure we call CreateNewScope implementation when called from the controller
            AutoFacExtensions.ResolveAsMock<IScopePresentation>(true);
            AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>(true);

            // Act
            var result = controller.ConfirmOutOfScopeDetails() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(testStateModel.EnterAnswers.FirstName, "PreserveFirstName", "Expected the FirstName to be 'PreserveFirstName'");
            Assert.AreEqual(testStateModel.EnterAnswers.LastName, "PreserveLastName", "Expected the LastName to be 'PreserveLastName'");
            Assert.AreEqual(
                testStateModel.EnterAnswers.EmailAddress,
                "PreserveEmailAddress",
                "Expected the EmailAddress to be 'PreserveEmailAddress'");

            // saved scope
            Assert.That(testOrgs[0].OrganisationScopes.Count == 1, "OrganisationScopes should contain 1 result");

            OrganisationScope savedScope = testOrgs[0].OrganisationScopes.First();
            Assert.That(savedScope.OrganisationId == testOrgs[0].OrganisationId, "SavedScope OrganisationId does not match");
            Assert.That(savedScope.CampaignId == testStateModel.CampaignId, "SavedScope CampaignId does not match");
            Assert.That(
                savedScope.ContactEmailAddress == testStateModel.EnterAnswers.EmailAddress,
                "SavedScope ContactEmailAddress does not match");
            Assert.That(savedScope.ContactFirstname == testStateModel.EnterAnswers.FirstName, "SavedScope ContactFirstname does not match");
            Assert.That(savedScope.ContactLastname == testStateModel.EnterAnswers.LastName, "SavedScope ContactLastname does not match");
            Assert.That(savedScope.ReadGuidance == testStateModel.EnterAnswers.HasReadGuidance(), "SavedScope ReadGuidance does not match");
            Assert.That(savedScope.Reason == testStateModel.EnterAnswers.Reason, "SavedScope Reason does not match");
            Assert.That(savedScope.ScopeStatus == ScopeStatuses.InScope, "SavedScope ScopeStatus does not match");
            Assert.That(savedScope.RegisterStatus == RegisterStatuses.RegisterPending, "SavedScope RegisterStatus does not match");
            Assert.That(savedScope.SnapshotDate.ToShortDateString() == "05/04/2000", "SavedScope SnapshotDate does not match");

            // route result
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "AboutYou", "Expected the Action to be 'AboutYou'");
            Assert.AreEqual(result.ControllerName, "Register", "Expected the Controller to be 'Register'");
        }

        #endregion

        #region ConfirmOutOfScopeAnswers

        [Test]
        [Description("ScopeController ConfirmOutOfScopeAnswers GET: When User is Admin then Redirect to Admin")]
        public void ScopeController_ConfirmOutOfScopeAnswers_GET_When_User_is_Admin_then_Redirect_to_Admin()
        {
            // Arrange
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            var controller = UiTestHelper.GetController<ScopeController>(-1, null, govEqualitiesOfficeUser);

            // Act
            var result = controller.ConfirmOutOfScopeAnswers() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "Home", "Expected the Action to be 'Home'");
            Assert.AreEqual(result.ControllerName, "Admin", "Expected the Controller to be 'Admin'");
        }

        [Test]
        [Description(
            "ScopeController ConfirmOutOfScopeAnswers GET: When Stashed Model is new ModelState() Then Return Session Expired CustomError")]
        public void ScopeController_ConfirmOutOfScopeAnswers_GET_When_Stashed_Model_is_Null_Then_Return_Session_Expired_CustomError()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            // Act
            var result = controller.ConfirmOutOfScopeAnswers() as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "CustomError", "Expected the ViewName to be 'CustomError'");
            Assert.AreEqual(((ErrorViewModel) result.Model).ErrorCode, 1134, "Expected title to match");
        }

        [Test]
        [Description(
            "ScopeController ConfirmOutOfScopeAnswers GET When CurrentUser Is new ModelState() Then Redirect To ManageOrganisations")]
        public void ScopeController_ConfirmOutOfScopeAnswers_GET_When_CurrentUser_Is_Null_Then_Redirect_To_ManageOrganisations()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>(99, null, null);
            controller.StashModel(new ScopingViewModel());

            // Act
            var result = controller.ConfirmOutOfScopeAnswers() as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "ConfirmOutOfScopeAnswers", "Expected the ViewName to be 'CustomError'");
        }

        [Test]
        [Description("ScopeController ConfirmOutOfScopeAnswers POST: When User is Admin then Redirect to Admin")]
        public async Task ScopeController_ConfirmOutOfScopeAnswers_POST_When_User_is_Admin_then_Redirect_to_AdminAsync()
        {
            // Arrange
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            var controller = UiTestHelper.GetController<ScopeController>(-1, null, govEqualitiesOfficeUser);

            // Act
            var result = await controller.ConfirmOutOfScopeAnswers(string.Empty) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "Home", "Expected the Action to be 'Home'");
            Assert.AreEqual(result.ControllerName, "Admin", "Expected the Controller to be 'Admin'");
        }

        [Test]
        [Description(
            "ScopeController ConfirmOutOfScopeAnswers POST: When Stashed Model is new ModelState() Then Return Session Expired CustomError")]
        public async Task
            ScopeController_ConfirmOutOfScopeAnswers_POST_When_Stashed_Model_is_Null_Then_Return_Session_Expired_CustomErrorAsync()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            // Act
            var result = await controller.ConfirmOutOfScopeAnswers(string.Empty) as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "CustomError", "Expected the ViewName to be 'CustomError'");
            Assert.AreEqual(((ErrorViewModel) result.Model).ErrorCode, 1134, "Expected title to match");
        }

        [Test]
        [Description("ScopeController ConfirmOutOfScopeAnswers POST: When Authorised Then SaveScope overrides contact details")]
        public async Task ScopeController_ConfirmOutOfScopeAnswers_POST_When_Authorised_Then_SaveScope_Overrides_Contact_DetailsAsync()
        {
            // Arrange
            User mockUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();

            var mockOrg = new Organisation {SectorType = SectorTypes.Private};
            var mockUserOrg = new UserOrganisation {Organisation = mockOrg, User = mockUser};

            var controller = UiTestHelper.GetController<ScopeController>(-1, null, mockUser, mockOrg, mockUserOrg);

            var testStateModel = new ScopingViewModel {
                OrganisationId = mockOrg.OrganisationId,
                IsOutOfScopeJourney = false,
                EnterCodes = new EnterCodesViewModel {EmployerReference = "ABCDEFGH", SecurityToken = "HGFEDCBA"}
            };

            controller.Session[controller + ":Model"] = testStateModel;

            AutoFacExtensions.ResolveAsMock<IScopePresentation>(true);

            AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>(true);
            AutoFacExtensions.ResolveAsMock<IScopePresentation>(true);
            AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>(true);

            // Act
            var result = await controller.ConfirmOutOfScopeAnswers(string.Empty) as RedirectToActionResult;
            testStateModel = controller.Session.Get<ScopingViewModel>(controller + ":Model");

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(mockUser.Firstname, testStateModel.EnterAnswers.FirstName, "Expected the FirstName to be 'ExpectedFirstname'");
            Assert.AreEqual(mockUser.Lastname, testStateModel.EnterAnswers.LastName, "Expected the LastName to be 'ExpectedLastname'");
            Assert.AreEqual(
                mockUser.EmailAddress,
                testStateModel.EnterAnswers.EmailAddress,
                "Expected the EmailAddress to be 'ExpectedEmailAddress'");

            // route result
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "FinishOutOfScope", "Expected the Action to be 'FinishOutOfScope'");
            Assert.AreEqual(result.ControllerName, "Scope", "Expected the Controller to be 'Scope'");
        }

        [Test]
        [Description(
            "ScopeController ConfirmOutOfScopeAnswers POST: When Stashed Model Can Be Unstashed Then Return ConfirmOutOfScopeAnswers ViewResult")]
        public async Task
            ScopeController_ConfirmOutOfScopeAnswers_POST_When_Stashed_Model_Can_Be_Unstashed_Then_Return_ConfirmOutOfScopeAnswers_ViewResultAsync()
        {
            // Arrange
            User user = CreateUser(1, "user@test.com");
            var organisationId = 300;
            Organisation organisation = createPrivateOrganisation(organisationId, "Company1", 12345678);
            UserOrganisation userOrganisation = CreateUserOrganisation(organisation, user.UserId, VirtualDateTime.Now);

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmOutOfScopeAnswers));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(
                user.UserId,
                routeData,
                user,
                organisation,
                userOrganisation);

            ScopingViewModel scopingViewModel = ScopingViewModelHelper.GetScopingViewModel();
            // organisationId is getting overriden somewhere and so this is a hack to keep them the same
            scopingViewModel.OrganisationId = organisationId;
            controller.StashModel(scopingViewModel);

            // Act
            var result = await controller.ConfirmOutOfScopeAnswers(string.Empty) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("FinishOutOfScope", result.ActionName, "Expected the Action to be 'FinishOutOfScope'");
            Assert.AreEqual("Scope", result.ControllerName, "Expected the Controller to be 'Scope'");
        }

        [Test]
        [Description("ScopeController ConfirmOutOfScopeAnswers POST: When Scope is Changed to Out For Current Year Then Email Users")]
        public async Task ScopeController_ConfirmOutOfScopeAnswers_POST_When_Scope_is_Changed_to_Out_For_Current_Year_Then_Email_Users()
        {
            // Arrange
            User user1 = CreateUser(1, "user1@test.com");
            User user2 = CreateUser(2, "user2@test.com");
            var organisationId = 100;
            Organisation organisation = createPrivateOrganisation(organisationId, "Company1", 12345678);
            UserOrganisation userOrganisation1 = CreateUserOrganisation(organisation, user1.UserId, VirtualDateTime.Now);
            UserOrganisation userOrganisation2 = CreateUserOrganisation(organisation, user2.UserId, VirtualDateTime.Now);

            Organisation otherOrganisation = createPrivateOrganisation(200, "Company2", 12341234);
            User user3 = CreateUser(3, "user3@test.com");
            UserOrganisation userOrganisation3 = CreateUserOrganisation(otherOrganisation, user3.UserId, VirtualDateTime.Now);

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmOutOfScopeAnswers));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(
                user1.UserId,
                routeData,
                user1,
                user2,
                user3,
                organisation,
                otherOrganisation,
                userOrganisation1,
                userOrganisation2,
                userOrganisation3);

            ScopingViewModel scopingViewModel = ScopingViewModelHelper.GetScopingViewModel();
            // organisationId is getting overriden somewhere and so this is a hack to keep them the same
            scopingViewModel.OrganisationId = organisationId;

            // this may well break at some point in future
            scopingViewModel.AccountingDate = new DateTime(2022, 4, 05);

            controller.StashModel(scopingViewModel);

            var mockEmailQueue = new Mock<IQueue>();
            Program.MvcApplication.SendNotifyEmailQueue = mockEmailQueue.Object;
            mockEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<NotifyEmail>()));

            // Act
            var result = await controller.ConfirmOutOfScopeAnswers(string.Empty) as RedirectToActionResult;

            // Assert
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user1.EmailAddress))),
                Times.Once(),
                "Expected the current user's email address to be in the email send queue");
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.TemplateId.Contains(EmailTemplates.ScopeChangeOutEmail))),
                Times.Exactly(2),
                $"Expected the correct templateId to be in the email send queue, expected {EmailTemplates.ScopeChangeOutEmail}");
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user2.EmailAddress))),
                Times.Once(),
                "Expected the other user of the same organisation's email address to be in the email send queue");
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user3.EmailAddress))),
                Times.Never,
                "Do not expect users of other organisations to be in the email send queue");
        }

        [Test]
        [Description("ScopeController ConfirmOutOfScopeAnswers POST When Scope is Changed to Out For Previous Years Then Dont Email Users")]
        public async Task
            ScopeController_ConfirmOutOfScopeAnswers_POST_When_Scope_is_Changed_to_Out_For_Previous_Years_Then_Dont_Email_Users()
        {
            // Arrange
            User user1 = CreateUser(1, "user1@test.com");
            User user2 = CreateUser(2, "user2@test.com");
            var organisationId = 101;
            Organisation organisation = createPrivateOrganisation(organisationId, "Company1", 12345678);
            UserOrganisation userOrganisation1 = CreateUserOrganisation(organisation, user1.UserId, VirtualDateTime.Now);
            UserOrganisation userOrganisation2 = CreateUserOrganisation(organisation, user2.UserId, VirtualDateTime.Now);

            User user3 = CreateUser(3, "user3@test.com");
            Organisation organisation2 = createPrivateOrganisation(201, "Company2", 12341234);
            UserOrganisation userOrganisation3 = CreateUserOrganisation(organisation2, user3.UserId, VirtualDateTime.Now);

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(ScopeController.ConfirmOutOfScopeAnswers));
            routeData.Values.Add("Controller", "Scope");

            var controller = UiTestHelper.GetController<ScopeController>(
                user1.UserId,
                routeData,
                user1,
                user2,
                user3,
                organisation,
                organisation2,
                userOrganisation1,
                userOrganisation2,
                userOrganisation3);

            ScopingViewModel scopingViewModel = ScopingViewModelHelper.GetScopingViewModel();
            // organisationId is getting overriden somewhere and so this is a hack to keep them the same
            scopingViewModel.OrganisationId = organisationId;

            // this may well break at some point in future
            scopingViewModel.AccountingDate = new DateTime(2016, 4, 05);

            controller.StashModel(scopingViewModel);

            var mockEmailQueue = new Mock<IQueue>();
            Program.MvcApplication.SendNotifyEmailQueue = mockEmailQueue.Object;
            mockEmailQueue
                .Setup(q => q.AddMessageAsync(It.IsAny<NotifyEmail>()));

            // Act
            var result = await controller.ConfirmOutOfScopeAnswers(string.Empty) as RedirectToActionResult;

            // Assert
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user1.EmailAddress))),
                Times.Never(),
                "Do not expect the current user's email address to be in the email send queue");
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user2.EmailAddress))),
                Times.Never(),
                "Do not expect the other user of the same organisation's email address to be in the email send queue");
            mockEmailQueue.Verify(
                x => x.AddMessageAsync(It.Is<NotifyEmail>(inst => inst.EmailAddress.Contains(user3.EmailAddress))),
                Times.Never,
                "Do not expect users of other organisations to be in the email send queue");
        }

        #endregion

        #region GET RegisterOrManage

        [Test]
        [Description("ScopeController RegisterOrManage GET: When User is Admin then Redirect to Admin")]
        public void ScopeController_RegisterOrManage_GET_When_User_is_Admin_then_Redirect_to_Admin()
        {
            // Arrange
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            var controller = UiTestHelper.GetController<ScopeController>(-1, null, govEqualitiesOfficeUser);

            // Act
            var result = controller.RegisterOrManage() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "Home", "Expected the Action to be 'Home'");
            Assert.AreEqual(result.ControllerName, "Admin", "Expected the Controller to be 'Admin'");
        }

        [Test]
        [Description(
            "ScopeController RegisterOrManage GET: When Stashed Model is new ModelState() Then Return Session Expired CustomError")]
        public void ScopeController_RegisterOrManage_GET_When_Stashed_Model_is_Null_Then_Return_Session_Expired_CustomError()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            // Act
            var result = controller.RegisterOrManage() as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "CustomError", "Expected the ViewName to be 'CustomError'");
            Assert.AreEqual(((ErrorViewModel) result.Model).ErrorCode, 1134, "Expected title to match");
        }

        [Test]
        [Description("ScopeController RegisterOrManage GET: When User is Admin then Redirect to Admin")]
        public void ScopeController_RegisterOrManage_GET_When_User_is_Already_Registered_then_Redirect_to_Admin()
        {
            // Arrange
            ScopingViewModel mockedScopingViewModel = ScopingViewModelHelper.GetScopingViewModel();
            mockedScopingViewModel.UserIsRegistered = true;
            mockedScopingViewModel.OrganisationId = 999;
            var controller = UiTestHelper.GetController<ScopeController>();
            controller.StashModel(mockedScopingViewModel);

            // Act
            var result = controller.RegisterOrManage() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("ManageOrganisation", result.ActionName, "Expected the Action to be 'ManageOrganisation'");
            Assert.AreEqual("Organisation", result.ControllerName, "Expected the Controller to be 'Organisation'");
            Assert.AreEqual(",EIovIc8j9_CruUyYT3fa6w!!", result.RouteValues["id"], "Expected id to contain an encrypted value");
        }

        [TestCase("employerReference", "securityToken", "firstName", "lastName", "email@address.com")]
        [Description("ScopeController RegisterOrManage GET: When User is Admin then Redirect to Admin")]
        public void
            ScopeController_RegisterOrManage_GET_When_User_is_Not_Registered_then_Populate_PendingFasttrackCodes_And_Redirect_to_ManageOrganisations(
                string employerReference,
                string securityToken,
                string firstName,
                string lastname,
                string emailAddress)
        {
            // Arrange
            var mockedEnterCodesViewModel = Mock.Of<EnterCodesViewModel>(
                codesViewModel => codesViewModel.EmployerReference == employerReference && codesViewModel.SecurityToken == securityToken);

            var mockedEnterAnswersViewModel = Mock.Of<EnterAnswersViewModel>(
                answersViewModel => answersViewModel.FirstName == firstName
                                    && answersViewModel.LastName == lastname
                                    && answersViewModel.EmailAddress == emailAddress);

            var mockedScopingViewModel = Mock.Of<ScopingViewModel>(
                scopingViewModel => scopingViewModel.UserIsRegistered == false
                                    && scopingViewModel.EnterCodes == mockedEnterCodesViewModel
                                    && scopingViewModel.EnterAnswers == mockedEnterAnswersViewModel);
            var controller = UiTestHelper.GetController<ScopeController>();
            controller.StashModel(mockedScopingViewModel);

            // Act
            var result = controller.RegisterOrManage() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual("ManageOrganisations", result.ActionName, "Expected the Action to be 'ManageOrganisation'");
            Assert.AreEqual("Organisation", result.ControllerName, "Expected the Controller to be 'Organisation'");
            Assert.AreEqual("employerReference:securityToken:firstName:lastName:email@address.com", controller.PendingFasttrackCodes);
        }

        #endregion

        #region EnterOutOfScopeAnswers

        [Test]
        [Description("ScopeController EnterOutOfScopeAnswers GET: When User is Admin then Redirect to Admin")]
        public void ScopeController_EnterOutOfScopeAnswers_GET_When_User_is_Admin_then_Redirect_to_Admin()
        {
            // Arrange
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            var controller = UiTestHelper.GetController<ScopeController>(-1, null, govEqualitiesOfficeUser);

            // Act
            var result = controller.EnterOutOfScopeAnswers() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "Home", "Expected the Action to be 'Home'");
            Assert.AreEqual(result.ControllerName, "Admin", "Expected the Controller to be 'Admin'");
        }

        [Test]
        [Description(
            "ScopeController EnterOutOfScopeAnswers GET: When Stashed Model is new ModelState() Then Return Session Expired CustomError")]
        public void ScopeController_EnterOutOfScopeAnswers_GET_When_Stashed_Model_is_Null_Then_Return_Session_Expired_CustomError()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            // Act
            var result = controller.EnterOutOfScopeAnswers() as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "CustomError", "Expected the ViewName to be 'CustomError'");
            Assert.AreEqual(((ErrorViewModel) result.Model).ErrorCode, 1134, "Expected title to match");
        }

        [Test]
        [Description(
            "ScopeController EnterOutOfScopeAnswers GET When CurrentUser Is new ModelState() Then Redirect To ManageOrganisations")]
        public void ScopeController_EnterOutOfScopeAnswers_GET_When_CurrentUser_Is_Null_Then_Redirect_To_ManageOrganisations()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>(99, null, null);
            controller.StashModel(new ScopingViewModel());

            // Act
            var result = controller.EnterOutOfScopeAnswers() as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "EnterOutOfScopeAnswers", "Expected the ViewName to be 'CustomError'");
        }

        [Test]
        [Description("ScopeController EnterOutOfScopeAnswers POST: When User is Admin then Redirect to Admin")]
        public void ScopeController_EnterOutOfScopeAnswers_POST_When_User_is_Admin_then_Redirect_to_Admin()
        {
            // Arrange
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            var controller = UiTestHelper.GetController<ScopeController>(-1, null, govEqualitiesOfficeUser);

            // Act
            var result = controller.EnterOutOfScopeAnswers(new EnterAnswersViewModel()) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "Home", "Expected the Action to be 'Home'");
            Assert.AreEqual(result.ControllerName, "Admin", "Expected the Controller to be 'Admin'");
        }

        [Test]
        [Description(
            "ScopeController EnterOutOfScopeAnswers POST: When Stashed Model is new ModelState() Then Return Session Expired CustomError")]
        public void ScopeController_EnterOutOfScopeAnswers_POST_When_Stashed_Model_is_Null_Then_Return_Session_Expired_CustomError()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            // Act
            var result = controller.EnterOutOfScopeAnswers(new EnterAnswersViewModel()) as ViewResult;
            Assert.NotNull(result, "ViewResult should not be null");

            var errorViewModel = result.Model as ErrorViewModel;
            Assert.NotNull(errorViewModel, "ErrorViewModel should not be null");

            // Assert
            Assert.AreEqual(result.ViewName, "CustomError", "Expected the ViewName to be 'CustomError'");
            Assert.AreEqual("Session has expired", errorViewModel.Title);
            Assert.AreEqual("Your session has expired.", errorViewModel.Description);
            Assert.AreEqual("Continue", errorViewModel.ActionText);
            Assert.AreEqual(1134, errorViewModel.ErrorCode);
        }

        [Test]
        [Description("ScopeController EnterOutOfScopeAnswers POST EnterAnswersViewModel Must Be Inserted into ScopingViewModel")]
        public void ScopeController_EnterOutOfScopeAnswers_POST_EnterAnswersViewModel_Must_Be_Inserted_into_ScopingViewModel()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();
            controller.StashModel(new ScopingViewModel());

            // Act
            var result = controller.EnterOutOfScopeAnswers(new EnterAnswersViewModel()) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            // ConfirmOutOfScopeAnswers inside method EnterOutOfScopeAnswers - this isn't a mistake!!
            Assert.AreEqual("ConfirmOutOfScopeAnswers", result.ActionName, "Expected the Action to be 'ConfirmOutOfScopeAnswers'");

            var unstashedModel = controller.UnstashModel<ScopingViewModel>();
            Assert.NotNull(unstashedModel, "ScopingViewModel should have been stashed");
            Assert.NotNull(unstashedModel.EnterAnswers, "Answers view model should have been added to the scoping view model");
        }

        [Test]
        [Description("ScopeController EnterOutOfScopeAnswers POST OtherReason Is Removed From The ModelState")]
        public void ScopeController_EnterOutOfScopeAnswers_POST_OtherReason_Is_Removed_From_The_ModelState()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            controller.ModelState.AddModelError(nameof(EnterAnswersViewModel.OtherReason), "OTHERREASON ERROR");

            controller.StashModel(new ScopingViewModel());

            // Act
            var result = controller.EnterOutOfScopeAnswers(new EnterAnswersViewModel()) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            // ConfirmOutOfScopeAnswers inside method EnterOutOfScopeAnswers - this isn't a mistake!!
            Assert.AreEqual("ConfirmOutOfScopeAnswers", result.ActionName, "Expected the Action to be 'ConfirmOutOfScopeAnswers'");
            Assert.IsFalse(
                controller.ModelState.ContainsKey(nameof(EnterAnswersViewModel.OtherReason)),
                "Expected OtherReason to be removed from the StateModel");
        }

        [Test]
        [Description("ScopeController EnterOutOfScopeAnswers POST OtherReason Is Kept in the ModelState")]
        public void ScopeController_EnterOutOfScopeAnswers_POST_OtherReason_Is_Kept_In_The_ModelState()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            controller.ModelState.AddModelError(nameof(EnterAnswersViewModel.OtherReason), "OTHERREASON ERROR");

            controller.StashModel(new ScopingViewModel());

            var enterAnswersViewModel = new EnterAnswersViewModel {Reason = "Other"};

            // Act
            var result = controller.EnterOutOfScopeAnswers(enterAnswersViewModel) as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            // ConfirmOutOfScopeAnswers inside method EnterOutOfScopeAnswers - this isn't a mistake!!
            Assert.AreEqual("EnterOutOfScopeAnswers", result.ViewName, "Expected the View to be 'EnterOutOfScopeAnswers'");
            Assert.IsTrue(
                controller.ModelState.ContainsKey(nameof(EnterAnswersViewModel.OtherReason)),
                "Expected OtherReason to be available in the StateModel");
        }

        [Test]
        [Description("ScopeController EnterOutOfScopeAnswers POST OtherReason Is Removed From The ModelState")]
        public void
            ScopeController_EnterOutOfScopeAnswers_POST_When_CurrentUser_Is_Null_Then_FirstName_LastName_And_Email_Are_Removed_From_The_ModelState()
        {
            // Arrange
            User currentUser = UserHelper.GetNotAdminUserWithoutVerifiedEmailAddress();
            var controller = UiTestHelper.GetController<ScopeController>(-1, null, currentUser);

            controller.ModelState.AddModelError(nameof(EnterAnswersViewModel.FirstName), "FIRSTNAME ERROR");
            controller.ModelState.AddModelError(nameof(EnterAnswersViewModel.LastName), "LASTNAME ERROR");
            controller.ModelState.AddModelError(nameof(EnterAnswersViewModel.EmailAddress), "EMAIL ERROR");

            controller.StashModel(new ScopingViewModel());

            // Act
            var result = controller.EnterOutOfScopeAnswers(new EnterAnswersViewModel()) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.IsFalse(
                controller.ModelState.ContainsKey(nameof(EnterAnswersViewModel.FirstName)),
                "Expected FirstName to be removed from the StateModel");
            Assert.IsFalse(
                controller.ModelState.ContainsKey(nameof(EnterAnswersViewModel.LastName)),
                "Expected LastName to be removed from the StateModel");
            Assert.IsFalse(
                controller.ModelState.ContainsKey(nameof(EnterAnswersViewModel.EmailAddress)),
                "Expected EmailAddress to be removed from the StateModel");
        }

        [Test]
        [Description("ScopeController EnterOutOfScopeAnswers POST OtherReason Is Removed From The ModelState")]
        public void
            ScopeController_EnterOutOfScopeAnswers_POST_When_User_Is_Not_Logged_In_Then_FirstName_LastName_And_Email_Are_Removed_From_The_ModelState()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            controller.ModelState.AddModelError(nameof(EnterAnswersViewModel.FirstName), "FIRSTNAME ERROR");
            controller.ModelState.AddModelError(nameof(EnterAnswersViewModel.LastName), "LASTNAME ERROR");
            controller.ModelState.AddModelError(nameof(EnterAnswersViewModel.EmailAddress), "EMAIL ERROR");

            controller.StashModel(new ScopingViewModel());

            // Act
            var result = controller.EnterOutOfScopeAnswers(new EnterAnswersViewModel()) as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual("EnterOutOfScopeAnswers", result.ViewName, "Expected the View to be 'EnterOutOfScopeAnswers'");
            Assert.IsTrue(
                controller.ModelState.ContainsKey(nameof(EnterAnswersViewModel.FirstName)),
                "Expected FirstName to be removed from the StateModel");
            Assert.IsTrue(
                controller.ModelState.ContainsKey(nameof(EnterAnswersViewModel.LastName)),
                "Expected LastName to be removed from the StateModel");
            Assert.IsTrue(
                controller.ModelState.ContainsKey(nameof(EnterAnswersViewModel.EmailAddress)),
                "Expected EmailAddress to be removed from the StateModel");
        }

        [Test]
        [Description("ScopeController EnterOutOfScopeAnswers POST Email Is Changed To Lower Case")]
        public void ScopeController_EnterOutOfScopeAnswers_POST_Email_Is_Changed_To_Lower_Case()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();
            controller.StashModel(new ScopingViewModel());

            var enterAnswersViewModel = new EnterAnswersViewModel {
                Reason = "Other", EmailAddress = "EMAILINUPPERCASETOBECHANGEDTOLOWERCASE@EMAIL.COM"
            };

            // Act
            var result = controller.EnterOutOfScopeAnswers(enterAnswersViewModel) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(
                "emailinuppercasetobechangedtolowercase@email.com",
                enterAnswersViewModel.EmailAddress,
                "Expected OtherReason to be removed from the StateModel");
        }

        [Test]
        [Description("ScopeController_EnterOutOfScopeAnswers_POST_When_ModelState_Is_Not_Valid_Then_Blah")]
        public void ScopeController_EnterOutOfScopeAnswers_POST_When_ModelState_Is_Not_Valid_Then_Blah()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            controller.ModelState.AddModelError(nameof(EnterAnswersViewModel.FirstName), "FIRSTNAME ERROR");

            controller.StashModel(new ScopingViewModel());

            var enterAnswersViewModel = new EnterAnswersViewModel {Reason = "Other"};

            // Act
            var result = controller.EnterOutOfScopeAnswers(enterAnswersViewModel) as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "EnterOutOfScopeAnswers", "Expected the ViewName to be 'CustomError'");
            Assert.NotNull(result.Model, "The model must not be null");
        }

        #endregion

        #region DeclareScope

        #endregion

        #region FinishOutOfScope

        [Test]
        [Description("ScopeController FinishOutOfScope GET: When User is Admin then Redirect to Admin")]
        public void ScopeController_FinishOutOfScope_GET_When_User_is_Admin_then_Redirect_to_Admin()
        {
            // Arrange
            User govEqualitiesOfficeUser = UserHelper.GetGovEqualitiesOfficeUser();
            var controller = UiTestHelper.GetController<ScopeController>(-1, null, govEqualitiesOfficeUser);

            // Act
            var result = controller.FinishOutOfScope() as RedirectToActionResult;

            // Assert
            Assert.NotNull(result, "RedirectToActionResult should not be null");
            Assert.AreEqual(result.ActionName, "Home", "Expected the Action to be 'Home'");
            Assert.AreEqual(result.ControllerName, "Admin", "Expected the Controller to be 'Admin'");
        }

        [Test]
        [Description(
            "ScopeController FinishOutOfScope GET: When Stashed Model is new ModelState() Then Return Session Expired CustomError")]
        public void ScopeController_FinishOutOfScope_GET_When_Stashed_Model_is_Null_Then_Return_Session_Expired_CustomError()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>();

            // Act
            var result = controller.FinishOutOfScope() as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "CustomError", "Expected the ViewName to be 'CustomError'");
            Assert.AreEqual(((ErrorViewModel) result.Model).ErrorCode, 1134, "Expected title to match");
        }

        [Test]
        [Description("ScopeController FinishOutOfScope GET When CurrentUser Is new ModelState() Then Redirect To ManageOrganisations")]
        public void ScopeController_FinishOutOfScope_GET_When_CurrentUser_Is_Null_Then_Redirect_To_ManageOrganisations()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ScopeController>(99, null, null);
            controller.StashModel(new ScopingViewModel());

            // Act
            var result = controller.FinishOutOfScope() as ViewResult;

            // Assert
            Assert.NotNull(result, "ViewResult should not be null");
            Assert.AreEqual(result.ViewName, "FinishOutOfScope", "Expected the ViewName to be 'CustomError'");
        }

        #endregion

    }

}
