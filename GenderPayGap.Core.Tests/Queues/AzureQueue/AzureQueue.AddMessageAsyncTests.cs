using System;
using System.Text;
using System.Threading.Tasks;
using GenderPayGap.Core.Interfaces;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.AzureQueue
{

    [TestFixture]
    public class AddMessageAsyncTests
    {

        [SetUp]
        public void BeforeEach()
        {
            testAzureQueue = new Core.Classes.Queues.AzureQueue(testConnectionString, testQueueName);
        }

        private readonly string testConnectionString = "LogEventUnitTests";
        private readonly string testQueueName = "LogEventUnitTests";
        private Core.Classes.Queues.AzureQueue testAzureQueue;

        [TestCase]
        public void ThrowsWhenInstanceIsNull()
        {
            // Act
            var actualExpection = Assert.ThrowsAsync<ArgumentNullException>(async () => await testAzureQueue.AddMessageAsync<object>(null));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: instance", actualExpection.Message);
        }

        [TestCase]
        public void ThrowsWhenInstanceIsDefault()
        {
            // Act
            var actualExpection =
                Assert.ThrowsAsync<ArgumentNullException>(async () => await testAzureQueue.AddMessageAsync(default(object)));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: instance", actualExpection.Message);
        }

        [TestCase("")]
        [TestCase("  ")]
        [TestCase(null)]
        public void ThrowsWhenMessageIsIllegal(string testMessage)
        {
            // Act
            var actualExpection = Assert.ThrowsAsync<ArgumentNullException>(async () => await testAzureQueue.AddMessageAsync(testMessage));

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: message", actualExpection.Message);
        }

        [TestCase]
        public void ThrowsWhenLargeMessageAndNotSupported()
        {
            // Arrange
            var testLargeMessage = new string('a', 66000);

            // Act
            var actualExpection = Assert.ThrowsAsync<ArgumentException>(async () => await testAzureQueue.AddMessageAsync(testLargeMessage));

            // Assert
            Assert.AreEqual(
                $"AzureQueue: Queue message exceeds maximum message size for azure queues. The message size was {testLargeMessage.Length}. Azure's maximum message size is 65536.\r\nParameter name: message",
                actualExpection.Message);
        }

        [TestCase]
        public void WritesLargeMessagesToStorage()
        {
            // Arrange
            var WriteAsyncAsyncWasCalled = false;
            var mockFileRepo = new Mock<IFileRepository>();
            var testLargeMessage = new string('a', 66000);
            testAzureQueue = new Core.Classes.Queues.AzureQueue(testConnectionString, testQueueName, mockFileRepo.Object);

            mockFileRepo.Setup(f => f.WriteAsync(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback(
                    (string filePath, byte[] data) => {
                        // Assert
                        Assert.IsTrue(filePath.StartsWith("LargeQueueFiles\\LogEventUnitTests\\"), "Expected large filepath to match");
                        Assert.IsTrue(filePath.EndsWith(".json"), "Expected large filepath to be a json file");

                        Assert.AreEqual(testLargeMessage, Encoding.UTF8.GetString(data), "Expected large message to be the same");

                        WriteAsyncAsyncWasCalled = true;
                    })
                .Returns(Task.CompletedTask);

            // Act (We expect an exception here because we cant actually connect to azure so we get a connection error)
            Assert.ThrowsAsync<FormatException>(async () => await testAzureQueue.AddMessageAsync(testLargeMessage));

            // Assert
            Assert.IsTrue(WriteAsyncAsyncWasCalled, "Expected IFileRepository.WriteAsyncAsync to be called.");
        }

    }
}
