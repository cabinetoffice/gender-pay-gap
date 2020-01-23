using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.LogEventLoggerProvider
{

    [TestFixture]
    public class ConstructorTests
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
            var actualExpection =
                Assert.Throws<ArgumentNullException>(() => new Core.Classes.LogEventLoggerProvider(mockQueue.Object, testAppName, null));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: applicationName", actualExpection.Message);
        }

        [TestCase]
        public void ThrowsWhenQueueIsNull()
        {
            // Act
            var actualExpection = Assert.Throws<ArgumentNullException>(
                () => new Core.Classes.LogEventLoggerProvider(null, "TestApplicationName", Options.Create(new LoggerFilterOptions())));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: queue", actualExpection.Message);
        }

    }


}
