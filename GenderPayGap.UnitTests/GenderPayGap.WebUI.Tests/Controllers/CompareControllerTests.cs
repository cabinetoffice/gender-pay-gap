using System.Net;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Models;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
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
        
    }
}
