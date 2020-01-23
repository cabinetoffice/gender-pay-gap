using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Database;
using GenderPayGap.Tests.TestHelpers;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Controllers.Administration;
using GenderPayGap.WebUI.Models.Admin;
using GenderPayGap.WebUI.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.Controllers.Administration
{
    [TestFixture]
    public class AdminControllerUploadsTests
    {

        private const string UPLOAD_COMMAND = "Upload";
        private AdminController _TestAdminController;

        [SetUp]
        public void InitialiseTestObjectsBeforeEachTest()
        {
            User testUser = UserHelper.GetSuperAdmin();
            var thisTestDbObjects = new object[] {testUser};
            _TestAdminController = UiTestHelper.GetController<AdminController>(testUser.UserId, null, thisTestDbObjects);
        }

        [TearDown]
        public void ResetTestObjectsAfterEachTest()
        {
            _TestAdminController = null;
        }

        [Test]
        public async Task AdminController_POST_Upload_When_File_List_Is_Empty_Returns_No_File_Uploaded()
        {
            // Arrange
            _TestAdminController.StashModel(new UploadViewModel());

            // Act
            IActionResult result = await _TestAdminController.Uploads(new List<IFormFile>(), UPLOAD_COMMAND);

            // Assert
            Assert.NotNull(result, "Expected an object returning from the POST call to 'Uploads'");
            Assert.GreaterOrEqual(
                1,
                _TestAdminController.ModelState[""].Errors.Count,
                "Expected at least one error object to have been returned from 'Uploads', since this test is sending an empty list of files to upload.");
            Assert.AreEqual("No file uploaded", _TestAdminController.ModelState[""].Errors[0].ErrorMessage);
        }

        [Test]
        public async Task AdminController_POST_Upload_When_FileName_Does_Not_Match_Returns_Invalid_Filename()
        {
            // Arrange
            var someFileName = "SomeFilename";
            var uploadObject = new UploadViewModel.Upload {Filepath = someFileName + "InvalidFilename" + ".csv"};

            var uploadViewModel = new UploadViewModel();
            uploadViewModel.Uploads.Add(uploadObject);

            _TestAdminController.StashModel(uploadViewModel);

            var fileToUpload = Mock.Of<IFormFile>(f => f.FileName == someFileName);

            var listOfFiles = new List<IFormFile>();
            listOfFiles.Add(fileToUpload);

            // Act
            IActionResult result = await _TestAdminController.Uploads(listOfFiles, UPLOAD_COMMAND);

            // Assert
            Assert.NotNull(result, "Expected an object returning from the POST call to 'Uploads'");
            Assert.GreaterOrEqual(
                1,
                _TestAdminController.ModelState[""].Errors.Count,
                "Expected at least one error object to have been returned from 'Uploads', since this test is sending an invalid file to upload.");
            Assert.AreEqual($"Invalid filename '{fileToUpload.FileName}'", _TestAdminController.ModelState[""].Errors[0].ErrorMessage);
        }

        [Test]
        public async Task AdminController_POST_Upload_When_FileName_Length_Is_Zero_Returns_No_Content_Found()
        {
            // Arrange
            var someFileName = "SomeFilename.csv";
            var uploadObject = new UploadViewModel.Upload {Filepath = someFileName};

            var uploadViewModel = new UploadViewModel();
            uploadViewModel.Uploads.Add(uploadObject);

            _TestAdminController.StashModel(uploadViewModel);

            var fileToUpload = Mock.Of<IFormFile>(
                f => f.FileName == someFileName
                     && f.Length == 0 // Setting needed to make sure the method fails the way this test expects it to
            );

            var listOfFiles = new List<IFormFile>();
            listOfFiles.Add(fileToUpload);

            // Act
            IActionResult result = await _TestAdminController.Uploads(listOfFiles, UPLOAD_COMMAND);

            // Assert
            Assert.NotNull(result, "Expected an object returning from the POST call to 'Uploads'");
            Assert.GreaterOrEqual(
                1,
                _TestAdminController.ModelState[""].Errors.Count,
                "Expected at least one error object to have been returned from 'Uploads', since this test is sending an invalid file to upload.");
            Assert.AreEqual($"No content found in '{fileToUpload.FileName}'", _TestAdminController.ModelState[""].Errors[0].ErrorMessage);
        }

    }
}
