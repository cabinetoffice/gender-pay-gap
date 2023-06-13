using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Helpers;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.BusinessLogic.Models.Compare;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Models.Search;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers
{
    public class CompareControllerTests
    {

        #region AddEmployer
        [Test]
        public void CompareController_AddEmployer_NoEmployerId_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = null;
            string returnUrl = @"\viewing\search-results";

            // Act
            var result = controller.AddEmployer(employerIdentifier, returnUrl) as HttpStatusViewResult;
            
            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(employerIdentifier)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);

        }

        [Test]
        public void CompareController_AddEmployer_NoReturnUrl_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = null;

            // Act
            var result = controller.AddEmployer(employerIdentifier, returnUrl) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(returnUrl)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployer_NoLastSearch_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.AddEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployer_NoEmployers_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.AddEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployer_NoResults_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.AddEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployer_WrongEmployer_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.AddEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);

        }

        [Test]
        public void CompareController_AddEmployer_Success_RedirectToReturnUrl()
        {
            // Arrange
            var organisation = new Organisation {OrganisationId = 123, Status = OrganisationStatuses.Active, SectorType = SectorTypes.Private};
            var controller = UiTestHelper.GetController<CompareController>(dbObjects: new object[] {organisation});
            long organisationId = 123;
            var expectedObfuscatedOrganisationId = ViewingControllerTests.ConfigureObfuscator(organisationId);
            var employerIdentifier = expectedObfuscatedOrganisationId;
            var returnUrl = @"\viewing\search-results";

            // Act
            var result = controller.AddEmployer(employerIdentifier, returnUrl) as LocalRedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Contains(employerIdentifier));
            controller.AssertCookieAdded(CookieNames.LastCompareQuery, expectedObfuscatedOrganisationId);
        }
        #endregion

        #region AddEmployerJS
        [Test]
        public void CompareController_AddEmployerJS_NoEmployerId_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = null;
            string returnUrl = @"\viewing\search-results";

            // Act
            var result = controller.AddEmployerJs(employerIdentifier, returnUrl) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(employerIdentifier)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployerJS_NoReturnUrl_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = null;

            // Act
            var result = controller.AddEmployerJs(employerIdentifier, returnUrl) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(returnUrl)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployerJS_NoLastSearch_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.AddEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployerJS_NoResults_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.AddEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployerJS_WrongEmployer_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.AddEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployerJS_Success_RedirectToReturnUrl()
        {
            // Arrange
            Config.SetAppSetting("SearchService:CacheResults", "true");

            var organisation = new Organisation {OrganisationId = 123, OrganisationName = "Org123", Status = OrganisationStatuses.Active, SectorType = SectorTypes.Private};
            var controller = UiTestHelper.GetController<CompareController>(dbObjects: new object[] {organisation});
            string returnUrl = @"\viewing\search-results";
            var organisationId = 123;
            var expectedObfuscatedOrganisationId = ViewingControllerTests.ConfigureObfuscator(organisationId);

            var employer = new Core.Models.EmployerSearchModel() {
                OrganisationIdEncrypted = expectedObfuscatedOrganisationId,
                OrganisationId = "123",
                Name = "Org123"
            };

            var model = new AddRemoveButtonViewModel() {
                OrganisationIdEncrypted = employer.OrganisationIdEncrypted,
                OrganisationName = employer.Name
            };

            // Act
            var result = controller.AddEmployerJs(employer.OrganisationIdEncrypted, returnUrl) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, controller.ViewBag.ReturnUrl);
            Assert.AreEqual("Basket_Button", result.ViewName);
            CompareHelpers.Compare(model, result.Model);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Contains(model.OrganisationIdEncrypted));
            controller.AssertCookieAdded(CookieNames.LastCompareQuery, expectedObfuscatedOrganisationId);
        }
        #endregion

        #region RemoveEmployer
        [Test]
        public void CompareController_RemoveEmployer_NoEmployerId_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "FAWGFE";
            string employerIdentifier = null;
            var returnUrl = @"\viewing\search-results";
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var result = controller.RemoveEmployer(employerIdentifier, returnUrl) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(employerIdentifier)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Contains(organisationId));
        }

        [Test]
        public void CompareController_RemoveEmployer_NoReturnUrl_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "FGWWR3";
            var employerIdentifier = "abc123";
            string returnUrl = null;
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var result = controller.RemoveEmployer(employerIdentifier, returnUrl) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(returnUrl)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Contains(organisationId));
        }

        [Test]
        public void CompareController_RemoveEmployer_NoLastSearch_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string organisationId = "AFA123";
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.RemoveEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
        }

        [Test]
        public void CompareController_RemoveEmployer_NoEmployers_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "12ASF3";
            var employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.RemoveEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
        }

        [Test]
        public void CompareController_RemoveEmployer_NoResults_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "12AS3";
            var employerIdentifier = "abc123";
            var returnUrl = @"\viewing\search-results";
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.RemoveEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
        }

        [Test]
        public void CompareController_RemoveEmployer_WrongEmployer_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "12sd3";
            var employerIdentifier = "abc123";
            var returnUrl = @"\viewing\search-results";
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.RemoveEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
        }

        [Test]
        public void CompareController_RemoveEmployer_Success_RedirectToReturnUrl()
        {
            // Arrange
            Config.SetAppSetting("SearchService:CacheResults", "true");
            var organisationId = 123;
            var employerIdentifier = Obfuscator.Obfuscate(organisationId);
            var returnUrl = @"\viewing\search-results";

            var organisation = new Organisation {OrganisationId = 123, Status = OrganisationStatuses.Active, SectorType = SectorTypes.Private};
            var controller = UiTestHelper.GetController<CompareController>(dbObjects: new object[] {organisation});
            controller.CompareViewService.AddToBasket(employerIdentifier);

            // Act
            var result = controller.RemoveEmployer(employerIdentifier, returnUrl) as LocalRedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
            Assert.IsFalse(controller.CompareViewService.ComparedEmployers.Contains(employerIdentifier));
            controller.AssertCookieDeleted(CookieNames.LastCompareQuery);
        }
        #endregion

        #region RemoveEmployerJS
        [Test]
        public void CompareController_RemoveEmployerJS_NoEmployerId_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "12fa3";
            string employerIdentifier = null;
            var returnUrl = @"\viewing\search-results";
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var result = controller.RemoveEmployerJs(employerIdentifier, returnUrl) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(employerIdentifier)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Contains(organisationId));
        }

        [Test]
        public void CompareController_RemoveEmployerJS_NoReturnUrl_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "123aa";
            var employerIdentifier = "abc123";
            string returnUrl = null;
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var result = controller.RemoveEmployerJs(employerIdentifier, returnUrl) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(returnUrl)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Contains(organisationId));
        }

        [Test]
        public void CompareController_RemoveEmployerJS_NoLastSearch_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "ga123";
            var employerIdentifier = "abc123";
            var returnUrl = @"\viewing\search-results";
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.RemoveEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
        }

        [Test]
        public void CompareController_RemoveEmployerJS_NoEmployers_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "123fa";
            var employerIdentifier = "abc123";
            var returnUrl = @"\viewing\search-results";
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.RemoveEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
        }

        [Test]
        public void CompareController_RemoveEmployerJS_NoResults_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "1b2c3";
            var employerIdentifier = "abc123";
            var returnUrl = @"\viewing\search-results";
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.RemoveEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
        }

        [Test]
        public void CompareController_RemoveEmployerJS_WrongEmployer_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "12fa3";
            var employerIdentifier = "abc123";
            var returnUrl = @"\viewing\search-results";
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<PageNotFoundException>(() => controller.RemoveEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.NotFound, exception.StatusCode);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
        }

        [Test]
        public void CompareController_RemoveEmployerJS_Success_ReturnAddButtons()
        {
            // Arrange
            var organisation = new Organisation {OrganisationId = 123, Status = OrganisationStatuses.Active, SectorType = SectorTypes.Private};
            var controller = UiTestHelper.GetController<CompareController>(dbObjects: new object[] {organisation});
            long organisationId = 123;
            string employerIdentifier = Obfuscator.Obfuscate(organisationId);
            string returnUrl = @"\viewing\search-results";
            var employer = new Core.Models.EmployerSearchModel() {
                OrganisationIdEncrypted = employerIdentifier,
                OrganisationId = organisationId.ToString()
            };

            controller.CompareViewService.AddToBasket(employer.OrganisationIdEncrypted);
            var model = new AddRemoveButtonViewModel() { OrganisationIdEncrypted = employer.OrganisationIdEncrypted, OrganisationName = employer.Name };

            // Act
            var result = controller.RemoveEmployerJs(employerIdentifier, returnUrl) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, controller.ViewBag.ReturnUrl);
            Assert.AreEqual("Basket_Button", result.ViewName);
            CompareHelpers.Compare(model, result.Model);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
            Assert.IsFalse(controller.CompareViewService.ComparedEmployers.Contains(employer.OrganisationIdEncrypted));
            controller.AssertCookieDeleted(CookieNames.LastCompareQuery);
        }
        #endregion

        #region ClearEmployers

        [Test]
        public void CompareController_ClearEmployers_NoReturnUrl_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "1bb23";
            string returnUrl = null;
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var result = controller.ClearEmployers(returnUrl) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(returnUrl)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Contains(organisationId));
        }

        [Test]
        public void CompareController_ClearEmployers_Success_RedirectToReturnUrl()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "123ga";
            var returnUrl = @"\viewing\search-results";

            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var result = controller.ClearEmployers(returnUrl) as LocalRedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
            Assert.IsFalse(controller.CompareViewService.ComparedEmployers.Contains(organisationId));
        }
        #endregion
        
        #region CompareEmployers
        [Test]
        public void CompareController_CompareEmployers_NoYear_DefaultSortTheMostRecentCompletedReportingYearAsync()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(CompareController.CompareEmployers));
            routeData.Values.Add("Controller", "Viewing");

            var controller = UiTestHelper.GetController<CompareController>(0,routeData);

            //Setup the mock url helper
            var testUri = new Uri("https://localhost/Viewing/compare-employers");
            controller.AddMockUriHelper(testUri.ToString(), "CompareEmployers");

            var reportingYear = ReportingYearsHelper.GetTheMostRecentCompletedReportingYear();
            var mockOrg = OrganisationHelper.GetOrganisationInScope(reportingYear);
            DateTime accountingDateTime = mockOrg.SectorType.GetAccountingStartDate(reportingYear);

            //create the comparison data
            var expectedModel = ViewingServiceHelper.GetCompareTestData(5).ToList();

            //Setup the mocked business logic
            var mockOrgBL = new Mock<IOrganisationBusinessLogic>();
            mockOrgBL
                .Setup(x => x.GetCompareData(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(expectedModel);
            controller.OrganisationBusinessLogic = mockOrgBL.Object;

            // Act
            ViewResult result = controller.CompareEmployers(0) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.ViewName, "CompareEmployers");
            Assert.AreEqual(controller.ViewBag.ReturnUrl, testUri.PathAndQuery);

            var actualModel = result.Model as CompareViewModel;
            Assert.NotNull(actualModel);
            Assert.NotNull(actualModel.CompareReports);
            Assert.IsTrue(actualModel.CompareReports.All(obj => actualModel.Year == reportingYear));
            actualModel.CompareReports.Compare(expectedModel);
        }

        [Test]
        public void CompareController_CompareEmployers_WithYear_SameSortAsync()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(CompareController.CompareEmployers));
            routeData.Values.Add("Controller", "Viewing");

            var controller = UiTestHelper.GetController<CompareController>(0, routeData);

            //Setup the mock url helper
            var testUri = new Uri("https://localhost/Viewing/compare-employers");
            controller.AddMockUriHelper(testUri.ToString(), "CompareEmployers");

            var firstReportingYear = Global.FirstReportingYear;

            var mockOrg = OrganisationHelper.GetOrganisationInScope(firstReportingYear);
            DateTime accountingDateTime = mockOrg.SectorType.GetAccountingStartDate(firstReportingYear);

            //create the comparison data
            var expectedModel = ViewingServiceHelper.GetCompareTestData(5).ToList();

            //Setup the mocked business logic
            var mockOrgBL = new Mock<IOrganisationBusinessLogic>();
            mockOrgBL
                .Setup(x => x.GetCompareData(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(expectedModel);
            controller.OrganisationBusinessLogic = mockOrgBL.Object;

            // Act
            var result = controller.CompareEmployers(firstReportingYear) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.ViewName, "CompareEmployers");
            Assert.AreEqual(controller.ViewBag.ReturnUrl, testUri.PathAndQuery);


            var actualModel = result.Model as CompareViewModel;
            Assert.NotNull(actualModel);
            Assert.NotNull(actualModel.CompareReports);
            Assert.IsTrue(actualModel.CompareReports.All(obj => actualModel.Year == firstReportingYear));
            actualModel.CompareReports.Compare(expectedModel);
        }

        #endregion

    }
}
