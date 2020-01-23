using System.Collections.Generic;
using System.Threading.Tasks;
using GenderPayGap.Core.Models;
using GenderPayGap.Tests.Common.Classes;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.LogRecordLogger
{

    [TestFixture]
    public class WriteAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            mockQueue = new Mock<Core.Classes.Queues.AzureQueue>("TestConnectionString", "TestQueueName") {CallBase = true};

            mockLogRecordLogger =
                new Mock<Core.Classes.LogRecordLogger>(mockQueue.Object, testApplicationName, testFileName) {CallBase = true};
        }

        private Mock<Core.Classes.Queues.AzureQueue> mockQueue;
        private Mock<Core.Classes.LogRecordLogger> mockLogRecordLogger;

        private readonly string testApplicationName = "LogRecordUnitTests";
        private readonly string testFileName = "LogRecordUnitTest.csv";

        [TestCase]
        public async Task WritesNullColumnsToJson()
        {
            // Arrange
            object testNull = null;
            var AddMessageAsyncWasCalled = false;

            var testRecordModel = new {col1 = 123, col2 = testNull, col3 = "test"};

            mockQueue.Setup(c => c.AddMessageAsync(It.IsAny<string>()))
                .Callback(
                    (string message) => {
                        // Assert
                        Assert.IsTrue(message.Contains("\"col2\":null"), "Expected null columns to be written to the queue");
                        AddMessageAsyncWasCalled = true;
                    })
                .Returns(Task.CompletedTask);

            // Act
            Core.Classes.LogRecordLogger logger = mockLogRecordLogger.Object;
            await logger.WriteAsync(testRecordModel);

            // Assert
            Assert.IsTrue(AddMessageAsyncWasCalled);
        }

        [TestCase]
        public async Task WritesLogRecordWrapperModelToJson()
        {
            // Arrange
            var AddMessageAsyncWasCalled = false;

            var expectedWrapperModel = new LogRecordWrapperModel {
                ApplicationName = testApplicationName, FileName = testFileName, Record = "Some record"
            };

            mockQueue.Setup(c => c.AddMessageAsync(It.IsAny<string>()))
                .Callback(
                    (string message) => {
                        var actualWrapper = JsonConvert.DeserializeObject<LogRecordWrapperModel>(message);

                        // Assert
                        expectedWrapperModel.Compare(actualWrapper);

                        AddMessageAsyncWasCalled = true;
                    })
                .Returns(Task.CompletedTask);

            // Act
            Core.Classes.LogRecordLogger logger = mockLogRecordLogger.Object;
            await logger.WriteAsync(expectedWrapperModel.Record);

            // Assert
            Assert.IsTrue(AddMessageAsyncWasCalled);
        }

        [TestCase("record1", "record2", "record3", "record4", "record5")]
        public async Task WritesListOfLogRecordWrapperModelsToJson(params string[] testRecords)
        {
            // Arrange
            var AddMessageAsyncWasCalled = false;
            var expectedIndex = 0;

            var expectedEnumerableRecords = new List<LogRecordWrapperModel>();
            foreach (string record in testRecords)
            {
                expectedEnumerableRecords.Add(
                    new LogRecordWrapperModel {ApplicationName = testApplicationName, FileName = testFileName, Record = record});
            }

            mockQueue.Setup(c => c.AddMessageAsync(It.IsAny<string>()))
                .Callback(
                    (string message) => {
                        var actualWrapper = JsonConvert.DeserializeObject<LogRecordWrapperModel>(message);

                        // Assert
                        Assert.AreEqual(expectedEnumerableRecords[expectedIndex].ApplicationName, actualWrapper.ApplicationName);
                        Assert.AreEqual(expectedEnumerableRecords[expectedIndex].FileName, actualWrapper.FileName);
                        Assert.AreEqual(expectedEnumerableRecords[expectedIndex].Record, actualWrapper.Record);

                        expectedIndex++;
                        AddMessageAsyncWasCalled = true;
                    })
                .Returns(Task.CompletedTask);

            // Act
            Core.Classes.LogRecordLogger logger = mockLogRecordLogger.Object;
            await logger.WriteAsync(testRecords);

            // Assert
            Assert.IsTrue(AddMessageAsyncWasCalled);
        }

    }


}
