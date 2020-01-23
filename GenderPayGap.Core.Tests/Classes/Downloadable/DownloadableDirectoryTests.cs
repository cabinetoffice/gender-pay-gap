using System.Collections.Generic;
using GenderPayGap.Core.Classes.Downloadable;
using GenderPayGap.Core.Interfaces.Downloadable;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.Classes.Downloadable
{
    [TestFixture]
    public class DownloadableDirectoryTests
    {

        [TestCase(null, "BlahPath")]
        [TestCase("", "BlahPath")]
        [TestCase("/FileName.csv", "FileName.csv")]
        [TestCase("\\BackslashFilename.someExtension", "BackslashFilename.someExtension")]
        public void When_Name_Is_Not_Set_Returns_Last_Section_Of_FilePath(string fileName, string expectedName)
        {
            // Arrange
            string filePath = "Some/File/BlahPath" + fileName;

            var testDownloadableDirectory = new DownloadableDirectory(filePath);

            // Act
            string actualName = testDownloadableDirectory.Name;

            // Assert
            Assert.AreEqual(expectedName, actualName);
        }

        [TestCase("parent/blah", "parent")]
        [TestCase("parent/second/third", "parent/second")]
        [TestCase("/JustARoot", "")]
        [TestCase("parent\\backslash", "parent")]
        [TestCase("parent/second\\BackslashThird", "parent/second")]
        [TestCase("\\JustARoot", "")]
        public void GetSpecialParentFolderInfo_Returns_Correct_Parent_Folder_As_Two_Dots(string directoryPath, string expectedPath)
        {
            // Arrange - Act
            DownloadableDirectory actualDownloadableDirectory = DownloadableDirectory.GetSpecialParentFolderInfo(directoryPath);

            // Assert
            Assert.AreEqual("..", actualDownloadableDirectory.Name);
            Assert.AreEqual(expectedPath, actualDownloadableDirectory.Filepath);
        }

        [Test]
        public void DownloadableItems_Can_Be_Set_And_Get_Correctly()
        {
            // Arrange
            var listOfDownloadableItems =
                new List<IDownloadableItem> {new DownloadableDirectory("firstDirectory"), new DownloadableFile("firstFile")};

            const string someFilePath = "Some/Filepath";

            var testDownloadableDirectory = new DownloadableDirectory(someFilePath) {DownloadableItems = listOfDownloadableItems};

            // Act
            List<IDownloadableItem> actualListOfDownloadableItems = testDownloadableDirectory.DownloadableItems;

            // Assert
            Assert.AreEqual(2, actualListOfDownloadableItems.Count);
        }

        [Test]
        public void When_Name_Is_Set_Returns_Its_Value_Correctly()
        {
            // Arrange
            const string someName = "AFileName.txt";
            const string someFilePath = "Some/Filepath";

            var testDownloadableDirectory = new DownloadableDirectory(someFilePath) {Name = someName};

            // Act
            string actualName = testDownloadableDirectory.Name;

            // Assert
            Assert.AreNotEqual(someFilePath, actualName);
            Assert.AreEqual(someName, actualName);
        }

    }
}
