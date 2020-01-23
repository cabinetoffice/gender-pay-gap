using System;
using GenderPayGap.Extensions.AspNetCore;
using NUnit.Framework;

namespace GenderPayGap.Extensions.Tests
{
    [TestFixture]
    public class VirtualDateTimeTests
    {

        [SetUp]
        public void Setup()
        {
            VirtualDateTime.Initialise(Config.OffsetCurrentDateTimeForSite());
        }

        [Test]
        public void VirtualDateTime_Now_Returns_DateTime_Now_Plus_Three_Days_And_One_Hour_When_Configured()
        {
            // Arrange
            DateTime expectedDateTime = DateTime.Now.AddDays(3).AddHours(1);
            DateTime upperBound = expectedDateTime.AddSeconds(2);
            DateTime lowerBound = expectedDateTime.AddSeconds(-2);

            // Act
            DateTime actualDateTime = VirtualDateTime.Now;

            // Assert
            bool expectedResult = actualDateTime > lowerBound && actualDateTime < upperBound;
            string errorMessage =
                $"The dateTime returned from GpgVirtualDateTime.Now was expected to be between {lowerBound} and {upperBound}, but it was actually {actualDateTime}.";

            Assert.IsTrue(expectedResult, errorMessage);
        }

        [Test]
        public void VirtualDateTime_UtcNow_Returns_DateTime_UtcNow_Plus_Three_Days_And_One_Hour_When_Configured()
        {
            // Arrange
            DateTime expectedDateTime = DateTime.UtcNow.AddDays(3).AddHours(1);
            DateTime upperBound = expectedDateTime.AddSeconds(2);
            DateTime lowerBound = expectedDateTime.AddSeconds(-2);

            // Act
            DateTime actualDateTime = VirtualDateTime.UtcNow;

            // Assert
            bool expectedResult = actualDateTime > lowerBound && actualDateTime < upperBound;
            string errorMessage =
                $"The dateTime returned from GpgVirtualDateTime.UtcNow was expected to be between {lowerBound} and {upperBound}, but it was actually {actualDateTime}.";

            Assert.IsTrue(expectedResult, errorMessage);
        }

    }
}
