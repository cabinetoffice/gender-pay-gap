using System;
using System.Net;
using System.Threading.Tasks;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Models.HttpResultModels;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.ErrorHandling;
using GenderPayGap.WebUI.Models;
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

        #endregion

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
            var result = Assert.Throws<PageNotFoundException>(() => controller.Report("dxDN£34MdgC", Global.FirstReportingYear));
            Assert.NotNull(result);

            // Assert
            Assert.AreEqual("../Errors/404", result.ViewName);
            Assert.AreEqual(404, result.StatusCode);
        }

        [Test]
        public void ViewingController_Report_ZeroOrgId_returns_BadRequest()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();
            string obfuscatedZeroOrganisationId = new InternalObfuscator().Obfuscate(0);

            // Act
            var result = Assert.Throws<PageNotFoundException>(() => controller.Report(obfuscatedZeroOrganisationId, Global.FirstReportingYear));

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
        }

        [Test]
        public void ViewingController_Report_NoEmployer_returns_NotFound()
        {
            // Arrange
            var controller = UiTestHelper.GetController<ViewingController>();
            string obfuscatedEmployerId = ConfigureObfuscator(1548);

            // Act
            var result = Assert.Throws<PageNotFoundException>(() => controller.Report(obfuscatedEmployerId, Global.FirstReportingYear));

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
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
            var result = Assert.Throws<PageNotFoundException>(() => controller.Report(org.GetEncryptedId(), Global.FirstReportingYear));

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
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
            var result = Assert.Throws<PageNotFoundException>(() => controller.Report(org.GetEncryptedId(), Global.FirstReportingYear));

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
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
            var result = Assert.Throws<PageNotFoundException>(() => controller.Report(org.GetEncryptedId(), report1.AccountingDate.Year));

            // Assert
            Assert.NotNull(result, "Expected HttpStatusViewResult");
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
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
            var result = Assert.Throws<PageNotFoundException>(() => controller.Report(obfuscatedOrganisationId, Global.FirstReportingYear + 1));

            // Assert
            Assert.NotNull(result);
            Assert.AreEqual((int) HttpStatusCode.NotFound, result.StatusCode);
        }

        #endregion

    }
}
