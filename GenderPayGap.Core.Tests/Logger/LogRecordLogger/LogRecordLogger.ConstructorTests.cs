using System;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.LogRecordLogger
{

    [TestFixture]
    public class ConstrutorTests
    {

        [SetUp]
        public void BeforeEach()
        {
            mockQueue = new Mock<Core.Classes.Queues.AzureQueue>("TestConnectionString", "TestQueueName") {CallBase = true};
        }

        private Mock<Core.Classes.Queues.AzureQueue> mockQueue;

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ThrowsWhenApplicationNameIsIllegal(string testAppName)
        {
            // Act
            var actualExpection = Assert.Throws<ArgumentNullException>(
                () => new Core.Classes.LogRecordLogger(mockQueue.Object, testAppName, "TestFilename"));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: applicationName", actualExpection.Message);
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ThrowsWhenFileNameIsIllegal(string testFilename)
        {
            // Act
            var actualExpection = Assert.Throws<ArgumentNullException>(
                () => new Core.Classes.LogRecordLogger(mockQueue.Object, "TestAppName", testFilename));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: fileName", actualExpection.Message);
        }

        [TestCase]
        public void ThrowsWhenQueueIsNull()
        {
            // Act
            var actualExpection =
                Assert.Throws<ArgumentNullException>(() => new Core.Classes.LogRecordLogger(null, "TestAppName", "TestFilename"));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: queue", actualExpection.Message);
        }

    }

}
