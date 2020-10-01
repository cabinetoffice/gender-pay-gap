using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using Autofac;
using GenderPayGap.Core;
using GenderPayGap.Core.Classes;
using GenderPayGap.Core.Interfaces;
using GenderPayGap.Database;
using GenderPayGap.Extensions;
using GenderPayGap.Extensions.AspNetCore;
using GenderPayGap.Tests.Common.Classes;
using GenderPayGap.Tests.Common.TestHelpers;
using GenderPayGap.WebUI.Areas.Account.Abstractions;
using GenderPayGap.WebUI.Areas.Account.ViewServices;
using GenderPayGap.WebUI.BackgroundJobs;
using GenderPayGap.WebUI.BusinessLogic.Abstractions;
using GenderPayGap.WebUI.BusinessLogic.Services;
using GenderPayGap.WebUI.Classes.Presentation;
using GenderPayGap.WebUI.Classes.Services;
using GenderPayGap.WebUI.Controllers;
using GenderPayGap.WebUI.Controllers.Account;
using GenderPayGap.WebUI.Cookies;
using GenderPayGap.WebUI.ExternalServices;
using GenderPayGap.WebUI.ExternalServices.FileRepositories;
using GenderPayGap.WebUI.Repositories;
using GenderPayGap.WebUI.Search;
using GenderPayGap.WebUI.Services;
using GenderPayGap.WebUI.Tests.Mocks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Primitives;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace GenderPayGap.WebUI.Tests.TestHelpers
{
    public static class NewUiTestHelper
    {
        
        public static IContainer DIContainer;
        public static Mock<IBackgroundJobsApi> MockBackgroundJobsApi;

        private const string Url = "https://localhost/";
        public static Uri Uri => new Uri(Url, UriKind.Absolute);

        public static T GetController<T>(long userId = 0,  Dictionary<string, StringValues> requestFormValues = null, params object[] dbObjects)
        where T: Controller
        {

            DIContainer = BuildContainerIocForControllerOfType<T>(dbObjects);

            //Create Inversion of Control container
            Global.ContainerIoC = DIContainer;
            var mockPrincipal = CreateMockPrincipal(userId, dbObjects);

            //Mock HttpRequest
            var requestMock = new Mock<HttpRequest>();
            var requestForm = new FormCollection(requestFormValues);
            var requestHeaders = new HeaderDictionary();
            var requestCookies = new MockRequestCookieCollection(
                new Dictionary<string, string> {
                    {
                        "cookie_settings",
                        JsonConvert.SerializeObject(
                            new CookieSettings {
                                GoogleAnalyticsGpg = true, GoogleAnalyticsGovUk = true, RememberSettings = true
                            })
                    }
                });
            requestMock.SetupGet(x => x.Cookies).Returns(requestCookies);
            requestMock.SetupGet(x => x.Headers).Returns(requestHeaders);
            requestMock.SetupGet(req => req.Form).Returns(requestForm);

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
            var uri = new UriBuilder(Uri);

            //Mock HttpContext
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(ctx => ctx.User).Returns(mockPrincipal.Object);
            httpContextMock.SetupGet(ctx => ctx.Request.Headers).Returns(requestHeaders);
            httpContextMock.SetupGet(ctx => ctx.Request).Returns(requestMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Request.Cookies).Returns(requestCookies);
            httpContextMock.SetupGet(ctx => ctx.Request.HttpContext).Returns(httpContextMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Request.HasFormContentType).Returns(true);
            httpContextMock.SetupGet(ctx => ctx.Request.Form).Returns(requestForm);
            
            httpContextMock.SetupGet(ctx => ctx.Request.Scheme).Returns(uri.Scheme);
            httpContextMock.SetupGet(ctx => ctx.Request.Host).Returns(new HostString(uri.Host, uri.Port));
            httpContextMock.SetupGet(ctx => ctx.Request.Path).Returns(uri.Path);
            httpContextMock.SetupGet(ctx => ctx.Request.QueryString).Returns(new QueryString(uri.Query));

            httpContextMock.SetupGet(ctx => ctx.Response).Returns(responseMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Response.Cookies).Returns(responseCookies.Object);
            httpContextMock.SetupGet(ctx => ctx.Response.Headers).Returns(responseHeaders);
            httpContextMock.SetupGet(ctx => ctx.Response.HttpContext).Returns(httpContextMock.Object);
            httpContextMock.SetupGet(ctx => ctx.Session).Returns(new MockHttpSession());

            //Mock the httpcontext to the controllercontext
            var routeData = new RouteData();
            var controllerContextMock = new ControllerContext {HttpContext = httpContextMock.Object, RouteData = routeData};
            
            //Mock IHttpContextAccessor
            Mock<IHttpContextAccessor> mockHttpContextAccessor = DIContainer.Resolve<IHttpContextAccessor>().GetMockFromObject();
            mockHttpContextAccessor.SetupGet(a => a.HttpContext).Returns(httpContextMock.Object);

            //Configure the global HttpContext using the mock accessor
            System.Web.HttpContext.Configure(mockHttpContextAccessor.Object);

            //Create and return the controller
            var controller = DIContainer.Resolve<T>();
            controller.ControllerContext = controllerContextMock;
            return controller;
            
        }
        
        public static IContainer BuildContainerIocForControllerOfType<T>(params object[] dbObjects)
        where T : Controller
        {
            var builder = new ContainerBuilder();

            //Create an in-memory version of the database
            if (dbObjects != null && dbObjects.Length > 0)
            {
                builder.RegisterInMemoryTestDatabase(dbObjects);
            }
            else
            {
                Mock<IDataRepository> mockDataRepo = MoqHelpers.CreateMockAsyncDataRepository();
                builder.Register(c => mockDataRepo.Object).As<IDataRepository>().InstancePerLifetimeScope();
            }
            
            //Create the mock repositories
            // BL Repository
            builder.Register(c => new SystemFileRepository()).As<IFileRepository>().InstancePerLifetimeScope();
            
            builder.RegisterType<UserRepository>().As<IUserRepository>().SingleInstance();
            builder.RegisterType<RegistrationRepository>().As<RegistrationRepository>().SingleInstance();
            builder.RegisterType<OrganisationService>().As<OrganisationService>().InstancePerLifetimeScope();
            builder.RegisterType<ReturnService>().As<ReturnService>().InstancePerLifetimeScope();
            builder.RegisterType<DraftReturnService>().As<DraftReturnService>().InstancePerLifetimeScope();
            builder.RegisterType<PinInThePostService>().As<PinInThePostService>().SingleInstance();

            // BL Services
            builder.RegisterInstance(Config.Configuration);
            builder.RegisterType<UpdateFromCompaniesHouseService>().As<UpdateFromCompaniesHouseService>().InstancePerLifetimeScope();
            builder.RegisterType<DraftFileBusinessLogic>().As<IDraftFileBusinessLogic>().InstancePerLifetimeScope();

            builder.Register(
                    c => c.ResolveAsMock<ScopeBusinessLogic>(
                            false,
                            typeof(IDataRepository)
                            )
                        .Object)
                .As<IScopeBusinessLogic>()
                .InstancePerLifetimeScope();

            builder.Register(
                    c => c.ResolveAsMock<SubmissionBusinessLogic>(false, typeof(IDataRepository)).Object)
                .As<ISubmissionBusinessLogic>()
                .InstancePerLifetimeScope();
            builder.RegisterType<OrganisationBusinessLogic>().As<IOrganisationBusinessLogic>().InstancePerLifetimeScope();
            
            builder.Register(g => new MockGovNotify()).As<IGovNotifyAPI>().SingleInstance();
            
            builder.Register(
                    c => new ViewingService(
                        c.Resolve<IDataRepository>(),
                        c.Resolve<ViewingSearchService>())
                )
                .As<IViewingService>()
                .InstancePerLifetimeScope();

            builder.Register(
                    c => new SubmissionService(
                        c.Resolve<IDataRepository>(),
                        c.Resolve<IScopeBusinessLogic>(),
                        c.Resolve<IDraftFileBusinessLogic>()))
                .As<ISubmissionService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<CompareViewService>().As<ICompareViewService>().InstancePerLifetimeScope();
            builder.RegisterType<SearchViewService>().As<ISearchViewService>().InstancePerLifetimeScope();
            builder.RegisterType<ChangePasswordViewService>().As<IChangePasswordViewService>().InstancePerLifetimeScope();
            builder.RegisterType<CloseAccountViewService>().As<CloseAccountViewService>().InstancePerLifetimeScope();

            builder.RegisterType<AuditLogger>().As<AuditLogger>().SingleInstance();
            builder.RegisterType<AutoCompleteSearchService>().As<AutoCompleteSearchService>().InstancePerLifetimeScope();
            builder.RegisterType<ViewingSearchService>().As<ViewingSearchService>().InstancePerLifetimeScope();

            builder.Register(c => Mock.Of<IEncryptionHandler>()).As<IEncryptionHandler>().SingleInstance();
            
            // Register Controller
            builder.RegisterType<T>().InstancePerLifetimeScope();

            builder.Register(c => new MockCache()).As<IDistributedCache>().SingleInstance();
            builder.RegisterType<HttpCache>().As<IHttpCache>().SingleInstance();
            builder.RegisterType<HttpSession>().As<IHttpSession>().InstancePerLifetimeScope();
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
                testName = MethodBase.GetCurrentMethod().FindParentWithAttribute<TestAttribute>().Name;
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
        /// <param name="actionName">The name of the action to mock. If null tries to get from Controller.RoutData.</param>
        /// <param name="controllerName">
        ///     The name of the action to mock. If null tries to get from Controller.RoutData, then uses
        ///     the Controller name.
        /// </param>
        public static void AddMockUriHelperNew(this Controller controller, string url, string actionName = null, string controllerName = null)
        {
            
            if (controllerName != null)
            {
                controllerName = controllerName.BeforeFirst("Controller");
            }

            var uri = new Uri(url);
            if (uri.Authority.EqualsI(Uri.Authority))
            {
                url = uri.PathAndQuery;
            }

            Mock<IUrlHelper> mockUrlHelper = controller.Url?.GetMockFromObject() ?? new Mock<IUrlHelper>();
            Expression<Func<IUrlHelper, string>> urlSetup = helper => helper.Action(
                It.Is<UrlActionContext>(
                    uac => (string.IsNullOrWhiteSpace(actionName) || uac.Action == actionName)
                           && (string.IsNullOrWhiteSpace(controllerName) || uac.Controller == controllerName || uac.Controller == null)));

            mockUrlHelper.Setup(urlSetup).Returns(url).Verifiable();

            controller.Url = mockUrlHelper.Object;
        }


        private static Mock<ClaimsPrincipal> CreateMockPrincipal(long userId = 0, params object[] dbObjects)
        {
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
            }

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(m => m.Claims).Returns(claims);
            mockPrincipal.Setup(m => m.Identity.IsAuthenticated).Returns(userId > 0);
            if (userId > 0)
            {
                mockPrincipal.Setup(m => m.Identity.Name).Returns(userId.ToString());
            }

            return mockPrincipal;
        }
    }
    
}
