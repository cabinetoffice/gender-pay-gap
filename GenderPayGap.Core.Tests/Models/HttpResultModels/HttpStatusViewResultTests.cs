using System;
using System.Net;
using System.Threading.Tasks;
using GenderPayGap.Core.Models.HttpResultModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.Core.Tests.Models.HttpResultModels
{
    [TestFixture]
    public class HttpStatusViewResultTests
    {

        [Ignore("ongoing work, currently reviewing the mocking of 'IServiceProvider' so the test completes ")]
        [TestCase(HttpStatusCode.OK, "message")]
        public async Task Blah(HttpStatusCode httpStatusCode, string expectedLoggedMessage)
        {
            // Arrange
            string actualLoggedMessage = string.Empty;
            var actualLoggedLevel = LogLevel.None;

            #region ActionContext setup

            var configurableLogger = new Mock<ILogger<HttpStatusViewResult>>();
            configurableLogger
                .Setup(
                    x => x.Log(
                        It.IsAny<LogLevel>(),
                        It.IsAny<EventId>(),
                        It.IsAny<object>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<object, Exception, string>>()))
                .Callback(
                    (LogLevel logLevel,
                        EventId eventId,
                        object message,
                        Exception exception,
                        Func<object, Exception, string> formatter) => {
                        actualLoggedLevel = logLevel; // LogLevel.Error
                        // EventId myEventId = eventId;
                        actualLoggedMessage = message.ToString(); // 
                        // Exception myException = exception; // System.ArgumentNullException
                        // loggedExceptionMessage = exception.Message;
                        // Func<object, Exception, string> myFormatter = formatter;
                    });

            // var configurableServiceProvider = new Mock<ISupportRequiredService>();
            var configurableServiceProvider = new Mock<IServiceProvider>();
            configurableServiceProvider
                .Setup(x => x.GetRequiredService(It.IsAny<Type>()))
                .Returns(configurableLogger.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = configurableServiceProvider.Object;

            var actionContext = new ActionContext {
                ActionDescriptor = new ActionDescriptor(), HttpContext = httpContext, RouteData = new RouteData()
            };

            #endregion

            var _httpStatusViewResult = new HttpStatusViewResult(httpStatusCode);

            // Act
            await _httpStatusViewResult.ExecuteResultAsync(actionContext);

            // Assert
            Assert.AreEqual(LogLevel.Error, actualLoggedLevel);
            Assert.AreEqual(expectedLoggedMessage, actualLoggedMessage);
        }

    }
}
