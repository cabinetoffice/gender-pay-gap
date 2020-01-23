using GenderPayGap.Core;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Options;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

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
                MoqHelpers.CreateIOptionsSnapshotMock(new ViewingOptions()),
                mockHttpContextAccessor.Object,
                Mock.Of<IHttpSession>());

            // Act
            testService.LoadComparedEmployersFromCookie();

            // Assert
            Assert.AreEqual(
                expectedEmployerIds.Length,
                testService.BasketItemCount,
                $"Expected basket to contain {expectedEmployerIds.Length} employers");
            Assert.IsTrue(testService.ComparedEmployers.Value.Contains(expectedEmployerIds), "Expected employer ids to match basket items");
        }

        [TestCase]
        public void ClearsBasketBeforeLoadingFromCookie()
        {
            // Arrange
            mockHttpContext.Setup(x => x.Request.Cookies[It.Is<string>(arg => arg == CookieNames.LastCompareQuery)])
                .Returns("12345678");

            var testService = new CompareViewService(
                MoqHelpers.CreateIOptionsSnapshotMock(new ViewingOptions()),
                mockHttpContextAccessor.Object,
                Mock.Of<IHttpSession>());

            var testPreviousIds = new[] {"AAA", "BBB", "CCC"};
            testService.AddRangeToBasket(testPreviousIds);

            // Act
            testService.LoadComparedEmployersFromCookie();

            // Assert
            Assert.AreEqual(1, testService.BasketItemCount, "Expected basket to contain 1 employer");
            Assert.IsFalse(
                testService.ComparedEmployers.Value.Contains(testPreviousIds),
                "Expected previous employer ids to be cleared from basket");
        }

        [TestCase]
        [Ignore("Need to create a mock HttpRequest object to pass to SaveComparedEmployersToCookie, but I don't have time to do this now'")]
        public void SavesComparedEmployersToCookie()
        {
            // Arrange
            var AppendWasCalled = false;
            mockHttpContext.Setup(x => x.Request.Cookies[It.Is<string>(arg => arg == CookieNames.LastCompareQuery)])
                .Returns("12345678");

            var testService = new CompareViewService(
                MoqHelpers.CreateIOptionsSnapshotMock(new ViewingOptions()),
                mockHttpContextAccessor.Object,
                Mock.Of<IHttpSession>());

            var testIds = new[] {"AAA", "BBB", "CCC"};
            testService.AddRangeToBasket(testIds);

            mockHttpContext.Setup(
                    x => x.Response.Cookies.Append(
                        It.Is<string>(arg => arg == CookieNames.LastCompareQuery),
                        It.IsAny<string>(),
                        It.IsAny<CookieOptions>()))
                .Callback(
                    (string key, string value, CookieOptions options) => {
                        // Assert
                        Assert.AreEqual(string.Join(",", testIds), value);
                        AppendWasCalled = true;
                    });

            // Act
            testService.SaveComparedEmployersToCookie(null);

            // Assert
            Assert.IsTrue(AppendWasCalled);
        }

    }

}
