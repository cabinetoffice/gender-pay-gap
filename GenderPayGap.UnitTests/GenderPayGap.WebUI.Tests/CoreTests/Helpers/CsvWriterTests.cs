using System;
using System.IO;
using GenderPayGap.WebUI.Helpers;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.Helpers
{
    [TestFixture]
    public class CsvWriterTests
    {

        [TestCase('-')]
        [TestCase('+')]
        [TestCase('@')]
        [TestCase('=')]
        public void CsvWriter_Sanitizes_Strings_That_Start_With(char character)
        {
            // Arrange
            var value = character + "test";
            var expectedCsvRow = "\t" + character + $"test{System.Environment.NewLine}";

            // Act
            var actualCsvRow = CsvWriter.Write(WriteValue(value));

            // Assert
            Assert.AreEqual(expectedCsvRow, actualCsvRow);
        }

        [Test]
        public void CsvWriter_Does_Not_Sanitize_Negative_Values()
        {
            // Arrange
            var value = -10.2;
            var expectedCsvRow = $"-10.2{System.Environment.NewLine}";

            // Act
            var actualCsvRow = CsvWriter.Write(WriteValue(value));

            // Assert
            Assert.AreEqual(expectedCsvRow, actualCsvRow);
        }

        [Test]
        public void CsvWriter_Does_Not_Sanitize_Strings_That_Do_Not_Start_With_Injection_Character()
        {
            // Arrange
            var value = "Test - string that doesn't start with an injection character";
            var expectedCsvRow = $"Test - string that doesn't start with an injection character{System.Environment.NewLine}";

            // Act
            var actualCsvRow = CsvWriter.Write(WriteValue(value));

            // Assert
            Assert.AreEqual(expectedCsvRow, actualCsvRow);
        }

        private Func<MemoryStream, StreamReader, StreamWriter, CsvHelper.CsvWriter, string> WriteValue<T>(T value)
        {
            return (memoryStream, streamReader, streamWriter, csvWriter) =>
            {
                csvWriter.WriteField(value);
                csvWriter.NextRecord();
                streamWriter.Flush();
                memoryStream.Position = 0;
                return streamReader.ReadToEnd();
            };
        }

    }
}
