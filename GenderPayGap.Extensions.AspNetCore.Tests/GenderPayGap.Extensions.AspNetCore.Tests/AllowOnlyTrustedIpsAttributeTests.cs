using System;
using System.Collections.Generic;
using System.Net;
using GenderPayGap.Core;
using GenderPayGap.Core.Filters;
using GenderPayGap.Core.Models.HttpResultModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GenderPayGap.API.Tests.Filters
{
    public class AllowOnlyTrustedIpsAttributeTests
    {

        private string _rememberGlobalTrustedIpDomainsValueBeforeEachTest;

        [SetUp]
        public void SetUp()
        {
            _rememberGlobalTrustedIpDomainsValueBeforeEachTest = Global.TrustedIPDomains;
        }

        [TearDown]
        public void TearDown()
        {
            Global.TrustedIPDomains = _rememberGlobalTrustedIpDomainsValueBeforeEachTest;
        }

        [Test]
        public void AllowOnlyTrustedIpsAttribute_Succeeds_When_HttpContext_Is_Not_Null()
        {
            // Arrange
            var ipToTest = "26.0.45.1";
            IPAddress accessingSiteFromIpAddress = IPAddress.Parse(ipToTest);

            var httpConnectionFeature = new Mock<IHttpConnectionFeature>();
            httpConnectionFeature
                .Setup(hcf => hcf.RemoteIpAddress)
                .Returns(accessingSiteFromIpAddress);

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set(httpConnectionFeature.Object);

            var actionContext = new ActionContext {
                ActionDescriptor = new ActionDescriptor(), HttpContext = httpContext, RouteData = new RouteData()
            };

            var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), null);

            Global.TrustedIPDomains = ipToTest + ",68.91.0.0.1,47.217.217.217";

            var allowOnlyTrustedIpsAttribute = new AllowOnlyTrustedIps();

            // Act
            allowOnlyTrustedIpsAttribute.OnActionExecuting(context);

            // Assert
            Assert.Null(context.Result);
        }

        [Test]
        public void AllowOnlyTrustedIpsAttribute_Forbids_When_Unable_To_Read_Host_Address_Info()
        {
            // Arrange
            var httpConnectionFeature = new Mock<IHttpConnectionFeature>();
            httpConnectionFeature
                .Setup(hcf => hcf.RemoteIpAddress)
                .Returns(IPAddress.Parse("34.33.33.1"));

            string actualLoggedMessage = string.Empty;
            var configurableLogger = new Mock<ILogger<AllowOnlyTrustedIps>>();

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
                        // LogLevel myLogLevel = logLevel; // LogLevel.Error
                        // EventId myEventId = eventId;
                        actualLoggedMessage = message.ToString(); // Value cannot be null.\nParameter name: filePath
                        // Exception myException = exception; // System.ArgumentNullException
                        // loggedExceptionMessage = exception.Message;
                        // Func<object, Exception, string> myFormatter = formatter;
                    });

            var configurableServiceProvider = new Mock<IServiceProvider>();
            configurableServiceProvider
                .Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(configurableLogger.Object);

            var httpContext = new DefaultHttpContext();
            //httpContext.Features.Set(httpConnectionFeature.Object);
            httpContext.RequestServices = configurableServiceProvider.Object;

            var actionContext = new ActionContext {
                ActionDescriptor = new ActionDescriptor(), HttpContext = httpContext, RouteData = new RouteData()
            };

            var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), null);

            Global.TrustedIPDomains = "15.0.64.1";

            var allowOnlyTrustedIpsAttribute = new AllowOnlyTrustedIps(AllowOnlyTrustedIps.IpRangeTypes.TrustedIPDomains);

            // Act
            allowOnlyTrustedIpsAttribute.OnActionExecuting(context);
            var expectedHttpStatusCodeResult403 = context.Result as HttpForbiddenResult;

            // Assert
            Assert.NotNull(expectedHttpStatusCodeResult403);
            Assert.AreEqual(HttpStatusCode.Forbidden, (HttpStatusCode) expectedHttpStatusCodeResult403.StatusCode);
            Assert.IsNull(expectedHttpStatusCodeResult403.ContentType);

            var expectedLoggedMessage =
                "Access to an unknown controller was forbidden since it was not possible to read its host address information";
            Assert.AreEqual(expectedLoggedMessage, actualLoggedMessage);
        }

        [Test]
        public void AllowOnlyTrustedIpsAttribute_Forbids_When_Address_Is_Not_On_Trusted_List()
        {
            // Arrange
            var httpConnectionFeature = new Mock<IHttpConnectionFeature>();
            httpConnectionFeature
                .Setup(hcf => hcf.RemoteIpAddress)
                .Returns(IPAddress.Parse("45.33.33.1")); // address NOT on trusted list

            string actualLoggedMessage = string.Empty;
            var configurableLogger = new Mock<ILogger<AllowOnlyTrustedIps>>();

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
                        // LogLevel myLogLevel = logLevel; // LogLevel.Error
                        // EventId myEventId = eventId;
                        actualLoggedMessage = message.ToString(); // Value cannot be null.\nParameter name: filePath
                        // Exception myException = exception; // System.ArgumentNullException
                        // loggedExceptionMessage = exception.Message;
                        // Func<object, Exception, string> myFormatter = formatter;
                    });

            var configurableServiceProvider = new Mock<IServiceProvider>();
            configurableServiceProvider
                .Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(configurableLogger.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set(httpConnectionFeature.Object);
            httpContext.RequestServices = configurableServiceProvider.Object;

            var actionContext = new ActionContext {
                ActionDescriptor = new ActionDescriptor(), HttpContext = httpContext, RouteData = new RouteData()
            };

            var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object>(), null);

            Global.TrustedIPDomains = "25.0.64.1,101.102.103.104";

            var allowOnlyTrustedIpsAttribute = new AllowOnlyTrustedIps(AllowOnlyTrustedIps.IpRangeTypes.TrustedIPDomains);

            // Act
            allowOnlyTrustedIpsAttribute.OnActionExecuting(context);
            var expectedHttpStatusCodeResult403 = context.Result as HttpForbiddenResult;

            // Assert
            Assert.NotNull(expectedHttpStatusCodeResult403);
            Assert.AreEqual(HttpStatusCode.Forbidden, (HttpStatusCode) expectedHttpStatusCodeResult403.StatusCode);
            Assert.IsNull(expectedHttpStatusCodeResult403.ContentType);

            var expectedLoggedMessage =
                "Access to an unknown controller was forbidden for address 45.33.33.1 as it is not part of the configured ips TrustedIPDomains";
            Assert.AreEqual(expectedLoggedMessage, actualLoggedMessage);
        }

        [Test]
        public void AllowOnlyTrustedIpsAttribute_Logs_Controller_Name_If_Known()
        {
            // Arrange
            var httpConnectionFeature = new Mock<IHttpConnectionFeature>();
            httpConnectionFeature
                .Setup(hcf => hcf.RemoteIpAddress)
                .Returns(IPAddress.Parse("96.97.98.99")); // address NOT on trusted list

            string actualLoggedMessage = string.Empty;
            var configurableLogger = new Mock<ILogger<AllowOnlyTrustedIps>>();

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
                        // LogLevel myLogLevel = logLevel; // LogLevel.Error
                        // EventId myEventId = eventId;
                        actualLoggedMessage = message.ToString(); // Value cannot be null.\nParameter name: filePath
                        // Exception myException = exception; // System.ArgumentNullException
                        // loggedExceptionMessage = exception.Message;
                        // Func<object, Exception, string> myFormatter = formatter;
                    });

            var configurableServiceProvider = new Mock<IServiceProvider>();
            configurableServiceProvider
                .Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(configurableLogger.Object);

            var httpContext = new DefaultHttpContext();
            httpContext.Features.Set(httpConnectionFeature.Object);
            httpContext.RequestServices = configurableServiceProvider.Object;

            var actionContext = new ActionContext {
                ActionDescriptor = new ActionDescriptor(), HttpContext = httpContext, RouteData = new RouteData()
            };

            var context = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object>(),
                new OnlyForTestingPleaseIgnoreThisController());

            Global.TrustedIPDomains = "25.26.27.28,31.32.33.34";

            var allowOnlyTrustedIpsAttribute = new AllowOnlyTrustedIps(AllowOnlyTrustedIps.IpRangeTypes.TrustedIPDomains);

            // Act
            allowOnlyTrustedIpsAttribute.OnActionExecuting(context);
            var expectedHttpStatusCodeResult403 = context.Result as HttpForbiddenResult;

            // Assert
            Assert.NotNull(expectedHttpStatusCodeResult403);
            Assert.AreEqual(HttpStatusCode.Forbidden, (HttpStatusCode) expectedHttpStatusCodeResult403.StatusCode);
            Assert.IsNull(expectedHttpStatusCodeResult403.ContentType);

            var expectedLoggedMessage =
                "Access to controller GenderPayGap.API.Tests.Filters.AllowOnlyTrustedIpsAttributeTests+OnlyForTestingPleaseIgnoreThisController was forbidden for address 96.97.98.99 as it is not part of the configured ips TrustedIPDomains";
            Assert.AreEqual(expectedLoggedMessage, actualLoggedMessage);
        }

        private class OnlyForTestingPleaseIgnoreThisController : Controller { }

    }
}
