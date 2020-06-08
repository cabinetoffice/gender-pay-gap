using System;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using GenderPayGap.BusinessLogic;
using GenderPayGap.BusinessLogic.Models.Submit;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Core.Models;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
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
    [TestFixture]
    [SetCulture("en-GB")]
    public class ViewingControllerTests
    {

        #region Routing

        [TestCase("~/", "Index")]
        [TestCase("/", "Index")]

        // legacy links used to read employer-details?id:blah
        [TestCase("/viewing/employer-details", "EmployerDetails")]
        [TestCase("/viewing/employer-details?e=16789", "EmployerDetails")]
        [TestCase("/viewing/employer-details?id=,C4ccuVKhO6F0MlQxXdf7Rw!!", "EmployerDetails")]

        // previous employer-dash links (longer)
        [TestCase("/viewing/employer-{employerIdentifier}", "EmployerDeprecated")]
        [TestCase("/viewing/employer-,PLQZ1tnP3Kszj-mD71Rl2Q!!", "EmployerDeprecated")]

        //// new shorter links:
        [TestCase("/Employer/OkeAd4r9", "Employer")]
        [TestCase("/Employer/7M7MVukE/report-2018", "Report")]
        [TestCase("/viewing/employer-,PLQZ1tnP3Kszj-mD71Rl2Q!!/report-2017", "ReportDeprecated")]
        [TestCase("/Employer/5gY48Hyt/2017", "Report")]
        public void ViewingController_EmployerDetails_Testing_Routes(string url, string expectedAction)
        {
            /*
             * This test's code has been copied from:
             * https://salvadorgascon.wordpress.com/2014/10/13/test-your-server-side-routes/
             */

            /* This no longer works with AspNetCore
             * 
            var routes = new RouteCollection();

            // Add routes using attributes
            routes.MapAttributeRoutesInAssembly(typeof(ViewingController));

            // Avoid "MapMvcAttributeRoutes" invocation method
            RouteConfig.RegisterRoutes(routes);

            // And finally, test the route
            RouteAssert.HasRoute(
                routes,
                url,
                new { controller = "Viewing", action = expectedAction}
            );*/
        }

        #endregion

        #region Search

        [Test]
        [Description("ViewingController.SearchResults GET: Returns 400 for invalid search parameters")]
        public async Task ViewingController_SearchResults_GET_Returns_400_for_invalid_search_parameters()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "SearchResults");
            mockRouteData.Values.Add("Controller", "Viewing");

            var controller = UiTestHelper.GetController<ViewingController>(0, mockRouteData);

            // Test p Parameter
            var result = await controller.SearchResults(new SearchResultsQuery {search = "search text", p = 0}) as HttpStatusViewResult;
            Assert.NotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("EmployerSearch: Invalid page 0", result.StatusDescription);

            // Test es Parameter
            result =
                await controller.SearchResults(new SearchResultsQuery {search = "search text", es = new[] {4, 10}, p = 1}) as
                    HttpStatusViewResult;
            Assert.NotNull(result);
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual("EmployerSearch: Invalid EmployerSize 4,10", result.StatusDescription);
        }

        [Ignore("Relies on Azure search, which will be replaced soon")]
        [Test]
        [Description("ViewingController.SearchResults GET: Returns Finder/SearchResults view")]
        public async Task ViewingController_SearchResults_GET_Returns_Finder_SearchResults_view()
        {
            // Arrange
            var mockRouteData = new RouteData();
            mockRouteData.Values.Add("Action", "SearchResults");
            mockRouteData.Values.Add("Controller", "Viewing");

            var controller = UiTestHelper.GetController<ViewingController>(0, mockRouteData);

            var searchText = "search text";
            var sectorType = SearchType.BySectorType;
            // Test
            var query = new SearchResultsQuery {search = searchText, t = sectorType, p = 1};
            var result = await controller.SearchResults(query) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual("Finder/SearchResults", result.ViewName);
            var model = result.Model as SearchViewModel;
            Assert.NotNull(model);
            Assert.AreEqual(query.search, model.search);
            Assert.AreEqual(query.p, model.p);
        }

        #endregion

        #region Downloads

        #endregion

        #region EmployerDeprecated

        [Test]
        public void ViewingController_EmployerDeprecated_When_ReceivedId_Is_Not_Valid_Base64_String_Then_Returns_Custom_Error()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();

            // Act
            var result = controller.EmployerDeprecated("?&%") as ViewResult;
            Assert.NotNull(result);
            var model = result.Model as ErrorViewModel;
            Assert.NotNull(model);

            // Assert
            Assert.AreEqual("CustomError", result.ViewName);
            Assert.AreEqual(400, model.ErrorCode);
        }

        [Test]
        public void ViewingController_EmployerDeprecated_NoEmployerIdentity_returns_BadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();

            // Act
            var result = controller.EmployerDeprecated(string.Empty) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual("Missing employer identifier", result.StatusDescription);
        }

        [Test]
        public void ViewingController_EmployerDeprecated_When_An_Encrypted_Long_Id_Is_Received_Then_()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerDetails");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {OrganisationId = 202, Status = OrganisationStatuses.Active};

            var report = new Return {ReturnId = 101, OrganisationId = org.OrganisationId, Status = ReturnStatuses.Submitted};
            var controller = UiTestHelper.GetController<ViewingController>(default, routeData, report);

            string mockedObfuscatorToSetup = ConfigureObfuscator(org.OrganisationId);

            string longEncryptedEmployerId = Encryption.EncryptQuerystring(org.OrganisationId.ToString());

            // Act
            var result = controller.EmployerDeprecated(longEncryptedEmployerId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Permanent);
            Assert.AreEqual("Employer", result.ActionName);
            Assert.That(result.ControllerName.EqualsI("Viewing", null));
            Assert.AreNotEqual("dVgHdQEQ", result.RouteValues["employerIdentifier"], "InternalObfuscator must NOT use seed 113");
            Assert.AreNotEqual("NAnA4HgE", result.RouteValues["employerIdentifier"], "InternalObfuscator must NOT use seed 127");
        }

        #endregion

        #region Employer details

        [Test]
        public async Task ViewingController_EmployerDetails_NoEmployerIdentity_returns_BadRequestAsync()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();

            // Act
            var result = await controller.EmployerDetails(string.Empty) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual("EmployerDetails: 'e' query parameter was null or white space", result.StatusDescription);
        }

        [Test]
        [Description("EmployerDetails: Permanent Redirect using return id")]
        public async Task ViewingController_EmployerDetails_RedirectUsingReturnIdAsync()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "EmployerDetails");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {OrganisationId = 98745, Status = OrganisationStatuses.Active, OrganisationName = "patata"};

            var report = new Return {ReturnId = 65412, OrganisationId = org.OrganisationId, Status = ReturnStatuses.Submitted};
            var controller = UiTestHelper.GetController<ViewingController>(0, routeData, org, report);

            var submissionBusinessLogic = new SubmissionBusinessLogic(UiTestHelper.DIContainer.Resolve<IDataRepository>());

            var organisationBusinessLogic = new OrganisationBusinessLogic(
                UiTestHelper.DIContainer.Resolve<IDataRepository>(),
                submissionBusinessLogic,
                UiTestHelper.DIContainer.Resolve<IEncryptionHandler>(),
                UiTestHelper.DIContainer.Resolve<IObfuscator>());

            controller.OrganisationBusinessLogic = organisationBusinessLogic;

            string encryptedReportId = ConfigureEncryptionHandler(report.ReturnId.ToString());

            string expectedObfuscatedOrganisationId = ConfigureObfuscator(report.OrganisationId);

            // Act
            var result = await controller.EmployerDetails(id: encryptedReportId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.Permanent);
            Assert.AreEqual("Employer", result.ActionName);
            Assert.That(result.ControllerName.EqualsI("Viewing", null));

            string routevalue = result.RouteValues["employerIdentifier"].ToString();

            Assert.AreEqual(expectedObfuscatedOrganisationId, result.RouteValues["employerIdentifier"].ToString());
        }

        private static string ConfigureEncryptionHandler(string valueToEncrypt)
        {
            string expectedResult = Encryption.EncryptQuerystring(valueToEncrypt);

            Mock<IEncryptionHandler> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IEncryptionHandler>();
            mockedObfuscatorToSetup
                .Setup(x => x.DecryptAndDecode(It.IsAny<string>()))
                .Returns(valueToEncrypt);

            return expectedResult;
        }

        [Test]
        public async Task ViewingController_EmployerDetails_When_Y_Is_Received_Then_An_Error_Is_ReturnedAsync()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();

            // Act
            var result = await controller.EmployerDetails(y: 34) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual("EmployerDetails: 'e' query parameter was null or white space", result.StatusDescription);
        }

        [Test]
        [Description("EmployerDetails: Permanent Redirect using organisation id")]
        public async Task ViewingController_EmployerDetails_RedirectUsingOrganisationIdAsync()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();
            string obfuscatedReportOrganisationId = ConfigureObfuscator(10158);

            // Act
            var result = await controller.EmployerDetails(obfuscatedReportOrganisationId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.Permanent);
            Assert.AreEqual("Employer", result.ActionName);
            //Assert.AreEqual("Viewing", result.ControllerName);
            Assert.AreEqual(obfuscatedReportOrganisationId, result.RouteValues["employerIdentifier"].ToString());
        }

        [Test]
        public async Task ViewingController_EmployerDetails_When_ReceivedId_Is_Not_Valid_Base64_String_Then_Returns_Custom_ErrorAsync()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();

            var organisationBusinessLogic = new OrganisationBusinessLogic(
                UiTestHelper.DIContainer.Resolve<IDataRepository>(),
                UiTestHelper.DIContainer.Resolve<ISubmissionBusinessLogic>(),
                new EncryptionHandler(),
                UiTestHelper.DIContainer.Resolve<IObfuscator>());

            controller.OrganisationBusinessLogic = organisationBusinessLogic;

            // Act
            var result = await controller.EmployerDetails(id: "?&%") as ViewResult;
            Assert.NotNull(result);
            var model = result.Model as ErrorViewModel;
            Assert.NotNull(model);

            // Assert
            Assert.AreEqual("CustomError", result.ViewName);
            Assert.AreEqual(400, model.ErrorCode);
        }

        [Test]
        public async Task ViewingController_EmployerDetails_When_Report_Is_Not_Found_Then_Returns_Custom_ErrorAsync()
        {
            // Arrange
            var returnIdToUse = "2548";
            var controller = UiTestHelper.GetController<ViewingController>();
            string encryptedReturnId = Encryption.EncryptQuerystring(returnIdToUse);

            // Act
            var result = await controller.EmployerDetails(id: encryptedReturnId) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
            Assert.AreEqual($"Employer: Could not find GPG Data for returnId:'{encryptedReturnId}'", result.StatusDescription);
        }

        #endregion

        #region Employer

        [Test]
        public void ViewingController_Employer_NoEmployerIdentity_returns_BadRequest()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Employer");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {OrganisationId = 202, Status = OrganisationStatuses.Active};

            var report = new Return {ReturnId = 101, OrganisationId = org.OrganisationId, Status = ReturnStatuses.Submitted};

            var controller = UiTestHelper.GetController<ViewingController>(0, routeData, org, report);

            // Act
            var result = controller.Employer("") as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public void ViewingController_Employer_BadEmployerIdentity_returns_400CustomError()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Employer");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {OrganisationId = 202, Status = OrganisationStatuses.Active};

            var report = new Return {ReturnId = 101, OrganisationId = org.OrganisationId, Status = ReturnStatuses.Submitted};

            var controller = UiTestHelper.GetController<ViewingController>(0, routeData, org, report);

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(It.IsAny<string>()))
                .Throws(new Exception("Kaboom"));

            // Act
            var result = controller.Employer("ADGFGHU!$£") as ViewResult;
            Assert.NotNull(result, "Expected ViewResult");
            var model = result.Model as ErrorViewModel;
            Assert.NotNull(model);

            // Assert
            Assert.AreEqual("CustomError", result.ViewName);
            Assert.AreEqual(400, model.ErrorCode);
        }

        [Test]
        public void ViewingController_Employer_ZeroOrgId_returns_BadRequest()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Employer");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {OrganisationId = 202, Status = OrganisationStatuses.Active};

            var report = new Return {ReturnId = 101, OrganisationId = org.OrganisationId, Status = ReturnStatuses.Submitted};

            var controller = UiTestHelper.GetController<ViewingController>(0, routeData, org, report);

            // Act
            var result = controller.Employer(Encryption.EncryptQuerystring("0")) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result, "Expected HttpStatusViewResult");
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public void ViewingController_Employer_NoEmployer_returns_NotFound()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Employer");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {OrganisationId = 202, Status = OrganisationStatuses.Active};

            var report = new Return {ReturnId = 101, OrganisationId = org.OrganisationId, Status = ReturnStatuses.Submitted};

            var controller = UiTestHelper.GetController<ViewingController>(0, routeData, org, report);

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(It.IsAny<string>()))
                .Returns(9876);

            // Act
            var result = controller.Employer(Encryption.EncryptQuerystring("200")) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
            Assert.AreEqual("Employer: Could not find organisation ',di0kAFDy6aEo_ovct9iTpg!!'", result.StatusDescription);
        }

        [TestCase(OrganisationStatuses.Deleted, "Employer: The status of this organisation is 'Deleted'")]
        public void ViewingController_Employer_When_OrganisationStatus_Is_Not_Active_Returns_Gone(OrganisationStatuses orgStatus,
            string expectedDescriptionMessage)
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Employer");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {OrganisationId = 202, Status = orgStatus};

            var controller = UiTestHelper.GetController<ViewingController>(0, routeData, org);

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(It.IsAny<string>()))
                .Returns(org.OrganisationId.ToInt32());

            // Act
            var result = controller.Employer(org.GetEncryptedId()) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.Gone, result.StatusCode);
            Assert.AreEqual(expectedDescriptionMessage, result.StatusDescription);
        }

        [Test]
        public void ViewingController_Employer_Active_Organisation_returns_CorrectViewAndModel()
        {
            // Arrange
            var org = new Organisation {OrganisationId = 6548, Status = OrganisationStatuses.Active};

            var controller = UiTestHelper.GetController<ViewingController>(default, null, org);

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(It.IsAny<string>()))
                .Returns(org.OrganisationId.ToInt32());

            // Act
            var result = controller.Employer("someEncryptedOrgId") as ViewResult;

            // Assert
            // Check for correct view
            Assert.NotNull(result, "Expected ViewResult not to be null");
            Assert.AreEqual("EmployerDetails/Employer", result.ViewName);

            // Check for organisation model
            var model = result.Model as EmployerDetailsViewModel;
            Assert.NotNull(model, "Expected EmployerDetailsViewModel not to be null");
            Assert.NotNull(model.Organisation, "Expected Organisation not to be null");

            // Check model is same as expected
            org.Compare(model.Organisation);
        }

        #endregion

        #region ReportDeprecated

        [Test]
        [Description("Report: When no employer reference returns a HTTPbad request error")]
        public void ViewingController_ReportDeprecated_NoEmployerIdentity_returns_BadRequest()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Report");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {
                OrganisationId = 202,
                Status = OrganisationStatuses.Active,
                SectorType = Numeric.Rand(0, 1) == 0 ? SectorTypes.Private : SectorTypes.Public
            };

            var report = new Return {
                ReturnId = 101,
                OrganisationId = org.OrganisationId,
                Status = ReturnStatuses.Submitted,
                AccountingDate = org.SectorType.GetAccountingStartDate(Numeric.Rand(Global.FirstReportingYear, VirtualDateTime.Now.Year))
            };

            var controller = UiTestHelper.GetController<ViewingController>(0, routeData, org, report);

            // Act
            var result = controller.ReportDeprecated("", report.AccountingDate.Year) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result, "Expected HttpStatusViewResult");
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        [Description("Report: When year before first reporting year returns a HTTPbad request error")]
        public void ViewingController_ReportDeprecated_YearTooEarly_returns_BadRequest()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Report");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {
                OrganisationId = 202,
                Status = OrganisationStatuses.Active,
                SectorType = Numeric.Rand(0, 1) == 0 ? SectorTypes.Private : SectorTypes.Public
            };

            var report = new Return {
                ReturnId = 101,
                OrganisationId = org.OrganisationId,
                Status = ReturnStatuses.Submitted,
                AccountingDate = org.SectorType.GetAccountingStartDate(Numeric.Rand(Global.FirstReportingYear, VirtualDateTime.Now.Year))
            };

            var controller = UiTestHelper.GetController<ViewingController>(0, routeData, org, report);

            // Act
            var result = controller.ReportDeprecated(org.GetEncryptedId(), Global.FirstReportingYear - 1) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result, "Expected HttpStatusViewResult");
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        [Description("Report: When year after this year returns a HTTPbad request error")]
        public void ViewingController_ReportDeprecated_YearTooLate_returns_BadRequest()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Report");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {
                OrganisationId = 202,
                Status = OrganisationStatuses.Active,
                SectorType = Numeric.Rand(0, 1) == 0 ? SectorTypes.Private : SectorTypes.Public
            };

            var report = new Return {
                ReturnId = 101,
                OrganisationId = org.OrganisationId,
                Status = ReturnStatuses.Submitted,
                AccountingDate = org.SectorType.GetAccountingStartDate(Numeric.Rand(Global.FirstReportingYear, VirtualDateTime.Now.Year))
            };

            var controller = UiTestHelper.GetController<ViewingController>(0, routeData, org, report);

            // Act
            var result = controller.ReportDeprecated(org.GetEncryptedId(), VirtualDateTime.Now.AddYears(1).Year) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result, "Expected HttpStatusViewResult");
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public void ViewingController_ReportDeprecated_BadEmployerIdentity_returns_400CustomError()
        {
            // Arrange
            var routeData = new RouteData();
            routeData.Values.Add("Action", "Report");
            routeData.Values.Add("Controller", "Viewing");

            var org = new Organisation {
                OrganisationId = 202,
                Status = OrganisationStatuses.Active,
                SectorType = Numeric.Rand(0, 1) == 0 ? SectorTypes.Private : SectorTypes.Public
            };

            var report = new Return {
                ReturnId = 101,
                OrganisationId = org.OrganisationId,
                Status = ReturnStatuses.Submitted,
                AccountingDate = org.SectorType.GetAccountingStartDate(Numeric.Rand(Global.FirstReportingYear, VirtualDateTime.Now.Year))
            };

            var controller = UiTestHelper.GetController<ViewingController>(0, routeData, org, report);

            // Act
            var result = controller.ReportDeprecated("ADGFGHU!$£", report.AccountingDate.Year) as ViewResult;
            Assert.NotNull(result);
            var model = result.Model as ErrorViewModel;
            Assert.NotNull(model);

            // Assert
            Assert.AreEqual("CustomError", result.ViewName);
            Assert.AreEqual(400, model.ErrorCode);
        }

        [Test]
        public void ViewingController_ReportDeprecated_Zero_Organisation_Id_Returns_Redirection_To_Report_Method()
        {
            // Arrange
            var organisationId = 0;
            var controller = UiTestHelper.GetController<ViewingController>();
            string obfuscatedOrgId = ConfigureObfuscator(organisationId);
            string encryptedOrganisationId = new EncryptionHandler()
                .EncryptAndEncode(organisationId);

            // Act
            var result = controller.ReportDeprecated(encryptedOrganisationId, Global.FirstReportingYear) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsTrue(result.Permanent);
            Assert.AreEqual("Report", result.ActionName);
            Assert.AreEqual(obfuscatedOrgId, result.RouteValues["employerIdentifier"].ToString());
        }

        public static string ConfigureObfuscator(long valueToObfuscate)
        {
            string result = new InternalObfuscator().Obfuscate(valueToObfuscate.ToString());

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();

            mockedObfuscatorToSetup
                .Setup(x => x.Obfuscate(It.IsAny<int>()))
                .Returns(result);

            mockedObfuscatorToSetup
                .Setup(x => x.Obfuscate(It.IsAny<string>()))
                .Returns(result);

            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(result))
                .Returns(valueToObfuscate.ToInt32());

            return result;
        }

        #endregion

        #region Report

        [Test]
        public void ViewingController_Report_NoEmployerIdentity_returns_BadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();

            // Act
            var result = controller.Report(string.Empty, default) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual("Missing employer identifier", result.StatusDescription);
        }

        [Test]
        public void ViewingController_Report_YearTooEarly_returns_BadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();
            int year = Global.FirstReportingYear - 1;

            // Act
            var result = controller.Report("_", year) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Invalid snapshot year {year}", result.StatusDescription);
        }

        [Test]
        public void ViewingController_Report_YearTooLate_returns_BadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();
            int year = VirtualDateTime.Now.AddYears(1).Year;

            // Act
            var result = controller.Report("_", year) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Invalid snapshot year {year}", result.StatusDescription);
        }

        [Test]
        public void ViewingController_Report_BadEmployerIdentity_returns_400_CustomError()
        {
            var controller = UiTestHelper.GetController<ViewingController>();

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(It.IsAny<string>()))
                .Throws(new Exception("Kaboom"));

            // Act
            var result = controller.Report("dxDN£34MdgC", Global.FirstReportingYear) as ViewResult;
            Assert.NotNull(result);
            var model = result.Model as ErrorViewModel;
            Assert.NotNull(model);

            // Assert
            Assert.AreEqual("CustomError", result.ViewName);
            Assert.AreEqual(400, model.ErrorCode);
            Assert.AreEqual("Please try again later", model.Description);
            Assert.AreEqual("Something’s gone wrong", model.Title);
            Assert.AreEqual("Continue", model.ActionText);
        }

        [Test]
        public void ViewingController_Report_ZeroOrgId_returns_BadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();
            string obfuscatedZeroOrganisationId = new InternalObfuscator().Obfuscate(0);

            // Act
            var result = controller.Report(obfuscatedZeroOrganisationId, Global.FirstReportingYear) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.BadRequest, result.StatusCode);
            Assert.AreEqual($"Bad employer identifier {obfuscatedZeroOrganisationId}", result.StatusDescription);
            Assert.AreNotEqual("B7gZMAkD", result.StatusDescription, "InternalObfuscator must NOT use seed 113");
            Assert.AreNotEqual("dZzBr4ns", result.StatusDescription, "InternalObfuscator must NOT use seed 127");
        }

        [Test]
        public void ViewingController_Report_NoEmployer_returns_NotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();
            string obfuscatedEmployerId = ConfigureObfuscator(1548);

            // Act
            var result = controller.Report(obfuscatedEmployerId, Global.FirstReportingYear) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
            Assert.AreEqual("Employer: Could not find organisation 'Mzc8pnQs'", result.StatusDescription);
        }

        [TestCase(20154, OrganisationStatuses.Deleted)]
        public void ViewingController_Report_Organisation_Status_Not_Active_returns_Gone(int organisationId,
            OrganisationStatuses organisationStatus)
        {
            var org = new Organisation {OrganisationId = organisationId, Status = organisationStatus};

            var controller = UiTestHelper.GetController<ViewingController>(org.OrganisationId, null, org);

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(It.IsAny<string>()))
                .Returns(org.OrganisationId.ToInt32());

            // Act
            var result = controller.Report(org.GetEncryptedId(), Global.FirstReportingYear) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.Gone, result.StatusCode);
            Assert.AreEqual($"Employer: The status of this organisation is '{organisationStatus}'", result.StatusDescription);
        }

        [Test]
        public void ViewingController_Report_NoReport_returns_NotFound()
        {
            // Arrange
            var org = new Organisation {OrganisationId = 89754, Status = OrganisationStatuses.Active};

            var controller = UiTestHelper.GetController<ViewingController>(org.OrganisationId, null, org);

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(It.IsAny<string>()))
                .Returns(org.OrganisationId.ToInt32());

            // Act
            var result = controller.Report(org.GetEncryptedId(), Global.FirstReportingYear) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
            Assert.AreEqual("Employer: Could not find GPG Data for organisation:jdCWd4F1 and year:2017", result.StatusDescription);
        }

        [TestCase(ReturnStatuses.Deleted)]
        [TestCase(ReturnStatuses.Retired)]
        public void ViewingController_Report_Status_Not_Submitted_returns_Gone(ReturnStatuses returnStatus)
        {
            // Arrange
            var org = new Organisation {OrganisationId = 98754, Status = OrganisationStatuses.Active, SectorType = SectorTypes.Public};

            var report1 = new Return {
                ReturnId = 101,
                OrganisationId = org.OrganisationId,
                Status = returnStatus == ReturnStatuses.Retired ? ReturnStatuses.Deleted : ReturnStatuses.Retired,
                StatusDate = VirtualDateTime.Now,
                AccountingDate = org.SectorType.GetAccountingStartDate(Global.FirstReportingYear)
            };
            var report2 = new Return {
                ReturnId = 102,
                OrganisationId = org.OrganisationId,
                Status = returnStatus,
                StatusDate = report1.StatusDate.AddSeconds(1),
                AccountingDate = org.SectorType.GetAccountingStartDate(Global.FirstReportingYear)
            };

            var controller = UiTestHelper.GetController<ViewingController>(org.OrganisationId, null, org, report1, report2);

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(It.IsAny<string>()))
                .Returns(org.OrganisationId.ToInt32());

            // Act
            var result = controller.Report(org.GetEncryptedId(), report1.AccountingDate.Year) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result, "Expected HttpStatusViewResult");
            Assert.AreEqual((int) HttpStatusCode.Gone, result.StatusCode);
            Assert.AreEqual($"Employer report '2017' is showing with status '{returnStatus}'", result.StatusDescription);
        }

        [Test]
        public void ViewingController_Report_If_Report_Is_Missing_Then_Returns_Http_Not_Found()
        {
            // Arrange
            var org = new Organisation {
                OrganisationId = 6548,
                Status = OrganisationStatuses.Active,
                SectorType = Numeric.Rand(0, 1) == 0 ? SectorTypes.Private : SectorTypes.Public
            };

            var report = new Return {
                ReturnId = 98754,
                OrganisationId = org.OrganisationId,
                Status = ReturnStatuses.Retired,
                AccountingDate = org.SectorType.GetAccountingStartDate(Global.FirstReportingYear)
            };

            var controller = UiTestHelper.GetController<ViewingController>(default, null, org, report);
            string obfuscatedOrganisationId = org.GetEncryptedId();
            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(obfuscatedOrganisationId))
                .Returns(org.OrganisationId.ToInt32());

            // Act
            var result = controller.Report(obfuscatedOrganisationId, Global.FirstReportingYear + 1) as HttpStatusViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
            Assert.AreEqual(
                $"Employer: Could not find GPG Data for organisation:{obfuscatedOrganisationId} and year:2018",
                result.StatusDescription);
        }

        [Test]
        public void ViewingController_Report_When_GetSubmissionByOrganisationAndYear_Throws_Exception_Then_Returns_CustomError()
        {
            // Arrange
            var org = new Organisation {
                OrganisationId = 6548,
                Status = OrganisationStatuses.Active,
                SectorType = Numeric.Rand(0, 1) == 0 ? SectorTypes.Private : SectorTypes.Public
            };

            var controller = UiTestHelper.GetController<ViewingController>(default, null, org);

            string obfuscatedOrganisationId = org.GetEncryptedId();
            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(obfuscatedOrganisationId))
                .Returns(org.OrganisationId.ToInt32());

            var mockedSubmissionBusinessLogicToSetup = new Mock<ISubmissionBusinessLogic>();
            mockedSubmissionBusinessLogicToSetup
                .Setup(x => x.GetSubmissionByOrganisationAndYear(It.IsAny<Organisation>(), It.IsAny<int>()))
                .Throws(new Exception("Kaboom"));

            controller.SubmissionBusinessLogic = mockedSubmissionBusinessLogicToSetup.Object;

            // Act
            var result = controller.Report(obfuscatedOrganisationId, Global.FirstReportingYear + 1) as ViewResult;
            Assert.NotNull(result);
            var model = result.Model as ErrorViewModel;
            Assert.NotNull(model);

            // Assert
            Assert.AreEqual("CustomError", result.ViewName);
            Assert.AreEqual(400, model.ErrorCode);
            Assert.AreEqual("Please try again later", model.Description);
            Assert.AreEqual("Something’s gone wrong", model.Title);
            Assert.AreEqual("Continue", model.ActionText);
        }

        [Test]
        public void ViewingController_Report_OK_returns_CorrectViewAndModel()
        {
            // Arrange
            var org = new Organisation {
                OrganisationId = 202,
                Status = OrganisationStatuses.Active,
                SectorType = Numeric.Rand(0, 1) == 0 ? SectorTypes.Private : SectorTypes.Public
            };

            DateTime testAccountingDate = org.SectorType.GetAccountingStartDate(Global.FirstReportingYear);

            org.OrganisationScopes = new[] {
                new OrganisationScope {
                    SnapshotDate = testAccountingDate, ScopeStatus = ScopeStatuses.OutOfScope, Status = ScopeRowStatuses.Active
                }
            };

            var report = new Return {
                ReturnId = 101,
                OrganisationId = org.OrganisationId,
                Status = ReturnStatuses.Submitted,
                Organisation = org,
                AccountingDate = testAccountingDate,
                DiffMeanBonusPercent = 10,
                DiffMeanHourlyPayPercent = 11,
                DiffMedianBonusPercent = 12,
                DiffMedianHourlyPercent = 13,
                FemaleLowerPayBand = 14,
                FemaleMedianBonusPayPercent = 15,
                FemaleMiddlePayBand = 16,
                FemaleUpperPayBand = 17,
                FemaleUpperQuartilePayBand = 18,
                MaleLowerPayBand = 19,
                MaleMedianBonusPayPercent = 20,
                MaleMiddlePayBand = 21,
                MaleUpperPayBand = 22,
                MaleUpperQuartilePayBand = 23,
                JobTitle = "JobTitle101",
                FirstName = "FirstName101",
                LastName = "LastName101",
                CompanyLinkToGPGInfo = "CompanyLinkToGPGInfo101",
                MinEmployees = 1000,
                MaxEmployees = 4999
            };

            var expectedModel = new ReturnViewModel();
            expectedModel.SectorType = report.Organisation.SectorType;
            expectedModel.ReturnId = report.ReturnId;
            expectedModel.OrganisationId = report.OrganisationId;
            expectedModel.EncryptedOrganisationId = report.Organisation.GetEncryptedId();
            expectedModel.DiffMeanBonusPercent = report.DiffMeanBonusPercent;
            expectedModel.DiffMeanHourlyPayPercent = report.DiffMeanHourlyPayPercent;
            expectedModel.DiffMedianBonusPercent = report.DiffMedianBonusPercent;
            expectedModel.DiffMedianHourlyPercent = report.DiffMedianHourlyPercent;
            expectedModel.FemaleLowerPayBand = report.FemaleLowerPayBand;
            expectedModel.FemaleMedianBonusPayPercent = report.FemaleMedianBonusPayPercent;
            expectedModel.FemaleMiddlePayBand = report.FemaleMiddlePayBand;
            expectedModel.FemaleUpperPayBand = report.FemaleUpperPayBand;
            expectedModel.FemaleUpperQuartilePayBand = report.FemaleUpperQuartilePayBand;
            expectedModel.MaleLowerPayBand = report.MaleLowerPayBand;
            expectedModel.MaleMedianBonusPayPercent = report.MaleMedianBonusPayPercent;
            expectedModel.MaleMiddlePayBand = report.MaleMiddlePayBand;
            expectedModel.MaleUpperPayBand = report.MaleUpperPayBand;
            expectedModel.MaleUpperQuartilePayBand = report.MaleUpperQuartilePayBand;
            expectedModel.JobTitle = report.JobTitle;
            expectedModel.FirstName = report.FirstName;
            expectedModel.LastName = report.LastName;
            expectedModel.CompanyLinkToGPGInfo = report.CompanyLinkToGPGInfo;
            expectedModel.AccountingDate = report.AccountingDate;

            expectedModel.Address = report.Organisation.GetLatestAddress()?.GetAddressString();
            expectedModel.LatestAddress = report.Organisation.GetLatestAddress()?.GetAddressString();
            if (expectedModel.Address.EqualsI(expectedModel.LatestAddress))
            {
                expectedModel.LatestAddress = null;
            }

            expectedModel.OrganisationName = report.Organisation.GetName(report.StatusDate)?.Name ?? report.Organisation.OrganisationName;
            expectedModel.LatestOrganisationName = report.Organisation.OrganisationName;
            if (expectedModel.OrganisationName.EqualsI(expectedModel.LatestOrganisationName))
            {
                expectedModel.LatestOrganisationName = null;
            }

            expectedModel.Sector = report.Organisation.GetSicSectorsString(report.StatusDate);
            expectedModel.LatestSector = report.Organisation.GetSicSectorsString();
            if (expectedModel.Sector.EqualsI(expectedModel.LatestSector))
            {
                expectedModel.LatestSector = null;
            }

            expectedModel.OrganisationSize = report.OrganisationSize;
            expectedModel.Modified = report.Modified;

            expectedModel.IsInScopeForThisReportYear = false; // as org.scope is "out of scope", this MUST be false.

            expectedModel.EHRCResponse = "False";

            var controller = UiTestHelper.GetController<ViewingController>(default, null, org, report);
            string obfuscatedOrganisationId = org.GetEncryptedId();

            Mock<IObfuscator> mockedObfuscatorToSetup = AutoFacExtensions.ResolveAsMock<IObfuscator>();
            mockedObfuscatorToSetup
                .Setup(x => x.DeObfuscate(obfuscatedOrganisationId))
                .Returns(org.OrganisationId.ToInt32());

            // Act
            var result = controller.Report(obfuscatedOrganisationId, Global.FirstReportingYear) as ViewResult;
            Assert.NotNull(result);
            var model = result.Model as ReturnViewModel;
            Assert.NotNull(model);

            // Assert
            Assert.AreEqual("EmployerDetails/Report", result.ViewName);
            expectedModel.Compare(model);
        }

        #endregion

    }
}
