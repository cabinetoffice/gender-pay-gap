using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core.Models.Downloadable;
using GenderPayGap.WebUI.Controllers.Administration;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Administration
{
    [TestFixture]
    public class DownloadableFileControllerTests
    {

        private DownloadableFileController _TestDownloadableFileController;
        private IContainer IocContainer;

        [OneTimeSetUp]
        public void WarmUpTests()
        {
            IocContainer = UiTestHelper.BuildContainerIoC();
        }

        [TestCase(null, "filePath")]
        [TestCase("", "filePath")]
        [TestCase("     ", "filePath")]
        public async Task POST_Download_When_An_Argument_Is_Not_Provided_Returns_Bad_Request_ArgumentNull(string filePath,
            string argumentToBeLoggedAsMissing)
        {
            // Arrange
            var downloadableFileBusinessLogic = IocContainer.Resolve<IDownloadableFileBusinessLogic>();

            _TestDownloadableFileController = new DownloadableFileController(
                null,
                null,
                null,
                null,
                downloadableFileBusinessLogic);

            // Act
            IActionResult actualResult = await _TestDownloadableFileController.DownloadFile(filePath);
            Assert.NotNull(actualResult);
            var expectedBadRequestResult = actualResult as BadRequestResult;

            // Assert
            Assert.NotNull(expectedBadRequestResult, $"expected a BadRequestResult but was {actualResult.GetType()}");
            Assert.AreEqual((int) HttpStatusCode.BadRequest, expectedBadRequestResult.StatusCode);
        }

        [TestCase("unavailableDirectory/someFile.csv", "someFile")]
        [TestCase("/aFile.csv", "aFile")]
        [TestCase("unavailableFile.csv", "directoryPath")]
        public async Task POST_Download_When_Directory_Is_Not_Available_Returns_DirectoryNotFound(string filePath,
            string fileNameReportedOnError)
        {
            // Arrange
            var downloadableFileBusinessLogic = IocContainer.Resolve<IDownloadableFileBusinessLogic>();

            _TestDownloadableFileController = new DownloadableFileController(
                null,
                null,
                null,
                null,
                downloadableFileBusinessLogic);

            // Act
            IActionResult actualResult = await _TestDownloadableFileController.DownloadFile(filePath);
            Assert.NotNull(actualResult);
            var expectedNotFoundResult = actualResult as NotFoundResult;

            // Assert
            Assert.NotNull(expectedNotFoundResult, $"expected a NotFoundResult but was {actualResult.GetType()}");
            Assert.AreEqual((int) HttpStatusCode.NotFound, expectedNotFoundResult.StatusCode);
        }

        [Test]
        public async Task POST_Download_When_File_Is_Not_Available_Returns_FileNotFoundException()
        {
            // Arrange

            var configurableDownloadableFileBusinessLogic = new Mock<IDownloadableFileBusinessLogic>();
            configurableDownloadableFileBusinessLogic
                .Setup(x => x.GetFileRemovingSensitiveInformationAsync(It.IsAny<string>()))
                .ThrowsAsync(new FileNotFoundException());

            _TestDownloadableFileController = new DownloadableFileController(
                null,
                null,
                null,
                null,
                configurableDownloadableFileBusinessLogic.Object);

            // Act
            IActionResult actualResult = await _TestDownloadableFileController.DownloadFile("AFilePath");
            Assert.NotNull(actualResult);
            var expectedNotFoundResult = actualResult as NotFoundResult;

            // Assert
            Assert.NotNull(expectedNotFoundResult, $"expected a NotFoundResult but was {actualResult.GetType()}");
            Assert.AreEqual((int) HttpStatusCode.NotFound, expectedNotFoundResult.StatusCode);
        }

        [Test]
        public async Task POST_Download_When_File_Is_Available_Returns_FileContentResult()
        {
            // Arrange
            var configurableDownloadableFileBusinessLogic = new Mock<IDownloadableFileBusinessLogic>();
            configurableDownloadableFileBusinessLogic
                .Setup(x => x.GetFileRemovingSensitiveInformationAsync(It.IsAny<string>()))
                .ReturnsAsync(new DownloadableFileModel("someName") {DataTable = new DataTable()});

            _TestDownloadableFileController = new DownloadableFileController(
                null,
                null,
                null,
                null,
                configurableDownloadableFileBusinessLogic.Object);

            // Act
            IActionResult actualResult = await _TestDownloadableFileController.DownloadFile("AFilePath");

            // Assert
            Assert.NotNull(actualResult);
            var expectedFileContentResult = actualResult as FileContentResult;
            Assert.NotNull(expectedFileContentResult, $"expected a expectedFileContentResult but was {actualResult.GetType()}");
            Assert.AreEqual("someName", expectedFileContentResult.FileDownloadName);
        }

    }
}
