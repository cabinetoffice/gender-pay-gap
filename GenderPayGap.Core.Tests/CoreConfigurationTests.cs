using System;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests
{
    [TestFixture]
    public class CoreConfigurationTests
    {

        [SetUp]
        public void Setup()
        {
            VirtualDateTime.Initialise(Config.OffsetCurrentDateTimeForSite());
        }

        [Test]
        public void APIConfiguration_Test_Has_OffsetCurrentDateTimeForSite_Configured_To_Zero_Days_Ahead()
        {
            // Arrange
            DateTime expectedDateTime = DateTime.Now;
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

    }
}
