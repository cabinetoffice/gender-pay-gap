using GenderPayGap.Core.Classes.Downloadable;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.Classes.Downloadable
{
    [TestFixture]
    public class DownloadableFileTests
    {

        [TestCase("", null)]
        [TestCase("/FileName.csv", "FileName.csv")]
        [TestCase("\\BackslashFilename.someExtension", "BackslashFilename.someExtension")]
        public void When_Name_Is_Not_Set_Returns_Last_Section_Of_FilePath(string filePath, string expectedName)
        {
            // Arrange
            var testableDownloadableFile = new DownloadableFile(filePath);

            // Act
            string actualName = testableDownloadableFile.Name;

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [TestCase("", null)]
        [TestCase("filePath/submissionLog", "Submission History")]
        [TestCase("filePath/NotAMappedLog_Name", null)]
        public void Type_Is_Correctly_Determined_From_Filename(string fileName, string expectedType)
        {
            // Arrange
            var testableDownloadableFile = new DownloadableFile(fileName);

            // Act
            string actualType = testableDownloadableFile.Type;

            // Assert
            Assert.AreEqual(expectedType, actualType);
        }

        [TestCase("", null)]
        [TestCase("aPath\\submissionLog", "Audit history of approved and rejected registrations.")]
        [TestCase("aPath\\NotAMappedLog_Name", null)]
        public void Description_Is_Correctly_Determined_From_Filename(string fileName, string expectedDescription)
        {
            // Arrange
            var testableDownloadableFile = new DownloadableFile(fileName);

            // Act
            string actualDescription = testableDownloadableFile.Description;

            // Assert
            Assert.AreEqual(expectedDescription, actualDescription);
        }

        [TestCase("", null)]
        [TestCase("location\\submissionLog", "Submission History")]
        [TestCase("location\\NotAMappedLog_Name", null)]
        public void Title_Is_Correctly_Determined_From_Filename(string fileName, string expectedTitle)
        {
            // Arrange
            var testableDownloadableFile = new DownloadableFile(fileName);

            // Act
            string actualTitle = testableDownloadableFile.Title;

            // Assert
            Assert.AreEqual(expectedTitle, actualTitle);
        }

    }
}
