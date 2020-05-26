using GenderPayGap.Core.Classes;
using GenderPayGap.Extensions.AspNetCore;
using Microsoft.Azure.Search;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.Classes
{
    [TestFixture]
    public class SicCodeSearchRepositoryTests
    {

        [Test]
        public void SicCodeSearchRepository_Can_Be_Created()
        {
            // Arrange
            string sicCodeSearchServiceName = Config.GetAppSetting("SearchService:ServiceName");
            string sicCodeSearchAdminApiKey = Config.GetAppSetting("SearchService:AdminApiKey");

            var sicCodeSearchServiceClient = new SearchServiceClient(
                sicCodeSearchServiceName,
                new SearchCredentials(sicCodeSearchAdminApiKey));

            // Act
            var actualSicCodeSearchRepository = new SicCodeSearchRepository(sicCodeSearchServiceClient);

            // Assert
            Assert.NotNull(
                actualSicCodeSearchRepository,
                "This test should have been able to create a SicCodeSearchRepository object but seems it was unable to do so");
        }

    }
}
