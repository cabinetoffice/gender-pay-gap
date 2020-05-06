using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using GenderPayGap.BusinessLogic.Services;
using GenderPayGap.Core.Classes.Downloadable;
using GenderPayGap.Core.Interfaces.Downloadable;
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

        [TestCase(null, "Logs/GenderPayGap.WebUI")]
        [TestCase("", "Logs/GenderPayGap.WebUI")]
        [TestCase("   ", "Logs/GenderPayGap.WebUI")]
        [TestCase("Some/Given/Path", "Some/Given/Path")]
        public async Task POST_WebsiteLogs_Defaults_To_Base_Logs_Path_When_Not_Provided(string filePath, string expectedPath)
        {
            // Arrange
            string actualPath = string.Empty;
            var configurableDownloadableFileBusinessLogic = new Mock<IDownloadableFileBusinessLogic>();

            configurableDownloadableFileBusinessLogic
                .Setup(x => x.GetListOfDownloadableItemsFromPathAsync(It.IsAny<string>()))
                .Callback((string fp) => { actualPath = fp; })
                .ReturnsAsync(new List<IDownloadableItem>());

            _TestDownloadableFileController = new DownloadableFileController(
                null,
                null,
                null,
                null,
                configurableDownloadableFileBusinessLogic.Object);

            // Act
            await _TestDownloadableFileController.WebsiteLogs(filePath);

            // Assert
            Assert.AreEqual(expectedPath, actualPath);
        }

        [Test]
        public async Task POST_WebsiteLogs_Returns_Valid_List_Of_Downloadable_Items()
        {
            // Arrange
            ConfigureDownloadableFileController();

            // Act
            IActionResult actualResult = await _TestDownloadableFileController.WebsiteLogs("WebsiteLogsPath");
            Assert.NotNull(actualResult);
            var expectedViewResult = actualResult as ViewResult;
            Assert.NotNull(expectedViewResult);

            // Assert
            var actualDownloadableItems = expectedViewResult.Model as List<IDownloadableItem>;
            Assert.NotNull(
                actualDownloadableItems,
                "Expected the model to have returned an list of downloadable items [IEnumerable<IDownloadableItem>]");
            Assert.AreEqual(3, actualDownloadableItems.Count, "Expected the tree configured items to have been returned");
            IDownloadableItem directoryOne = actualDownloadableItems.Find(x => x.Filepath == "Directory1");
            Assert.NotNull(directoryOne);
            Assert.AreEqual(
                "Directory1",
                directoryOne.Filepath,
                "Expected Directory1 to have been returned as it was configured as a response at the beginning of this test");
        }

        [TestCase(null, "Logs/GenderPayGap.WebJob")]
        [TestCase("", "Logs/GenderPayGap.WebJob")]
        [TestCase("   ", "Logs/GenderPayGap.WebJob")]
        [TestCase("Some/Given/Path", "Some/Given/Path")]
        public async Task POST_WebjobLogs_Defaults_To_Base_Logs_Path_When_Not_Provided(string filePath, string expectedPath)
        {
            // Arrange
            string actualPath = string.Empty;
            var configurableDownloadableFileBusinessLogic = new Mock<IDownloadableFileBusinessLogic>();

            configurableDownloadableFileBusinessLogic
                .Setup(x => x.GetListOfDownloadableItemsFromPathAsync(It.IsAny<string>()))
                .Callback((string fp) => { actualPath = fp; })
                .ReturnsAsync(new List<IDownloadableItem>());

            _TestDownloadableFileController = new DownloadableFileController(
                null,
                null,
                null,
                null,
                configurableDownloadableFileBusinessLogic.Object);

            // Act
            await _TestDownloadableFileController.WebjobLogs(filePath);

            // Assert
            Assert.AreEqual(expectedPath, actualPath);
        }

        [Test]
        public async Task POST_WebjobLogs_Returns_Valid_List_Of_Downloadable_Items()
        {
            // Arrange
            ConfigureDownloadableFileController();

            // Act
            IActionResult actualResult = await _TestDownloadableFileController.WebjobLogs("WebjobLogsPath");
            Assert.NotNull(actualResult);
            var expectedViewResult = actualResult as ViewResult;
            Assert.NotNull(expectedViewResult);

            // Assert
            var actualDownloadableItems = expectedViewResult.Model as List<IDownloadableItem>;
            Assert.NotNull(
                actualDownloadableItems,
                "Expected the model to have returned an list of downloadable items [IEnumerable<IDownloadableItem>]");
            Assert.AreEqual(3, actualDownloadableItems.Count, "Expected the tree configured items to have been returned");
            IDownloadableItem directoryTwo = actualDownloadableItems.Find(x => x.Filepath == "Directory2");
            Assert.NotNull(directoryTwo);
            Assert.AreEqual(
                "Directory2",
                directoryTwo.Filepath,
                "Expected Directory2 to have been returned as it was configured as a response at the beginning of this test");
        }

        [TestCase(null, "Logs/GenderPayGap.IdentityServer4")]
        [TestCase("", "Logs/GenderPayGap.IdentityServer4")]
        [TestCase("   ", "Logs/GenderPayGap.IdentityServer4")]
        [TestCase("Some/Given/Path", "Some/Given/Path")]
        public async Task POST_IdentityLogs_Defaults_To_Base_Logs_Path_When_Not_Provided(string filePath, string expectedPath)
        {
            // Arrange
            string actualPath = string.Empty;
            var configurableDownloadableFileBusinessLogic = new Mock<IDownloadableFileBusinessLogic>();

            configurableDownloadableFileBusinessLogic
                .Setup(x => x.GetListOfDownloadableItemsFromPathAsync(It.IsAny<string>()))
                .Callback((string fp) => { actualPath = fp; })
                .ReturnsAsync(new List<IDownloadableItem>());

            _TestDownloadableFileController = new DownloadableFileController(
                null,
                null,
                null,
                null,
                configurableDownloadableFileBusinessLogic.Object);

            // Act
            await _TestDownloadableFileController.IdentityLogs(filePath);

            // Assert
            Assert.AreEqual(expectedPath, actualPath);
        }

        [Test]
        public async Task POST_IdentityLogs_Returns_Valid_List_Of_Downloadable_Items()
        {
            // Arrange
            ConfigureDownloadableFileController();

            // Act
            IActionResult actualResult = await _TestDownloadableFileController.IdentityLogs("IdentityLogsPath");
            Assert.NotNull(actualResult);
            var expectedViewResult = actualResult as ViewResult;
            Assert.NotNull(expectedViewResult);

            // Assert
            var actualDownloadableItems = expectedViewResult.Model as List<IDownloadableItem>;
            Assert.NotNull(
                actualDownloadableItems,
                "Expected the model to have returned an list of downloadable items [IEnumerable<IDownloadableItem>]");
            Assert.AreEqual(3, actualDownloadableItems.Count, "Expected the tree configured items to have been returned");
            IDownloadableItem fileOne = actualDownloadableItems.Find(x => x.Filepath == "file1");
            Assert.NotNull(fileOne);
            Assert.AreEqual(
                "file1",
                fileOne.Filepath,
                "Expected file1 to have been returned as it was configured as a response at the beginning of this test");
        }

        private void ConfigureDownloadableFileController()
        {
            var listOfDownloadableItems = new List<IDownloadableItem> {
                new DownloadableDirectory("Directory1"), new DownloadableDirectory("Directory2"), new DownloadableFile("file1")
            };

            var expectedLogsPath = "";

            var configurableDownloadableFileBusinessLogic = new Mock<IDownloadableFileBusinessLogic>();
            configurableDownloadableFileBusinessLogic
                .Setup(x => x.GetListOfDownloadableItemsFromPathAsync(It.IsAny<string>()))
                .ReturnsAsync(listOfDownloadableItems);

            _TestDownloadableFileController = new DownloadableFileController(
                null,
                null,
                null,
                null,
                configurableDownloadableFileBusinessLogic.Object);
        }

    }
}
