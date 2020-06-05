using System;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Register;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Registration
{
    [TestFixture]
    public partial class RegisterControllerTests
    {

        [Test]
        [Description("Check registration completes successfully when correct pin entered ")]
        public async Task RegisterController_ActivateService_POST_CorrectPIN_ServiceActivated()
        {
            //ARRANGE:
            //create a user who does exist in the db
            var user = new User {UserId = 1, EmailAddress = "test@hotmail.com", EmailVerifiedDate = VirtualDateTime.Now};
            var org = new Organisation {OrganisationId = 1, SectorType = SectorTypes.Private, Status = OrganisationStatuses.Pending};

            //TODO: Refactoring to user the same Helpers (ie AddScopeStatus.AddScopeStatus)
            org.OrganisationScopes.Add(
                new OrganisationScope {
                    Organisation = org,
                    ScopeStatus = ScopeStatuses.InScope,
                    SnapshotDate = org.SectorType.GetAccountingStartDate(VirtualDateTime.Now.Year),
                    Status = ScopeRowStatuses.Active
                });

            var address = new OrganisationAddress {AddressId = 1, OrganisationId = 1, Organisation = org, Status = AddressStatuses.Pending};
            var pin = "ASDFG";
            var userOrg = new UserOrganisation {
                UserId = 1,
                OrganisationId = 1,
                PINSentDate = VirtualDateTime.Now,
                PIN = pin,
                AddressId = address.AddressId,
                Address = address
            };

            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(RegisterController.ActivateService));
            routeData.Values.Add("Controller", "Register");

            var controller = UiTestHelper.GetController<RegisterController>(user.UserId, routeData, user, org, address, userOrg);
            controller.ReportingOrganisationId = org.OrganisationId;

            var model = new CompleteViewModel {PIN = pin};

            //ACT:
            var result = await controller.ActivateService(model) as RedirectToActionResult;

            //ASSERT:
            Assert.That(result != null, "Expected RedirectToActionResult");
            Assert.That(result.ActionName == "ServiceActivated", "Expected redirect to ServiceActivated");
            Assert.That(userOrg.PINConfirmedDate > DateTime.MinValue);
            Assert.That(userOrg.Organisation.Status == OrganisationStatuses.Active);
            Assert.That(userOrg.Organisation.GetLatestAddress().AddressId == address.AddressId);
            Assert.That(address.Status == AddressStatuses.Active);
            
        }

        [Test]
        [Ignore("Needs fixing/deleting")]
        [Description("RegisterController.ServiceActivated GET: When OrgScope is Not Null Then Return Expected ViewData")]
        public void RegisterController_ServiceActivated_GET_When_OrgScope_is_Not_Null_Then_Return_Expected_ViewData()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ServiceActivated");
            mockRouteData.Values.Add("Controller", "Register");

            var mockOrg = new Organisation {
                OrganisationId = 52425, SectorType = SectorTypes.Private, OrganisationName = "Mock Organisation Ltd"
            };

            var mockUser = new User {UserId = 87654, EmailAddress = "mock@test.com", EmailVerifiedDate = VirtualDateTime.Now};

            var mockReg = new UserOrganisation {UserId = 87654, OrganisationId = 52425, PINConfirmedDate = VirtualDateTime.Now};

            var controller = UiTestHelper.GetController<RegisterController>(87654, mockRouteData, mockUser, mockOrg, mockReg);
            controller.ReportingOrganisationId = mockOrg.OrganisationId;

            var testUri = new Uri("https://localhost/register/activate-service");
            controller.AddMockUriHelper(testUri.ToString(), "ActivateService");

            //Mock the Referrer
            controller.Request.Headers["Referer"] = testUri.ToString();

            // Act
            var viewResult = controller.ServiceActivated() as ViewResult;

            // Assert
            Assert.NotNull(viewResult, "ViewResult should not be null");
            Assert.AreEqual(viewResult.ViewName, "ServiceActivated", "Expected the ViewName to be 'ServiceActivated'");

            // Assert ViewData
            Assert.That(controller.ViewBag.OrganisationName == mockOrg.OrganisationName, "Expected OrganisationName");
        }

    }
}
