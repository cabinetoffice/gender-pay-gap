using GenderPayGap.Core;
using GenderPayGap.WebUI.Classes.Presentation;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace GenderPayGap.Tests.Services.Compare
{

    public class LoadsAndSavesCookieTests
    {

        private readonly Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();

        private readonly Mock<IHttpContextAccessor> mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

        [SetUp]
        public void BeforeEach()
        {
            mockHttpContext.Setup(x => x.Request.Cookies.ContainsKey(It.Is<string>(arg => arg == CookieNames.LastCompareQuery)))
                .Returns(true);

            mockHttpContextAccessor.SetupGet(x => x.HttpContext).Returns(mockHttpContext.Object);
        }

        [TestCase]
        [TestCase("AAAAAAAA", "BBBBBBBB", "CCCCCCCC", "DDDDDDDD", "FFFFFFFF")]
        public void LoadsEmployersFromCookie(params string[] expectedEmployerIds)
        {
            // Arrange
            mockHttpContext.Setup(x => x.Request.Cookies[It.Is<string>(arg => arg == CookieNames.LastCompareQuery)])
                .Returns(string.Join(",", expectedEmployerIds));

            var testService = new CompareViewService(
                mockHttpContextAccessor.Object);

            // Act
            testService.LoadComparedEmployersFromCookie();

            // Assert
            Assert.AreEqual(
                expectedEmployerIds.Length,
                testService.BasketItemCount,
                $"Expected basket to contain {expectedEmployerIds.Length} employers");
            Assert.IsTrue(expectedEmployerIds.All(eId => testService.ComparedEmployers.Contains(eId)), "Expected employer ids to match basket items");
        }

    }

}
