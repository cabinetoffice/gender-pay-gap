using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using Autofac;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.BackgroundJobs;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Classes;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Controllers.Admin;
using GenderPayGap.WebUI.Cookies;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Helpers;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Search;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.TestHelpers
{
    public static class UiTestHelper
    {

        public static Mock<IBackgroundJobsApi> MockBackgroundJobsApi;

        public static T GetController<T>(long userId = 0, RouteData routeData = null, params object[] dbObjects)
            where T : Controller
        {
            IContainer DIContainer = BuildContainerIoC(dbObjects);

            //Create Inversion of Control container
            Startup.ContainerIoC = DIContainer;

            //Mock UserId as claim
            var claims = new List<Claim>();
            if (userId < 0)
            {
                User user = dbObjects.OfType<User>().FirstOrDefault();
                if (user == null)
                {
                    foreach (object item in dbObjects)
                    {
                        var enumerable = item as IEnumerable<object>;
                        if (enumerable != null)
                        {
                            user = enumerable.OfType<User>().FirstOrDefault();
                            if (user != null)
                            {
                                break;
                            }
                        }
                    }
                }

                userId = user.UserId;
            }


            if (userId != 0)
            {
                claims.Add(new Claim("user_id", userId.ToString()));
                claims.Add(new Claim(ClaimTypes.Role, LoginRoles.GpgEmployer));
            }

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(m => m.Claims).Returns(claims);
            mockPrincipal.Setup(m => m.Identity.IsAuthenticated).Returns(userId > 0);
            if (userId > 0)
            {
                mockPrincipal.Setup(m => m.Identity.Name).Returns(userId.ToString());
            }

            //Set the Remote IP Address
            var features = new FeatureCollection();
            features.Set<IHttpConnectionFeature>(new HttpConnectionFeature {RemoteIpAddress = IPAddress.Parse("127.0.0.1")});

            //Mock HttpRequest
            var requestMock = new Mock<HttpRequest>();

            var requestHeaders = new HeaderDictionary();
            var requestCookies = new MockRequestCookieCollection(
                new Dictionary<string, string>
                {
                    {
                        "cookie_settings",
                        JsonConvert.SerializeObject(
                            new CookieSettings {
                                GoogleAnalyticsGpg = true, GoogleAnalyticsGovUk = true
                            })
                    }
                });
            requestMock.SetupGet(x => x.Cookies).Returns(requestCookies);
            requestMock.SetupGet(x => x.Headers).Returns(requestHeaders);

            //Done:Added the queryString Dictionary property for mock object
            var queryString = new QueryString("?code=abcdefg");
            requestMock.SetupGet(x => x.QueryString).Returns(queryString);

            var query = new QueryCollection();

            //Mock HttpResponse
            var responseMock = new Mock<HttpResponse>();
            var responseHeaders = new HeaderDictionary();
            var responseCookies = new Mock<IResponseCookies>();
            responseCookies.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            responseCookies.Setup(c => c.Append(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>())).Verifiable();
            responseCookies.Setup(c => c.Delete(It.IsAny<string>())).Verifiable();
            responseCookies.Setup(c => c.Delete(It.IsAny<string>(), It.IsAny<CookieOptions>())).Verifiable();
            responseMock.SetupGet(x => x.Headers).Returns(responseHeaders);
            responseMock.SetupGet(x => x.Cookies).Returns(responseCookies.Object);

            //Mock session
            var uri = new UriBuilder(new Uri("https://localhost/", UriKind.Absolute));

            //Mock HttpContext
            var httpContextMock = new Mock<HttpContext>();
            //contextMock.Setup(m => m.Cache).Returns(HttpRuntime.Cache);
            httpContextMock.Setup(ctx => ctx.User).Returns(mockPrincipal.Object);
            httpContextMock.SetupGet(ctx => ctx.Request.Headers).Returns(requestHeaders);
            httpContextMock.SetupGet(ctx => ctx.Request).Returns(requestMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Request.Cookies).Returns(requestCookies);
            httpContextMock.SetupGet(ctx => ctx.Request.QueryString).Returns(queryString);
            httpContextMock.SetupGet(ctx => ctx.Request.Query).Returns(query);
            httpContextMock.SetupGet(ctx => ctx.Request.HttpContext).Returns(httpContextMock.Object);
            //contextMock.SetupGet(ctx => ctx.Request.GetUri()).Returns(Uri);
            httpContextMock.SetupGet(ctx => ctx.Request.Scheme).Returns(uri.Scheme);
            httpContextMock.SetupGet(ctx => ctx.Request.Host).Returns(new HostString(uri.Host, uri.Port));
            httpContextMock.SetupGet(ctx => ctx.Request.Path).Returns(uri.Path);
            httpContextMock.SetupGet(ctx => ctx.Request.QueryString).Returns(new QueryString(uri.Query));

            httpContextMock.SetupGet(ctx => ctx.Response).Returns(responseMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Response.Cookies).Returns(responseCookies.Object);
            httpContextMock.SetupGet(ctx => ctx.Response.Headers).Returns(responseHeaders);
            httpContextMock.SetupGet(ctx => ctx.Response.HttpContext).Returns(httpContextMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Features).Returns(features);

            //Mock the httpcontext to the controllercontext

            //var controllerContextMock = new Mock<ControllerContext>();
            var controllerContextMock = new ControllerContext {HttpContext = httpContextMock.Object, RouteData = routeData};

            //Create and return the controller
            var controller = DIContainer.Resolve<T>();

            controller.ControllerContext = controllerContextMock;
            
            return controller;
        }

        public static IContainer BuildContainerIoC(params object[] dbObjects)
        {
            var builder = new ContainerBuilder();

            //Create an in-memory version of the database
            if (dbObjects != null && dbObjects.Length > 0)
            {
                builder.RegisterInMemoryTestDatabase(dbObjects);
            }
            else
            {
                Mock<IDataRepository> mockDataRepo = MoqHelpers.CreateMockDataRepository();
                builder.Register(c => mockDataRepo.Object).As<IDataRepository>().InstancePerLifetimeScope();
            }

            //Create the mock repositories
            // BL Repository
            builder.Register(c => new SystemFileRepository()).As<IFileRepository>().SingleInstance();
            builder.RegisterType<UserRepository>().As<UserRepository>().SingleInstance();
            builder.RegisterType<RegistrationRepository>().As<RegistrationRepository>().SingleInstance();
            builder.RegisterType<OrganisationService>().As<OrganisationService>().InstancePerLifetimeScope();
            builder.RegisterType<ReturnService>().As<ReturnService>().InstancePerLifetimeScope();
            builder.RegisterType<DraftReturnService>().As<DraftReturnService>().InstancePerLifetimeScope();

            // BL Services
            builder.RegisterInstance(Config.Configuration);
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();

            builder.RegisterType<ScopeBusinessLogic>().As<ScopeBusinessLogic>().SingleInstance();

            builder.Register(g => new MockGovNotify()).As<IGovNotifyAPI>().SingleInstance();

            builder.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();

            builder.RegisterType<CompareViewService>().As<CompareViewService>().InstancePerLifetimeScope();
            
            builder.RegisterType<AuditLogger>().As<AuditLogger>().SingleInstance();
            builder.RegisterType<AutoCompleteSearchService>().As<AutoCompleteSearchService>().InstancePerLifetimeScope();
            builder.RegisterType<ViewingSearchService>().As<ViewingSearchService>().InstancePerLifetimeScope();
            

            //Register WebTracker
            builder.Register(c => Mock.Of<IWebTracker>()).As<IWebTracker>().InstancePerLifetimeScope();

            //Register all controllers - this is required to ensure KeyFilter is resolved in constructors
            builder.RegisterType<ErrorController>().InstancePerLifetimeScope();
            
            builder.RegisterType<AdminUnconfirmedPinsController>().InstancePerLifetimeScope();
            builder.RegisterType<AdminDatabaseIntegrityChecksController>().InstancePerLifetimeScope();
            builder.RegisterType<CookieController>().InstancePerLifetimeScope();

            builder.Register(c => new MockCache()).As<IDistributedCache>().SingleInstance();
            builder.Register(c => Mock.Of<IHttpContextAccessor>()).As<IHttpContextAccessor>().InstancePerLifetimeScope();

            builder.RegisterType<ActionContextAccessor>().As<IActionContextAccessor>().SingleInstance();
            builder.Register(c => Mock.Of<IUrlHelper>()).SingleInstance();

            builder.RegisterType<EmailSendingService>().As<EmailSendingService>().InstancePerLifetimeScope();

            MockBackgroundJobsApi = new Mock<IBackgroundJobsApi>();
            builder.Register(c => MockBackgroundJobsApi.Object).As<IBackgroundJobsApi>().InstancePerLifetimeScope();

            var environmentMock = new Mock<IHostingEnvironment>();
            environmentMock.SetupGet(m => m.WebRootPath).Returns(Environment.CurrentDirectory);
            builder.RegisterInstance(environmentMock.Object).As<IHostingEnvironment>().SingleInstance();

            IContainer container = builder.Build();

            return container;
        }

        public static void AssertCookieAdded(this Controller controller, string key, string value)
        {
            Mock<IResponseCookies> mockCookies = Mock.Get(controller.HttpContext.Response.Cookies);
            mockCookies.Verify(
                c => c.Append(key, value, It.IsAny<CookieOptions>()),
                Times.Once(),
                $"The cookie '{key}' was not saved with value '{value}'");
        }


        /// <summary>
        ///     Registers an InMemory SQLRespository and populates with entities
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="dbObjects"></param>
        public static void RegisterInMemoryTestDatabase(this ContainerBuilder builder, params object[] dbObjects)
        {
            GpgDatabaseContext dbContext = CreateInMemoryTestDatabase(dbObjects);
            builder.Register(c => new SqlRepository(dbContext)).As<IDataRepository>().InstancePerLifetimeScope();
        }

        public static GpgDatabaseContext CreateInMemoryTestDatabase(params object[] dbObjects)
        {
            //Get the method name of the unit test or the parent
            string testName = TestContext.CurrentContext.Test.FullName;
            if (string.IsNullOrWhiteSpace(testName))
            {
                testName = GetCurrentTestName();
            }

            DbContextOptionsBuilder<GpgDatabaseContext> optionsBuilder =
                new DbContextOptionsBuilder<GpgDatabaseContext>().UseInMemoryDatabase(testName);

            optionsBuilder.ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            // show more detailed EF errors i.e. ReturnId value instead of '{ReturnId}' in the logs etc...
            optionsBuilder.EnableSensitiveDataLogging();

            var dbContext = new GpgDatabaseContext(optionsBuilder.Options);
            if (dbObjects != null && dbObjects.Length > 0)
            {
                foreach (object item in dbObjects)
                {
                    var enumerable = item as IEnumerable<object>;
                    if (enumerable == null)
                    {
                        dbContext.Add(item);
                    }
                    else
                    {
                        dbContext.AddRange(enumerable);
                    }
                }

                dbContext.SaveChanges();
            }

            return dbContext;
        }

        /// <summary>
        ///     Adds a new mock url to the action method of an existing or new Mock URl helper of the controller
        /// </summary>
        /// <param name="controller">The source controller to add the mock setup</param>
        /// <param name="url">The url to be returned by the Action method</param>
        public static void AddMockUriHelperNew(this Controller controller, string url)
        {
            Mock<IUrlHelper> mockUrlHelper = controller.Url?.GetMockFromObject() ?? new Mock<IUrlHelper>();

            mockUrlHelper.Setup(helper => helper.Action(It.IsAny<UrlActionContext>())).Returns(url).Verifiable();

            controller.Url = mockUrlHelper.Object;
        }

        public static string GetCurrentTestName()
        {
            return FindParentMethodWithTestAttribute(MethodBase.GetCurrentMethod()).Name;
        }

        private static MethodBase FindParentMethodWithTestAttribute(MethodBase callingMethod)
        {
            // Iterate throught all attributes
            StackFrame[] frames = new StackTrace().GetFrames();

            for (int i = 1; i < frames.Length; i++)
            {
                StackFrame frame = frames[i];
                if (frame.HasMethod())
                {
                    MethodBase method = frame.GetMethod();

                    if (method.GetCustomAttribute<TestAttribute>() != null)
                    {
                        return method;
                    }
                }
            }

            return callingMethod;
        }

        public static void SetDefaultEncryptionKeys()
        {
            Encryption.SetDefaultEncryptionKey("BA9138B8C0724F168A05482456802405", "45fc394a7f5b29f660bfdf3313224dac");
        }
        
    }
}
