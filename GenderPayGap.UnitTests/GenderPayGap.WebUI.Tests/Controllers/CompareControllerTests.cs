using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.BusinessLogic.Models.Compare;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Controllers;
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
            controller.SearchViewService.LastSearchResults = null;

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.AddEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployer_NoEmployers_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = null
            };

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.AddEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployer_NoResults_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = null
                }
            };

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.AddEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployer_WrongEmployer_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = new List<Core.Models.EmployerSearchModel>()
                }
            };

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.AddEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);

        }

        [Test]
        public void CompareController_AddEmployer_Success_RedirectToReturnUrl()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            long organisationId = 123;
            var expectedObfuscatedOrganisationId = ViewingControllerTests.ConfigureObfuscator(organisationId);
            var employerIdentifier = expectedObfuscatedOrganisationId;
            var returnUrl = @"\viewing\search-results";
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = new List<Core.Models.EmployerSearchModel>() {
                        new Core.Models.EmployerSearchModel() {
                             OrganisationIdEncrypted=employerIdentifier,
                             OrganisationId=organisationId.ToString()
                        }
                    }
                }
            };

            // Act
            var result = controller.AddEmployer(employerIdentifier, returnUrl) as RedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Value.Contains(employerIdentifier));
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
            controller.SearchViewService.LastSearchResults = null;

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.AddEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployerJS_NoEmployers_ReturnsNotFound()
        {
            // Arrange
            var org = OrganisationHelper.GetMockedOrganisation("abc123");
            var controller = UiTestHelper.GetController<CompareController>();
            string returnUrl = @"\viewing\search-results";
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = null
            };

            var mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(org.EmployerReference))
                .Returns((int)org.OrganisationId);

            // Act
            Assert.Throws<HttpException>(() => controller.AddEmployerJs(org.EmployerReference, returnUrl), "Expected IdentityNotMappedException");
        }

        [Test]
        public void CompareController_AddEmployerJS_NoResults_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = null
                }
            };

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.AddEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployerJS_WrongEmployer_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = new List<Core.Models.EmployerSearchModel>()
                }
            };

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.AddEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
        }

        [Test]
        public void CompareController_AddEmployerJS_Success_RedirectToReturnUrl()
        {
            // Arrange
            Config.SetAppSetting("SearchService:CacheResults", "true");

            var controller = UiTestHelper.GetController<CompareController>();
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

            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = new List<Core.Models.EmployerSearchModel>() {
                        employer
                    }
                }
            };

            // Act
            var result = controller.AddEmployerJs(employer.OrganisationIdEncrypted, returnUrl) as PartialViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, controller.ViewBag.ReturnUrl);
            Assert.AreEqual("Basket_Button", result.ViewName);
            CompareHelpers.Compare(model, result.Model);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Value.Contains(model.OrganisationIdEncrypted));
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
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Value.Contains(organisationId));
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
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Value.Contains(organisationId));
        }

        [Test]
        public void CompareController_RemoveEmployer_NoLastSearch_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string organisationId = "AFA123";
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";
            controller.SearchViewService.LastSearchResults = null;
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.RemoveEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
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
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = null
            };
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.RemoveEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
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
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = null
                }
            };
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.RemoveEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
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
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = new List<Core.Models.EmployerSearchModel>()
                }
            };
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.RemoveEmployer(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
        }

        [Test]
        public void CompareController_RemoveEmployer_Success_RedirectToReturnUrl()
        {
            // Arrange
            Config.SetAppSetting("SearchService:CacheResults", "true");
            var organisationId = 123;
            var employerIdentifier = "abc123";
            var returnUrl = @"\viewing\search-results";

            var controller = UiTestHelper.GetController<CompareController>();
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = new List<Core.Models.EmployerSearchModel>() {
                        new Core.Models.EmployerSearchModel() {
                             OrganisationIdEncrypted=employerIdentifier,
                             OrganisationId=organisationId.ToString()
                        }
                    }
                }
            };
            controller.CompareViewService.AddToBasket(employerIdentifier);

            // Act
            var result = controller.RemoveEmployer(employerIdentifier, returnUrl) as RedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
            Assert.IsFalse(controller.CompareViewService.ComparedEmployers.Value.Contains(employerIdentifier));
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
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Value.Contains(organisationId));
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
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Value.Contains(organisationId));
        }

        [Test]
        public void CompareController_RemoveEmployerJS_NoLastSearch_ReturnsNotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            var organisationId = "ga123";
            var employerIdentifier = "abc123";
            var returnUrl = @"\viewing\search-results";
            controller.SearchViewService.LastSearchResults = null;
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.RemoveEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
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
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = null
            };
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.RemoveEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
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
            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = null
                }
            };
            controller.CompareViewService.AddToBasket(organisationId);

            // Act
            var exception = Assert.Throws<HttpException>(() => controller.RemoveEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
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
            var exception = Assert.Throws<HttpException>(() => controller.RemoveEmployerJs(employerIdentifier, returnUrl));

            Assert.AreEqual((int)HttpStatusCode.BadRequest, exception.StatusCode);
            Assert.AreEqual($"Bad employer identifier {employerIdentifier}", exception.Message);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 1);
        }

        [Test]
        public void CompareController_RemoveEmployerJS_Success_ReturnAddButtons()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            long organisationId = 123;
            string employerIdentifier = "abc123";
            string returnUrl = @"\viewing\search-results";
            var employer = new Core.Models.EmployerSearchModel() {
                OrganisationIdEncrypted = employerIdentifier,
                OrganisationId = organisationId.ToString()
            };

            controller.SearchViewService.LastSearchResults = new SearchViewModel() {
                Employers = new PagedResult<Core.Models.EmployerSearchModel>() {
                    Results = new List<Core.Models.EmployerSearchModel>() {
                        employer
                    }
                }
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
            Assert.IsFalse(controller.CompareViewService.ComparedEmployers.Value.Contains(employer.OrganisationIdEncrypted));
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
            Assert.IsTrue(controller.CompareViewService.ComparedEmployers.Value.Contains(organisationId));
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
            var result = controller.ClearEmployers(returnUrl) as RedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
            Assert.AreEqual(controller.CompareViewService.BasketItemCount, 0);
            Assert.IsFalse(controller.CompareViewService.ComparedEmployers.Value.Contains(organisationId));
        }
        #endregion

        #region SortEmployers

        [Test]
        public async Task CompareController_SortEmployers_NoColumn_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string column = null;
            string returnUrl = @"\viewing\search-results";

            // Act
            var result = await controller.SortEmployers(column, returnUrl) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(column)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.SortColumn, null);
            Assert.AreEqual(controller.CompareViewService.SortAscending, true);
        }

        [Test]
        public async Task CompareController_SortEmployers_NoReturnUrl_ReturnsBadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string column = "OrganisationName";
            string returnUrl = null;

            // Act
            var result = await controller.SortEmployers(column, returnUrl) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int)HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Missing {nameof(returnUrl)}", result.StatusDescription);
            Assert.AreEqual(controller.CompareViewService.SortColumn, null);
            Assert.AreEqual(controller.CompareViewService.SortAscending, true);
        }

        [Test]
        public async Task CompareController_SortEmployers_SuccessAscending_RedirectToReturnUrl()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string column = "OrganisationName";
            string returnUrl = @"\viewing\search-results";

            // Act
            var result = await controller.SortEmployers(column, returnUrl) as RedirectResult;

            // Assert
            //Test the google analytics tracker was executed once on the controller
            controller.WebTracker.GetMockFromObject().Verify(mock => mock.TrackPageViewAsync(It.IsAny<Controller>(),"sort-employers: OrganisationName Ascending", "/compare-employers/sort-employers?OrganisationName=Ascending"), Times.Once());
            
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
            Assert.AreEqual(controller.CompareViewService.SortColumn, column);
            Assert.AreEqual(controller.CompareViewService.SortAscending, true);

        }

        [Test]
        public async Task CompareController_SortEmployers_SuccessDescending_RedirectToReturnUrl()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string column = "OrganisationName";
            string returnUrl = @"\viewing\search-results";
            controller.CompareViewService.SortColumn = column;
            controller.CompareViewService.SortAscending = true;

            // Act
            var result = await controller.SortEmployers(column, returnUrl) as RedirectResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
            Assert.AreEqual(controller.CompareViewService.SortColumn, column);
            Assert.AreEqual(controller.CompareViewService.SortAscending, false);

        }

        [Test]
        public async Task CompareController_SortEmployers_SuccessChange_RedirectToReturnUrl()
        {
            // Arrange
            var controller = UiTestHelper.GetController<CompareController>();
            string column = "OrganisationName";
            string returnUrl = @"\viewing\search-results";
            controller.CompareViewService.SortColumn = "MaleUpperQuartilePayBand";
            controller.CompareViewService.SortAscending = false;

            // Act
            var result = await controller.SortEmployers(column, returnUrl) as RedirectResult;

            // Assert
            //Test the google analytics tracker was executed once on the controller
            controller.WebTracker.GetMockFromObject().Verify(mock => mock.TrackPageViewAsync(It.IsAny<Controller>(), "sort-employers: OrganisationName Descending", "/compare-employers/sort-employers?OrganisationName=Descending"), Times.Once());

            Assert.NotNull(result);
            Assert.AreEqual(returnUrl, result.Url);
            Assert.AreEqual(controller.CompareViewService.SortColumn, column);
            Assert.AreEqual(controller.CompareViewService.SortAscending, true);
        }

        #endregion

        #region CompareEmployers
        [Test]
        public async Task CompareController_CompareEmployers_NoYear_DefaultSortFirstYearAsync()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(CompareController.CompareEmployers));
            routeData.Values.Add("Controller", "Viewing");

            var controller = UiTestHelper.GetController<CompareController>(0,routeData);

            //Setup the mock url helper
            var testUri = new Uri("https://localhost/Viewing/compare-employers");
            controller.AddMockUriHelper(testUri.ToString(), "CompareEmployers");

            var firstReportingYear = Global.FirstReportingYear;
            var mockOrg = OrganisationHelper.GetOrganisationInScope("MockedOrg", firstReportingYear);
            DateTime accountingDateTime = mockOrg.Sector.GetAccountingStartDate(firstReportingYear);

            //create the comparison data
            var expectedModel = ViewingServiceHelper.GetCompareTestData(5).ToList();

            //Setup the mocked business logic
            var mockOrgBL = new Mock<IOrganisationBusinessLogic>();
            mockOrgBL
                .Setup(x => x.GetCompareDataAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedModel);
            controller.OrganisationBusinessLogic = mockOrgBL.Object;

            // Act
            ViewResult result = await controller.CompareEmployers(0) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.ViewName, "CompareEmployers");
            Assert.AreEqual(controller.ViewBag.ReturnUrl, testUri.PathAndQuery);
            Assert.AreEqual(controller.CompareViewService.SortColumn, null);
            Assert.AreEqual(controller.CompareViewService.SortAscending, true);

            var lastComparedEmployerList = controller.CompareViewService.ComparedEmployers.Value.ToList().ToSortedSet().ToDelimitedString();
            controller.CompareViewService.LastComparedEmployerList.Compare(lastComparedEmployerList);

            var actualModel = result.Model as CompareViewModel;
            Assert.NotNull(actualModel);
            Assert.NotNull(actualModel.CompareReports);
            Assert.IsTrue(actualModel.CompareReports.All(obj => actualModel.Year == firstReportingYear));
            actualModel.CompareReports.Compare(expectedModel);
        }

        [Test]
        public async Task CompareController_CompareEmployers_WithYear_SameSortAsync()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(CompareController.CompareEmployers));
            routeData.Values.Add("Controller", "Viewing");

            var controller = UiTestHelper.GetController<CompareController>(0, routeData);

            //Setup the mock url helper
            var testUri = new Uri("https://localhost/Viewing/compare-employers");
            controller.AddMockUriHelper(testUri.ToString(), "CompareEmployers");

            controller.CompareViewService.SortColumn = "OrganisationSize";
            controller.CompareViewService.SortAscending = false;

            var firstReportingYear = Global.FirstReportingYear;

            var mockOrg = OrganisationHelper.GetOrganisationInScope("MockedOrg", firstReportingYear);
            DateTime accountingDateTime = mockOrg.Sector.GetAccountingStartDate(firstReportingYear);

            //create the comparison data
            var expectedModel = ViewingServiceHelper.GetCompareTestData(5).ToList();

            //Setup the mocked business logic
            var mockOrgBL = new Mock<IOrganisationBusinessLogic>();
            mockOrgBL
                .Setup(x => x.GetCompareDataAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedModel);
            controller.OrganisationBusinessLogic = mockOrgBL.Object;

            // Act
            var result = await controller.CompareEmployers(firstReportingYear) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual(result.ViewName, "CompareEmployers");
            Assert.AreEqual(controller.ViewBag.ReturnUrl, testUri.PathAndQuery);
            Assert.AreEqual(controller.CompareViewService.SortColumn, "OrganisationSize");
            Assert.AreEqual(controller.CompareViewService.SortAscending, false);


            var actualModel = result.Model as CompareViewModel;
            Assert.NotNull(actualModel);
            Assert.NotNull(actualModel.CompareReports);
            Assert.IsTrue(actualModel.CompareReports.All(obj => actualModel.Year == firstReportingYear));
            actualModel.CompareReports.Compare(expectedModel);
        }

        #endregion

        #region DownloadCompareData
        [Test]
        public async Task CompareController_DownloadCompareData_NoYear_DefaultSortFirstYearAsync()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(CompareController.DownloadCompareData));
            routeData.Values.Add("Controller", "Viewing");

            var controller = UiTestHelper.GetController<CompareController>(0, routeData);
            var firstReportingYear = Global.FirstReportingYear;
            var mockOrg = OrganisationHelper.GetOrganisationInScope("MockedOrg", firstReportingYear);
            DateTime accountingDateTime = mockOrg.Sector.GetAccountingStartDate(firstReportingYear);

            //create the comparison data
            var expectedModel = ViewingServiceHelper.GetCompareTestData(5).ToList();

            //Setup the mocked business logic
            var mockOrgBL = new Mock<IOrganisationBusinessLogic>();
            mockOrgBL
                .Setup(x => x.GetCompareDataAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedModel);

            var expectedData = expectedModel.ToDataTable();
            mockOrgBL
                .Setup(x => x.GetCompareDatatable(It.IsAny<IEnumerable<CompareReportModel>>()))
                .Returns(expectedData);
            controller.OrganisationBusinessLogic = mockOrgBL.Object;

            // Act
            var result = await controller.DownloadCompareData() as ContentResult;

            // Assert
            //Test the google analytics tracker was executed once on the controller
            var filename = $"Compared GPG Data {firstReportingYear}-{(firstReportingYear+1).ToTwoDigitYear()}.csv";
            controller.WebTracker.GetMockFromObject().Verify(mock => mock.TrackPageViewAsync(It.IsAny<Controller>(), filename, null), Times.Once());

            Assert.NotNull(result);
            Assert.AreEqual(result.ContentType, "text/csv");
            Assert.AreEqual(controller.Response.Headers["Content-Disposition"], $"attachment; filename=\"{filename}\"");

            Assert.AreEqual(controller.CompareViewService.SortColumn, null);
            Assert.AreEqual(controller.CompareViewService.SortAscending, true);

            Assert.NotNull(result.Content);
            Assert.AreEqual(result.Content, expectedData.ToCSV());
        }

        [Test]
        public async Task CompareController_DownloadCompareData_WithYear_SameSortAsync()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", nameof(CompareController.DownloadCompareData));
            routeData.Values.Add("Controller", "Viewing");

            var controller = UiTestHelper.GetController<CompareController>(0, routeData);


            controller.CompareViewService.SortColumn = "OrganisationSize";
            controller.CompareViewService.SortAscending = false;

            var firstReportingYear = Global.FirstReportingYear;

            var mockOrg = OrganisationHelper.GetOrganisationInScope("MockedOrg", firstReportingYear);
            DateTime accountingDateTime = mockOrg.Sector.GetAccountingStartDate(firstReportingYear);

            //create the comparison data
            var expectedModel = ViewingServiceHelper.GetCompareTestData(5).ToList();

            //Setup the mocked business logic
            var mockOrgBL = new Mock<IOrganisationBusinessLogic>();
            mockOrgBL
                .Setup(x => x.GetCompareDataAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedModel);
            var expectedData = expectedModel.ToDataTable();
            mockOrgBL
                .Setup(x => x.GetCompareDatatable(It.IsAny<IEnumerable<CompareReportModel>>()))
                .Returns(expectedData);
            controller.OrganisationBusinessLogic = mockOrgBL.Object;

            // Act
            var result = await controller.DownloadCompareData(firstReportingYear) as ContentResult;

            // Assert

            //Test the google analytics tracker was executed once on the controller
            var filename = $"Compared GPG Data {firstReportingYear}-{(firstReportingYear + 1).ToTwoDigitYear()}.csv";
            controller.WebTracker.GetMockFromObject().Verify(mock => mock.TrackPageViewAsync(It.IsAny<Controller>(), filename, null), Times.Once());

            Assert.NotNull(result);
            Assert.AreEqual(result.ContentType, "text/csv");
            Assert.AreEqual(controller.Response.Headers["Content-Disposition"], $"attachment; filename=\"{filename}\"");

            Assert.AreEqual(controller.CompareViewService.SortColumn, "OrganisationSize");
            Assert.AreEqual(controller.CompareViewService.SortAscending, false);

            Assert.NotNull(result.Content);
            Assert.AreEqual(result.Content, expectedData.ToCSV());
        }

        #endregion


    }
}
