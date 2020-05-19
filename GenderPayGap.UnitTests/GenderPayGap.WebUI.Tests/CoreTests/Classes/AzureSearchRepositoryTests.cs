using System;
using GenderPayGap.Core.Classes;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.Classes
{
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class AzureSearchRepositoryTests
    {

        [SetUp]
        public void BeforeEach() { }

        [Test]
        public void AzureSearchRepository_Ctor_When_ServiceName_Is_Null_Then_Throw_ArgumentNullException()
        {
            // Arrange
            string nullTestServiceName = null;

            // Act
            TestDelegate testDelegate = () => new AzureSearchRepository(nullTestServiceName);

            // Assert
            Assert.That(
                testDelegate,
                Throws.TypeOf<ArgumentNullException>()
                    .With
                    .Message
                    .Contains("Value cannot be null. (Parameter 'serviceName')"));
        }

    }
}
