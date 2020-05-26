using System;
using GenderPayGap.Extensions.AspNetCore;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ConfigTests
    {

        [Test]
        public void Config_DefaultDate_Returns_DateTime_Now_Plus_Three_Days_And_One_Hour_When_Configured()
        { 
            // Arrange
            var expectedOffsetCurrentDateTimeForSite = new TimeSpan(917, 0, 0, 0, 0);

            // Act
            TimeSpan actualOffsetCurrentDateTimeForSite = Config.OffsetCurrentDateTimeForSite();

            // Assert
            Assert.AreEqual(
                expectedOffsetCurrentDateTimeForSite,
                actualOffsetCurrentDateTimeForSite,
                "This value is expected to be configured as 917 days and 0 hours on the 'appsettings.json' file");
        }

    }
}
