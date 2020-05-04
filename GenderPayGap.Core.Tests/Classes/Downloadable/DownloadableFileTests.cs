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
        [TestCase("filePath\\ErrorLog", "Error logs")]
        [TestCase("filePath\\WarningLog", "Warning logs")]
        [TestCase("filePath\\InfoLog", "Information logs")]
        [TestCase("filePath\\BadSicLog", "Bad SIC Codes Log")]
        [TestCase("filePath/RegistrationLog", "Registration History")]
        [TestCase("filePath/searchLog", "Search logs")]
        [TestCase("filePath/submissionLog", "Submission History")]
        [TestCase("filePath/userLog", "User Logs")]
        [TestCase("filePath/emailSendLog", "Email Send Log")]
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
        [TestCase("aPath/ErrorLog", "Error log from system")]
        [TestCase("aPath/WarningLog", "Warning log from system")]
        [TestCase("aPath/InfoLog", "Information log from system")]
        [TestCase("aPath/BadSicLog", "Companies and their unknown SIC Codes from Companies House.")]
        [TestCase("aPath\\RegistrationLog", "Audit history of approved and rejected registrations.")]
        [TestCase("aPath\\SearchLog", "Searches carried out by users.")]
        [TestCase("aPath\\submissionLog", "Audit history of approved and rejected registrations.")]
        [TestCase("aPath\\userLog", "A list of all user account activity.")]
        [TestCase("aPath\\emailSendLog", "Log of sent email messages via Gov Notify or SendGrid.")]
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
        [TestCase("location\\ErrorLog", "Error log")]
        [TestCase("location\\WarningLog", "Warning log")]
        [TestCase("location\\InfoLog", "Information log")]
        [TestCase("location\\BadSicLog", "Bad SIC Codes Logs")]
        [TestCase("location\\RegistrationLog", "Registration History")]
        [TestCase("location\\SearchLog", "Search log")]
        [TestCase("location\\submissionLog", "Submission History")]
        [TestCase("location\\userLog", "User Activity Log")]
        [TestCase("location\\emailSendLog", "Email Send Log")]
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
