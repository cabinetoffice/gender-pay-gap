using System.Net;
using System.Threading.Tasks;
using GenderPayGap.BusinessLogic;
using GenderPayGap.Core;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Extensions;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Models.Organisation;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers
{

    public partial class OrganisationControllerTests
    {

        [Test]
        [Description("ManageOrganisation GET: When Id IsNullOrWhitespace Then Return BadRequest")]
        public async Task ManageOrganisation_GET_When_Id_IsNullOrWhitespace_Then_Return_BadRequestAsync()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ManageOrganisation");
            mockRouteData.Values.Add("Controller", "Organisation");

            var controller = UiTestHelper.GetController<OrganisationController>(
                MockUsers[0].UserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);
            var testOrgId = "";

            // Act
            IActionResult actionResult = await controller.ManageOrganisation(testOrgId);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.BadRequest, "Expected the StatusCode to be a 'BadRequest'");
        }

        [Test]
        [Description("ManageOrganisation GET: When Org created in current year has no previos years scope redirect to DeclareScope")]
        public async Task ManageOrganisation_GET_When_NewOrg_PresumedScope_Then_RedirectToDeclareScope()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ManageOrganisation");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 2;
            var testOrgId = 123;

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);

            //Mock an existing explicit scope to prevent redirect to DeclareScope page
            Mock<IScopeBusinessLogic> mockScopeBL = AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>();
            mockScopeBL.Setup(x => x.GetLatestScopeStatusForSnapshotYearAsync(It.IsAny<long>(), It.IsAny<int>()))
                .ReturnsAsync(ScopeStatuses.PresumedInScope);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());

            // Act
            var actionResult = await controller.ManageOrganisation(encOrgId) as RedirectToActionResult;

            // Assert
            Assert.IsNotNull(actionResult, "actionResult should not be null");

            Assert.AreEqual(actionResult.ActionName, "DeclareScope", "Expected redirect to DeclareScope");
            Assert.AreEqual(actionResult.RouteValues["id"], encOrgId, "Wrong organisation id returned");
        }

        [Test]
        [Description("ManageOrganisation GET: When Org has Multiple Users Then Model Contains Assoc Users")]
        public async Task ManageOrganisation_GET_When_Org_has_Multiple_Users_Then_Model_Contains_Assoc_Users()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ManageOrganisation");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 2;
            var testOrgId = 123;

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);

            //Mock an existing explicit scope to prevent redirect to DeclareScope page
            Mock<IScopeBusinessLogic> mockScopeBl = AutoFacExtensions.ResolveAsMock<IScopeBusinessLogic>();
            mockScopeBl.Setup(x => x.GetLatestScopeStatusForSnapshotYearAsync(It.IsAny<long>(), It.IsAny<int>()))
                .ReturnsAsync(ScopeStatuses.InScope);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());

            // Act
            IActionResult actionResult = await controller.ManageOrganisation(encOrgId);

            // Assert
            var viewResult = actionResult as ViewResult;
            Assert.NotNull(viewResult, "viewResult should not be null");

            var model = (ManageOrganisationModel) viewResult.Model;
            Assert.NotNull(model, "model should not be null");
            Assert.NotNull(model.AssociatedUserOrgs, "AssociatedUserOrgs should not be null");
            Assert.AreEqual(model.AssociatedUserOrgs.Count, 1, "AssociatedUserOrgs should contain 1 user org");
            Assert.AreEqual(model.AssociatedUserOrgs[0], MockUserOrganisations[0], "Expected the User org to match");
        }

        [Test]
        [Description("ManageOrganisation GET: When User Not Assoc to Org Then Return Forbidden")]
        public async Task ManageOrganisation_GET_When_User_Not_Assoc_to_Org_Then_Return_ForbiddenAsync()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "ManageOrganisation");
            mockRouteData.Values.Add("Controller", "Organisation");

            var testUserId = 4;
            var testOrgId = 123;

            var controller = UiTestHelper.GetController<OrganisationController>(
                testUserId,
                mockRouteData,
                MockUsers,
                MockUserOrganisations);

            string encOrgId = Encryption.EncryptQuerystring(testOrgId.ToString());

            // Act
            IActionResult actionResult = await controller.ManageOrganisation(encOrgId);

            // Assert
            var httpStatusResult = actionResult as HttpStatusViewResult;
            Assert.NotNull(httpStatusResult, "httpStatusResult should not be null");
            Assert.AreEqual(httpStatusResult.StatusCode, (int) HttpStatusCode.Forbidden, "Expected the StatusCode to be a 'Forbidden'");
        }

    }

}
