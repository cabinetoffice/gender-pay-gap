using System;
using GenderPayGap.Core.Classes;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.Classes
{
    [TestFixture]
    public class AzureFileRepositoryTests
    {

        [TestCase("")]
        [TestCase(null)]
        public void AzureFileRepository_Constructor_When_ConnectionString_Is_Not_Set_Throws_Exception(string connectionString)
        {
            // Arrange / Act
            var actualException = Assert.Throws<ArgumentNullException>(() => { new AzureFileRepository(connectionString, null); });

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: connectionString", actualException.Message);
        }

        [TestCase("")]
        [TestCase(null)]
        public void AzureFileRepository_Constructor_When_ShareName_Is_Not_Set_Throws_Exception(string shareName)
        {
            // Arrange / Act
            var actualException =
                Assert.Throws<ArgumentNullException>(() => { new AzureFileRepository("some connection string", shareName); });

            // Assert
            Assert.AreEqual("Value cannot be null.\r\nParameter name: shareName", actualException.Message);
        }

    }
}
